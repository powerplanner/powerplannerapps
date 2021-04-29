using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpCheckBox : UwpView<Vx.Views.CheckBox, CheckBox>
    {
        public UwpCheckBox()
        {
            View.Checked += View_Checked;
            View.Unchecked += View_Checked;
        }

        private void View_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            VxView.IsCheckedChanged?.Invoke(View.IsChecked.Value);
        }

        protected override void ApplyProperties(Vx.Views.CheckBox oldView, Vx.Views.CheckBox newView)
        {
            base.ApplyProperties(oldView, newView);

            View.IsChecked = newView.IsChecked;
            View.Content = newView.Text;
            Views.IsEnabled = newView.IsEnabled;
        }
    }
}
