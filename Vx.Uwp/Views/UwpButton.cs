using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpButton : UwpView<Vx.Views.Button, Button>
    {
        public UwpButton()
        {
            View.Click += View_Click;
        }

        private void View_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            VxView.Click?.Invoke();
        }

        protected override void ApplyProperties(Vx.Views.Button oldView, Vx.Views.Button newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Content = newView.Text;
        }
    }
}
