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
            View.Name = "ComboBox" + DateTime.Now.Ticks.ToString();
        }

        private void View_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
