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
        }

        private void View_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VxView.Number != null)
            {
                if (double.TryParse(View.Text, out double result))
                {
                    VxView.Number.Value = result;
                }
                else
                {
                    VxView.Number.Value = null;
                }
            }
        }

        protected override void ApplyProperties(Vx.Views.NumberTextBox oldView, Vx.Views.NumberTextBox newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.Number != null)
            {
                if (newView.Number.Value != null)
                {
                    View.Text = newView.Number.Value.ToString();
                }
                else
                {
                    View.Text = "";
                }
            }

            View.PlaceholderText = newView.PlaceholderText;
        }
    }
}
