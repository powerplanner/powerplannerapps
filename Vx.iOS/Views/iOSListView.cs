using System;
using System.Collections;
using System.Collections.Specialized;
using Foundation;
using ToolsPortable;
using UIKit;
using Vx.Views;
using static Vx.Views.DataTemplateHelper;

namespace Vx.iOS.Views
{
    public class iOSListView : iOSView<Vx.Views.ListView, UITableView>
    {
        public iOSListView()
        {
            View.SeparatorInset = UIEdgeInsets.Zero;
            View.SeparatorStyle = UITableViewCellSeparatorStyle.None;

            // Stretch to full width even on iPad
            View.CellLayoutMarginsFollowReadableWidth = false;

            View.RowHeight = UITableView.AutomaticDimension;
            View.EstimatedRowHeight = 44;

            View.TableFooterView = new UIView(); // Eliminate extra separators on bottom of view

            View.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;

            View.AllowsSelection = false;
        }

        private INotifyCollectionChanged _prevList;
        protected override void ApplyProperties(ListView oldView, ListView newView)
        {
            base.ApplyProperties(oldView, newView);

            var padding = newView.Padding.AsModified();
            View.ContentInset = new UIEdgeInsets(0, padding.Left, padding.Bottom, padding.Right);
            View.TableHeaderView = new UIView(new CoreGraphics.CGRect(0, 0, 0, padding.Top)); // Have to use HeaderView for top padding since the ContentInset won't start scrolled (Insets are meant for a transparent header where the content scrolls underneath).

            if (oldView?.Items != newView.Items || oldView?.ItemTemplate != newView.ItemTemplate)
            {
                if (newView.Items == null || newView.ItemTemplate == null)
                {
                    View.Source = null;
                    View.ReloadData();
                }
                else
                {
                    View.Source = new TableViewSource(newView.Items, newView.ItemTemplate);

                    if (_prevList != newView.Items)
                    {
                        if (_prevList != null)
                        {
                            _prevList.CollectionChanged -= Items_CollectionChanged;
                        }

                        _prevList = newView.Items as INotifyCollectionChanged;
                        if (_prevList != null)
                        {
                            _prevList.CollectionChanged += Items_CollectionChanged;
                        }
                    }

                    View.ReloadData();
                }
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            View.ReloadData();
        }

        private class TableViewSource : UITableViewSource
        {
            private IEnumerable _items;
            private IList _itemsList;
            private Func<object, Vx.Views.View> _itemTemplate;
            private const string CellId = "VxCellId";
            public TableViewSource(IEnumerable items, Func<object, Vx.Views.View> itemTemplate)
            {
                _items = items;
                _itemsList = items as IList;
                _itemTemplate = itemTemplate;
            }

            private object GetItem(int row)
            {
                if (_itemsList != null)
                {
                    return _itemsList[row];
                }

                int i = 0;
                foreach (var item in _items)
                {
                    if (i == row)
                    {
                        return item;
                    }

                    i++;
                }

                throw new IndexOutOfRangeException();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var item = GetItem(indexPath.Row);

                var cell = tableView.DequeueReusableCell(CellId);

                // If no cell, create new one
                if (cell == null)
                {
                    cell = new UITableViewCell(UITableViewCellStyle.Default, CellId);
                    var componentView = new VxDataTemplateComponent
                    {
                        Data = item,
                        Template = _itemTemplate
                    }.Render();
                    componentView.TranslatesAutoresizingMaskIntoConstraints = false;
                    cell.ContentView.AddSubview(componentView);
                    componentView.StretchWidthAndHeight(cell.ContentView);
                    return cell;
                }

                var nativeComponent = (cell.ContentView.Subviews[0] as iOSNativeComponent);
                var component = nativeComponent.Component as VxDataTemplateComponent;
                component.Data = item;
                component.RenderOnDemand(); // Need it to update the views immediately before returning
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (_itemsList != null)
                {
                    return _itemsList.Count;
                }

                int i = 0;
                foreach (var item in _items)
                {
                    i++;
                }
                return i;
            }
        }
    }
}

