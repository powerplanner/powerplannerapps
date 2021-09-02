using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Vx.Uwp.Views
{
    public class UwpNumberTextBox : UwpView<Vx.Views.NumberTextBox, TextBox>
    {
        public UwpNumberTextBox()
        {
            View.TextChanged += View_TextChanged;

            View.InputScope = new InputScope()
            {
                Names =
                {
                    new InputScopeName(InputScopeNameValue.Number)
                }
            };

            View.GotFocus += View_GotFocus;
        }

        private void View_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            View.SelectAll();
        }

        private void View_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VxView.Number?.ValueChanged != null)
            {
                if (double.TryParse(View.Text, out double result))
                {
                    if (VxView.Number.Value != result)
                    {
                        VxView.Number.ValueChanged(result);
                    }
                }
                else
                {
                    if (VxView.Number.Value != null)
                    {
                        VxView.Number.ValueChanged(null);
                    }
                }
            }
        }

        protected override void ApplyProperties(Vx.Views.NumberTextBox oldView, Vx.Views.NumberTextBox newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.Number != null)
            {
                // This is to ensure that when user goes from "1.5" to "1." (deletes the end), it doesn't change it to 1
                if (!double.TryParse(View.Text, out double curr) || curr != newView.Number.Value)
                {
                    View.Text = newView.Number.Value.ToString();
                }
            }
            else
            {
                View.Text = "";
            }

            View.PlaceholderText = newView.PlaceholderText;
            View.IsEnabled = newView.IsEnabled;
            View.Header = newView.Header;
        }
    }
}
