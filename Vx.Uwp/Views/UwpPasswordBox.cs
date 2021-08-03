using InterfacesUWP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Vx.Uwp.Views
{
    public class UwpPasswordBox : UwpView<Vx.Views.PasswordBox, BarePasswordBox>
    {
        public UwpPasswordBox()
        {
            View.NativePasswordBox.PasswordChanged += View_TextChanged;
            View.NativePasswordBox.LostFocus += NativeTextBox_LostFocus;
            View.NativePasswordBox.GotFocus += NativeTextBox_GotFocus;
            View.Loaded += View_Loaded;
            View.EnterPressed += View_EnterPressed;
        }

        private void View_EnterPressed(object sender, EventArgs e)
        {
            if (VxView.OnSubmit != null)
            {
                VxView.OnSubmit();
            }
        }

        private void View_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (VxView != null && VxView.AutoFocus)
            {
                View.NativePasswordBox.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                View.NativePasswordBox.SelectAll();
            }
        }

        private void NativeTextBox_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (VxView.HasFocusChanged != null)
            {
                VxView.HasFocusChanged(true);
            }
        }

        private void NativeTextBox_LostFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (VxView.HasFocusChanged != null)
            {
                VxView.HasFocusChanged(false);
            }
        }

        private void View_TextChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (VxView.Text != null)
            {
                VxView.Text.Value = View.Password;
            }
        }

        protected override void ApplyProperties(Vx.Views.PasswordBox oldView, Vx.Views.PasswordBox newView)
        {
            base.ApplyProperties(oldView, newView);

            View.Header = newView.Header;
            View.Password = newView.Text?.Value ?? "";
            View.PlaceholderText = newView.PlaceholderText;
            View.ValidationState = newView.ValidationState;
            View.IsEnabled = newView.IsEnabled;
        }
    }
}
