using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Vx.Views
{
    public class VxEntry : StackLayout
    {
        private Label _label;
        private Entry _entry;

        public string Title
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        private VxState<string> _text;
        public VxState<string> Text
        {
            get => _text;
            set
            {
                if (object.Equals(_text, value))
                {
                    return;
                }

                _text = value;
                _entry.BindText(value);
            }
        }

        public bool IsPassword
        {
            get => _entry.IsPassword;
            set => _entry.IsPassword = value;
        }

        public VxEntry()
        {
            _label = new Label();
            _entry = new Entry();
            //_label.Tap(() => _entry.Focus());

            Children.Add(_label);
            Children.Add(_entry);
        }
    }
}
