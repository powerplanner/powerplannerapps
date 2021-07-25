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
        }

        private void View_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VxView.SelectedItem != View.SelectedItem)
            {
                VxView.SelectedItemChanged(View.SelectedItem);
            }
        }

        protected override void ApplyProperties(Vx.Views.ComboBox oldView, Vx.Views.ComboBox newView)
        {
            base.ApplyProperties(oldView, newView);

            View.IsEnabled = newView.IsEnabled;
            View.Header = newView.Header;
            // ItemsSource and selected item will be tricky...

            if (!object.ReferenceEquals(View.ItemsSource, newView.Items))
            {
                View.ItemsSource = newView.Items;
            }

            View.SelectedItem = newView.SelectedItem;
        }
    }
}
