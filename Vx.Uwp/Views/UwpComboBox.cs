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
        public UwpComboBox()
        {
            View.SelectionChanged += View_SelectionChanged;
            View.Name = "ComboBox" + GetHashCode();
        }

        private bool _ignoreSelectionChanged;

        private void View_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ignoreSelectionChanged)
            {
                _ignoreSelectionChanged = false;
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

            View.IsEnabled = newView.IsEnabled;
            View.Header = newView.Header;
            View.DataContext = newView.ItemTemplate;
            // ItemsSource and selected item will be tricky...

            if (!object.ReferenceEquals(oldView?.Items, newView.Items))
            {
                if (View.SelectedItem != null)
                {
                    // Changing the items source while there's currently a selected item will cause the selected item to clear (go to null).
                    // But we need to ignore that change, since we want to apply whatever selected item we'll have.
                    _ignoreSelectionChanged = true;
                }

                View.ItemsSource = newView.Items;
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
    }
}
