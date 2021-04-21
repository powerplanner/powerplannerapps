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
    public class DroidTextBox : DroidView<Vx.Views.TextBox, TextInputLayout>
    {
        private TextInputEditText _editText;

        public DroidTextBox()
        {
            _editText = new TextInputEditText(View.Context);
            View.AddView(_editText, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            _editText.TextChanged += _editText_TextChanged;
        }

        private void _editText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (VxView.Text != null)
            {
                VxView.Text.Value = _editText.Text;
            }
        }

        protected override void ApplyProperties(Vx.Views.TextBox oldView, Vx.Views.TextBox newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.Text != null && _editText.Text != newView.Text.Value)
            {
                _editText.Text = newView.Text.Value;
            }

            _editText.Hint = newView.Header;
        }
    }
}