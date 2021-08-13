using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using Google.Android.Material.TextField;
using InterfacesDroid.Helpers;
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
            _editText.FocusChange += _editText_FocusChange;
        }

        private void _editText_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (VxView.HasFocusChanged != null)
            {
                VxView.HasFocusChanged(e.HasFocus);
            }
        }

        private void _editText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            VxView.Text?.ValueChanged?.Invoke(_editText.Text.ToString());
        }

        private bool _firstTime = true;

        protected override void ApplyProperties(Vx.Views.TextBox oldView, Vx.Views.TextBox newView)
        {
            base.ApplyProperties(oldView, newView);

            if (newView.Text != null && _editText.Text.ToString() != newView.Text.Value)
            {
                _editText.Text = newView.Text.Value;
            }

            _editText.Error = newView.ValidationState?.ErrorMessage;

            View.Hint = newView.Header;
            View.Enabled = newView.IsEnabled;

            if (newView is Vx.Views.PasswordBox && oldView == null)
            {
                // Only need to set it for first time
                _editText.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword;
            }
            else
            {
                if (oldView == null || oldView.InputScope != newView.InputScope)
                {
                    switch (newView.InputScope)
                    {
                        case Vx.Views.InputScope.Normal:
                            _editText.InputType = Android.Text.InputTypes.TextFlagCapSentences | Android.Text.InputTypes.TextFlagAutoCorrect | Android.Text.InputTypes.TextFlagAutoComplete;
                            break;

                        case Vx.Views.InputScope.Email:
                            _editText.InputType = Android.Text.InputTypes.TextVariationEmailAddress | Android.Text.InputTypes.TextFlagAutoComplete;
                            break;

                        case Vx.Views.InputScope.Username:
                            _editText.InputType = Android.Text.InputTypes.TextVariationNormal | Android.Text.InputTypes.TextFlagAutoComplete;
                            break;
                    }
                }
            }

            if (newView.AutoFocus && _firstTime)
            {
                _firstTime = false;
                KeyboardHelper.FocusAndShow(_editText);
            }
        }
    }
}