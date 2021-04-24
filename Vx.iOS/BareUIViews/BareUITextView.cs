using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace InterfacesiOS.Views
{
    public class BareUITextView : UIView
    {
        public UITextView TextView { get; private set; }
        public UILabel Label { get; private set; }

        public BareUITextView()
        {
            Label = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TextColor = UIColor.LightGray
            };
            this.Add(Label);
            Label.StretchWidth(this, 16, 16);
            Label.PinToTop(this, top: 11);

            TextView = new CustomUITextView(UpdateLabelVisibility)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredBody,
                BackgroundColor = UIColor.Clear
            };
            this.Add(TextView);
            TextView.StretchWidthAndHeight(this, left: 11, right: 11, top: 2, bottom: 2);

            TextView.Changed += TextView_Changed;
            UpdateLabelVisibility();
        }

        private void TextView_Changed(object sender, EventArgs e)
        {
            UpdateLabelVisibility();
        }

        private void UpdateLabelVisibility()
        {
            Label.Hidden = TextView.Text.Length > 0;
        }

        public string Placeholder
        {
            get { return Label.Text; }
            set { Label.Text = value; }
        }

        private class CustomUITextView : UITextView
        {
            private Action _textSetAction;

            public CustomUITextView(Action textSetAction)
            {
                _textSetAction = textSetAction;
            }

            public override string Text
            {
                get { return base.Text; }
                set
                {
                    if (!object.Equals(base.Text, value))
                    {
                        base.Text = value;
                        _textSetAction();
                    }
                }
            }
        }
    }
}