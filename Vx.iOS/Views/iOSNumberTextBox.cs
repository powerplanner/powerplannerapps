using System;
using System.Globalization;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSNumberTextBox : iOSView<NumberTextBox, UIRoundedTextFieldWithHeader>
    {
        private readonly string _decimalSeparator = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;

        public iOSNumberTextBox()
        {
            View.TextChanged += View_TextChanged;
            View.KeyboardType = UIKeyboardType.DecimalPad;
        }

        private bool _hasEndingSeparator;

        private void View_TextChanged(object sender, string e)
        {
            _hasEndingSeparator = false;

            if (VxView.Number?.ValueChanged != null)
            {
                if (double.TryParse(View.Text, out double result))
                {
                    if (View.Text.EndsWith(_decimalSeparator))
                    {
                        _hasEndingSeparator = true;
                    }

                    if (result != VxView.Number.Value)
                    {
                        VxView.Number.ValueChanged(result);
                    }
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
                    if (_hasEndingSeparator)
                    {
                        View.Text = newView.Number.Value.ToString() + _decimalSeparator;
                    }
                    else
                    {
                        View.Text = newView.Number.Value.ToString();
                    }
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
