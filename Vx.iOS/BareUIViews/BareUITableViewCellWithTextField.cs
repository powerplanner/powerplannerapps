using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace InterfacesiOS.Views
{
    public class BareUITableViewCellWithTextField : UITableViewCell
    {
        private BareUITextField _textField;
        private UILabel _titleLabel;

        public BareUITableViewCellWithTextField(UITableViewCellStyle cellStyle) : base(cellStyle, null)
        {
            _titleLabel = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredBody
            };

            _textField = new BareUITextField()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.PreferredBody,
                TextColor = UIColor.SecondaryLabel,
                TextAlignment = UITextAlignment.Right
            };

            this.ContentView.AddSubview(_titleLabel);
            this.ContentView.AddSubview(_textField);

            var margins = this.ContentView.LayoutMarginsGuide;

            _titleLabel.LeadingAnchor.ConstraintEqualTo(margins.LeadingAnchor).Active = true;
            _titleLabel.CenterYAnchor.ConstraintEqualTo(margins.CenterYAnchor).Active = true;
            _titleLabel.SetContentHuggingPriority((float)UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Horizontal);
            _titleLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Horizontal);

            _textField.LeadingAnchor.ConstraintEqualTo(_titleLabel.TrailingAnchor, 8).Active = true;
            _textField.TrailingAnchor.ConstraintEqualTo(margins.TrailingAnchor).Active = true;
            _textField.CenterYAnchor.ConstraintEqualTo(margins.CenterYAnchor).Active = true;
        }

        public void SetTitleText(string title)
        {
            _titleLabel.Text = title;
        }

        public BareUITextField TextField => _textField;

        /// <summary>
        /// Do not call this, will throw exception. Use the TextField.
        /// </summary>
        public new UILabel DetailTextLabel
        {
            get => throw new InvalidOperationException("You should not modify the detail text label");
        }
    }
}