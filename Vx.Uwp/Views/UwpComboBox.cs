using ToolsPortable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpComboBox : UwpView<Vx.Views.ComboBox, ComboBox>
    {
        private static long _comboBoxNum;
        public UwpComboBox()
        {
            View.SelectionChanged += View_SelectionChanged;
            View.Name = "ComboBox" + _comboBoxNum;
            _comboBoxNum++;
        }

        private bool _isApplyingProperties;

        private void View_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isApplyingProperties)
            {
                return;
            }

            if (VxView.SelectedItem != null && VxView.SelectedItem.Value != View.SelectedItem)
            {
                VxView.SelectedItem.ValueChanged?.Invoke(View.SelectedItem);
            }
        }

        protected override void ApplyProperties(Vx.Views.ComboBox oldView, Vx.Views.ComboBox newView)
        {
            base.ApplyProperties(oldView, newView);

            _isApplyingProperties = true;
            try
            {
                View.IsEnabled = newView.IsEnabled;
                View.Header = newView.Header;
                View.DataContext = newView.ItemTemplate;

                if (!object.ReferenceEquals(oldView?.Items, newView.Items))
                {
                    View.Items.Clear();
                    if (newView.Items != null)
                    {
                        foreach (object item in newView.Items)
                        {
                            View.Items.Add(item);
                        }
                    }
                }

                if (newView.ItemTemplate != null)
                {
                    if (View.ItemTemplate == null)
                    {
                        View.ItemTemplate = UwpDataTemplateView.GetDataTemplate(View.Name);
                    }
                }
                else
                {
                    if (View.ItemTemplate != null)
                    {
                        View.ItemTemplate = null;
                    }
                }

                View.SelectedItem = newView.SelectedItem?.Value;
            }
            finally
            {
                _isApplyingProperties = false;
            }
        }
    }
}
