using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpSwitch : UwpView<Vx.Views.Switch, ToggleSwitch>
    {
        public UwpSwitch()
        {
            View.Toggled += View_Toggled;
        }

        private void View_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (VxView.IsOn != null)
            {
                VxView.IsOn.Value = View.IsOn;
            }
        }

        protected override void ApplyProperties(Switch oldView, Switch newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Header = newView.Title;

            if (newView.IsOn != null)
            {
                View.IsOn = newView.IsOn.Value;
            }

            View.IsEnabled = newView.IsEnabled;
        }
    }
}
