using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text.Method;
using Android.Views;
using Android.Webkit;
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
                LayoutParameters = new TextInputLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent),
                InputType = Android.Text.InputTypes.TextFlagCapSentences | Android.Text.InputTypes.TextFlagAutoCorrect,
                Gravity = GravityFlags.Top
            };

            if (this is DroidMultilineTextBox)
            {
                // Enable scrolling: https://stackoverflow.com/a/24428854
                _editText.ScrollBarStyle = ScrollbarStyles.InsideInset;
                _editText.VerticalScrollBarEnabled = true;
                _editText.OverScrollMode = OverScrollMode.Always;
                _editText.Touch += _editText_Touch;
            }

            View.AddView(_editText);

            _editText.TextChanged += _editText_TextChanged;
            _editText.FocusChange += _editText_FocusChange;
        }

        private void _editText_Touch(object sender, View.TouchEventArgs e)
        {
            if (_editText.HasFocus)
            {
                (sender as View).Parent.RequestDisallowInterceptTouchEvent(true);
                switch (e.Event.Action & MotionEventActions.Mask)
                {
                    case MotionEventActions.Scroll:
                        (sender as View).Parent.RequestDisallowInterceptTouchEvent(false);
                        e.Handled = true;
                        break;
                }
            }

            e.Handled = false;
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
            if (VxView.Text?.ValueChanged != null && _editText.Text.ToString() != VxView.Text.Value)
            {
                VxView.Text.ValueChanged.Invoke(_editText.Text.ToString());
            }
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
                    var inputType = GetInputType(newView.InputScope);

                    if (this is DroidMultilineTextBox)
                    {
                        inputType |= Android.Text.InputTypes.TextFlagMultiLine | Android.Text.InputTypes.ClassText;
                    }

                    _editText.InputType = inputType;
                }
            }

            if (newView.AutoFocus && _firstTime)
            {
                _firstTime = false;
                KeyboardHelper.FocusAndShow(_editText);
            }
        }

        private Android.Text.InputTypes GetInputType(Vx.Views.InputScope inputScope)
{
            switch (inputScope)
            {
                case Vx.Views.InputScope.Email:
                    return Android.Text.InputTypes.TextVariationEmailAddress | Android.Text.InputTypes.TextFlagAutoComplete;

                case Vx.Views.InputScope.Username:
                    return Android.Text.InputTypes.TextVariationNormal | Android.Text.InputTypes.TextFlagAutoComplete;

                case Vx.Views.InputScope.Normal:
                default:
                    return Android.Text.InputTypes.TextFlagCapSentences | Android.Text.InputTypes.TextFlagAutoCorrect | Android.Text.InputTypes.TextFlagAutoComplete;
            }
        }
    }

    public class DroidMultilineTextBox : DroidTextBox
    {
        // Child does all the unique code
    }
}