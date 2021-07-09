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

        public DroidTextBox() : base(new TextInputLayout(VxDroidExtensions.ApplicationContext, null, Resource.Attribute.materialOutlinedTextBoxStyle))
        {
            _editText = new TextInputEditText(View.Context)
            {
                LayoutParameters = new TextInputLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                InputType = Android.Text.InputTypes.TextFlagCapSentences | Android.Text.InputTypes.TextFlagAutoCorrect
            };

            View.AddView(_editText);

            _editText.TextChanged += _editText_TextChanged;
        }

        private void _editText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (VxView.Text != null)
            {
                VxView.Text.Value = _editText.Text.ToString();
            }
        }

        protected override void ApplyProperties(Vx.Views.TextBox oldView, Vx.Views.TextBox newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.Text != null && _editText.Text.ToString() != newView.Text.Value)
            {
                _editText.Text = newView.Text.Value;
            }

            View.Hint = newView.Header;
        }
    }
}