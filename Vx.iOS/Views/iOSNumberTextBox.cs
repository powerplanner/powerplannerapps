using System;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSNumberTextBox : iOSView<NumberTextBox, UITextFieldWithUnderline>
    {
        public iOSNumberTextBox()
        {
            View.EditingChanged += View_EditingDidEnd;
            View.EditingDidEnd += View_EditingDidEnd;
            View.EditingDidEndOnExit += View_EditingDidEnd;
            View.TextColor = Theme.Current.ForegroundColor.ToUI();
            View.SetHeight(34);
            View.KeyboardType = UIKeyboardType.DecimalPad;
        }

        private void View_EditingDidEnd(object sender, EventArgs e)
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
