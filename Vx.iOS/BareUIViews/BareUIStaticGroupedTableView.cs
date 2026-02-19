using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace InterfacesiOS.Views
{
    public class BareUIStaticGroupedTableView : UITableView
    {
        private StaticViewSource _staticViewSource;

        public BareUIStaticGroupedTableView() : base(new CoreGraphics.CGRect(), UITableViewStyle.Grouped)
        {
            _staticViewSource = new StaticViewSource(this);
        }

        public void ClearAll()
        {
            _staticViewSource?.ClearAll();
        }

        private static void ConfigureCellContent(UITableViewCell cell, string text, string secondaryText = null, UIFont font = null, UIColor textColor = null, int numberOfLines = 1, int secondaryNumberOfLines = 1)
        {
            var content = UIListContentConfiguration.CellConfiguration;
            content.Text = text;
            if (secondaryText != null)
                content.SecondaryText = secondaryText;
            if (font != null)
                content.TextProperties.Font = font;
            if (textColor != null)
                content.TextProperties.Color = textColor;
            content.TextProperties.NumberOfLines = numberOfLines;
            if (secondaryNumberOfLines != 1)
                content.SecondaryTextProperties.NumberOfLines = secondaryNumberOfLines;
            cell.ContentConfiguration = content;
        }

        private static void ConfigureValueCellContent(UITableViewCell cell, string text)
        {
            var content = UIListContentConfiguration.ValueCellConfiguration;
            content.Text = text;
            cell.ContentConfiguration = content;
        }

        private static void ConfigureSubtitleCellContent(UITableViewCell cell, string text, string secondaryText, int secondaryNumberOfLines = 1)
        {
            var content = UIListContentConfiguration.SubtitleCellConfiguration;
            content.Text = text;
            content.SecondaryText = secondaryText;
            content.SecondaryTextProperties.NumberOfLines = secondaryNumberOfLines;
            cell.ContentConfiguration = content;
        }

        public void AddDescriptionCell(string title)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Default, "TableCell");
            ConfigureCellContent(cell, title, numberOfLines: 0);
            AddCell(cell, null);
        }

        public void AddCaptionDescriptionCell(string title)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Default, "TableCell");
            ConfigureCellContent(cell, title, font: UIFont.PreferredCaption1, textColor: UIColor.LightGray, numberOfLines: 0);
            AddCell(cell, null);
        }

        public void AddCell(string title, Action invokeAction)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Default, "TableCell");
            ConfigureCellContent(cell, title);
            AddCell(cell, invokeAction);
        }

        public void AddCell(UITableViewCell cell, Action invokeAction)
        {
            _staticViewSource.AddCell(cell, invokeAction);
        }

        public void AddValueCell(string title, Binding.BindingHost bindingHost, string bindingValuePropertyName, Func<object, string> converter, Action invokeAction)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Value1, "Value1Cell");
            ConfigureValueCellContent(cell, title);

            // Use binding to update the secondary text via content configuration
            bindingHost.SetBinding(bindingValuePropertyName, value =>
            {
                string valueText;
                if (converter != null)
                    valueText = converter.Invoke(value);
                else if (value is string valueStr)
                    valueText = valueStr;
                else if (value == null)
                    valueText = "";
                else
                    valueText = value.ToString();

                var content = UIListContentConfiguration.ValueCellConfiguration;
                content.Text = title;
                content.SecondaryText = valueText;
                cell.ContentConfiguration = content;
            });

            AddCell(cell, invokeAction);
        }

        public void AddTextFieldCell(string title, Binding.BindingHost bindingHost, string bindingValuePropertyName, Action<BareUITextField> customizeTextField = null)
        {
            var cell = new BareUITableViewCellWithTextField(UITableViewCellStyle.Value1);
            cell.SetTitleText(title);
            bindingHost.SetTextFieldBinding(cell.TextField, bindingValuePropertyName);

            customizeTextField?.Invoke(cell.TextField);
        }

        /// <summary>
        /// Two-way binding
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="bindingHost"></param>
        /// <param name="bindingValuePropertyName"></param>
        public void AddCheckableCellWithDescription(string title, string description, Binding.BindingHost bindingHost, string bindingValuePropertyName)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Subtitle, "CheckableDescriptionCell");
            ConfigureSubtitleCellContent(cell, title, description, secondaryNumberOfLines: 0);
            bindingHost.SetIsCheckedBinding(cell, bindingValuePropertyName);
            AddCell(cell, delegate
            {
                try
                {
                    var type = bindingHost.DataContext.GetType();
                    var prop = type.GetProperty(bindingValuePropertyName);
                    prop.SetValue(bindingHost.DataContext, true);
                }
                catch { }
            });
        }

        public void StartNewGroup()
        {
            _staticViewSource.StartNewGroup();
        }

        public void Compile()
        {
            if (Source == null)
            {
                Source = _staticViewSource;
            }
            ReloadData();
        }

        private class StaticViewSource : UITableViewSource
        {
            private List<UITableViewCell> _cells = new List<UITableViewCell>();
            private List<Action> _actions = new List<Action>();
            private UITableView _tableView;

            public StaticViewSource(UITableView tableView)
            {
                _tableView = tableView;
            }

            private List<List<CellAndAction>> _groups = new List<List<CellAndAction>>()
            {
                new List<CellAndAction>()
            };

            public void ClearAll()
            {
                _groups = new List<List<CellAndAction>>()
                {
                    new List<CellAndAction>()
                };
            }

            public void AddCell(UITableViewCell cell, Action invokeAction)
            {
                if (invokeAction == null)
                {
                    cell.UserInteractionEnabled = false;
                }

                _groups.Last().Add(new CellAndAction()
                {
                    Cell = cell,
                    Action = invokeAction
                });
            }

            public void StartNewGroup()
            {
                _groups.Add(new List<CellAndAction>());
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                return _groups[indexPath.Section][indexPath.Row].Cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return _groups[(int)section].Count;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return _groups.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (indexPath.Section >= _groups.Count)
                {
                    return;
                }

                var group = _groups[indexPath.Section];

                if (indexPath.Row >= group.Count)
                {
                    return;
                }

                group[indexPath.Row].Action?.Invoke();

                // Immediately unselect it
                _tableView.SelectRow(null, true, UITableViewScrollPosition.None);
            }

            private class CellAndAction
            {
                public UITableViewCell Cell { get; set; }
                public Action Action { get; set; }
            }
        }
    }
}