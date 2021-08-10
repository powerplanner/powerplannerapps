using System;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSNumberTextBox : iOSView<NumberTextBox, UIRoundedTextFieldWithHeader>
    {
        public iOSNumberTextBox()
        {
            View.TextChanged += View_TextChanged;
            View.KeyboardType = UIKeyboardType.DecimalPad;
        }

        private void View_TextChanged(object sender, string e)
        {
            if (VxView.Number?.ValueChanged != null)
            {
                if (double.TryParse(View.Text, out double result))
                {
                    VxView.Number.ValueChanged(result);
                }
                else
                {
                    VxView.Number.ValueChanged(null);
                }
            }
        }

        protected override void ApplyProperties(NumberTextBox oldView, NumberTextBox newView)
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

            View.Placeholder = newView.PlaceholderText;
        }
    }
}
