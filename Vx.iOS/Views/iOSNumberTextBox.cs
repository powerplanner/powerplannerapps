using System;
using System.Globalization;
using Foundation;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSNumberTextBox : iOSView<NumberTextBox, UIRoundedTextFieldWithHeader>
    {
        private readonly NSNumberFormatter _formatter = new NSNumberFormatter()
        {
            GeneratesDecimalNumbers = true,
            NumberStyle = NSNumberFormatterStyle.Decimal
        };

        public iOSNumberTextBox()
        {
            View.TextChanged += View_TextChanged;
            View.KeyboardType = UIKeyboardType.DecimalPad;
            View.FocusChanged += View_FocusChanged;
        }

        private void View_FocusChanged(object sender, bool focused)
        {
            if (focused)
            {
                View.SelectAll();
            }
        }

        private bool _hasEndingSeparator;

        private void View_TextChanged(object sender, string e)
        {
            _hasEndingSeparator = false;

            if (VxView.Number?.ValueChanged != null)
            {
                var result = _formatter.NumberFromString(View.Text);
                if (result != null)
                {
                    if (View.Text.EndsWith(_formatter.DecimalSeparator))
                    {
                        _hasEndingSeparator = true;
                    }

                    if (result.DoubleValue != VxView.Number.Value)
                    {
                        VxView.Number.ValueChanged(result.DoubleValue);
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
                        View.Text = _formatter.StringFromNumber(newView.Number.Value.Value) + _formatter.DecimalSeparator;
                    }
                    else
                    {
                        View.Text = _formatter.StringFromNumber(newView.Number.Value.Value);
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
