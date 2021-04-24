using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Icu.Text;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace InterfacesDroid.Views
{
    public class BareEditDecimalNumber : EditText, ITextWatcher
    {
        public event EventHandler<double?> ValueChanged;

        private char _decimalSeparator;

        public BareEditDecimalNumber(Context context) : base(context)
        {
            Initialize();
        }

        public BareEditDecimalNumber(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        public BareEditDecimalNumber(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialize();
        }

        public BareEditDecimalNumber(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Initialize();
        }

        protected BareEditDecimalNumber(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            // We don't initialize here, for some reason both this and another constructor gets called
        }

        /// <summary>
        /// Note that this code is duplicated in <see cref="BareMaterialEditDecimalNumber"/>
        /// </summary>
        private void Initialize()
        {
            _decimalSeparator = DecimalFormatSymbols.Instance.DecimalSeparator;
            base.KeyListener = DigitsKeyListener.GetInstance("0123456789" + _decimalSeparator);

            base.AddTextChangedListener(this);
        }

        void ITextWatcher.BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            // Nothing
        }

        void ITextWatcher.OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            // Nothing
        }

        void ITextWatcher.AfterTextChanged(IEditable s)
        {
            // Handle removing
            // Any changes made to IEditable cause the method to be called again recursively

            var str = s.ToString();

            // If valid, we're good
            if (str.Length == 0)
            {
                SetValueAndTriggerValueChanged(null);
                return;
            }

            if (double.TryParse(str, out double result))
            {
                SetValueAndTriggerValueChanged(result);
                return;
            }

            // Currently typing out a decimal like ".2", with just the separator value should be 0
            if (str.Length == 1 && str[0] == _decimalSeparator)
            {
                SetValueAndTriggerValueChanged(0);
                return;
            }

            // Valid formats are...
            // 5
            // 65
            // 5.2
            // 65.2
            // 65.0
            // .2
            // . (they're still typing the "2")
            // 5. (they're still typing the "2")

            // Basically, they can only have ONE separator... if they have a second separator, we remove the second one

            int indexOfFirstSeparator = str.IndexOf(_decimalSeparator);
            if (indexOfFirstSeparator != -1)
            {
                int indexOfSubsequentSeparator = str.IndexOf(_decimalSeparator, indexOfFirstSeparator + 1);
                if (indexOfSubsequentSeparator != -1)
                {
                    // After this, AfterTextChanged will be called again recursively so that'll delete even more separators if there were more
                    s.Delete(indexOfSubsequentSeparator, indexOfSubsequentSeparator + 1);
                }
            }
        }

        private double? _value;
        public double? Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    UpdateText();
                }
            }
        }

        private void SetValueAndTriggerValueChanged(double? value)
        {
            if (Value == value)
            {
                return;
            }

            _value = value;
            ValueChanged?.Invoke(this, value);
        }

        private void UpdateText()
        {
            if (Value == null)
            {
                Text = "";
            }
            else
            {
                Text = Value.ToString();
            }
        }
    }
}