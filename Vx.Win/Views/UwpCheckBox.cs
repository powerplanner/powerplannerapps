using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpCheckBox : UwpView<Vx.Views.CheckBox, CheckBox>
    {
        public UwpCheckBox()
        {
            View.Checked += View_Checked;
            View.Unchecked += View_Checked;
        }

        private void View_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (VxView.IsChecked != View.IsChecked)
            {
                VxView.IsCheckedChanged?.Invoke(View.IsChecked.Value);
            }
        }

        protected override void ApplyProperties(Vx.Views.CheckBox oldView, Vx.Views.CheckBox newView)
        {
            base.ApplyProperties(oldView, newView);

            View.IsChecked = newView.IsChecked;
            View.Content = newView.Text;
            View.IsEnabled = newView.IsEnabled;
        }
    }
}
