using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

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
            View.IsEnabled = newView.IsEnabled;
        }
    }

    public class UwpDestructiveButton : UwpButton
    {
        public UwpDestructiveButton()
        {
            View.Style = Application.Current.Resources["DestructiveButtonStyle"] as Style;
        }
    }

    public class UwpAccentButton : UwpButton
    {
        public UwpAccentButton()
        {
            View.Style = Application.Current.Resources["AccentButtonStyle"] as Style;
        }
    }

    public class UwpTextButton : UwpView<Vx.Views.TextButton, HyperlinkButton>
    {
        private TextBlock _tb;
        public UwpTextButton()
        {
            var tb = new TextBlock();
            View.Content = tb;
            _tb = tb;

            View.Click += View_Click;
            View.Padding = new Thickness();
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            VxView.Click?.Invoke();
        }

        protected override void ApplyProperties(Vx.Views.TextButton oldView, Vx.Views.TextButton newView)
        {
            base.ApplyProperties(oldView, newView);

            _tb.Text = newView.Text;
            View.IsEnabled = newView.IsEnabled;
        }
    }
}
