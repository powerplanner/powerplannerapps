using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.TextField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid.Views
{
    public class DroidTextBox : DroidView<Vx.Views.TextBox, TextInputEditText>
    {
        public DroidTextBox() : base(new TextInputEditText(VxDroidExtensions.ApplicationContext))
        {
            View.InputType = Android.Text.InputTypes.TextFlagCapSentences | Android.Text.InputTypes.TextFlagAutoCorrect;
            View.TextChanged += _editText_TextChanged;
        }

        private void _editText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (VxView.Text != null)
            {
                VxView.Text.Value = View.Text;
            }
        }

        protected override void ApplyProperties(Vx.Views.TextBox oldView, Vx.Views.TextBox newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.Text != null && View.Text != newView.Text.Value)
            {
                View.Text = newView.Text.Value;
            }

            View.Hint = newView.Header;
        }
    }
}