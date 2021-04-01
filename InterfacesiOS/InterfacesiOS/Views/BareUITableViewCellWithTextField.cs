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

        public BareUITableViewCellWithTextField(UITableViewCellStyle cellStyle) : base(cellStyle, null)
        {
            var toReplace = base.DetailTextLabel;
            toReplace.Hidden = true;

            _textField = new BareUITextField()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = toReplace.Font,
                TextColor = toReplace.TextColor,
                TextAlignment = toReplace.TextAlignment,
                AdjustsFontSizeToFitWidth = toReplace.AdjustsFontSizeToFitWidth
            };

            // Impersonate UILabel with an identical UITextField
            // Good example here: https://stackoverflow.com/a/40643511/1454643
            this.ContentView.AddSubview(_textField);

            _textField.LeftAnchor.ConstraintEqualTo(toReplace.LeftAnchor).Active = true;
            _textField.RightAnchor.ConstraintEqualTo(toReplace.RightAnchor).Active = true;
            _textField.TopAnchor.ConstraintEqualTo(toReplace.TopAnchor).Active = true;
            _textField.BottomAnchor.ConstraintEqualTo(toReplace.BottomAnchor).Active = true;

            _textField.AddTarget(delegate
            {
                toReplace.Text = _textField.Text;
            }, UIControlEvent.EditingDidEnd);

            _textField.AddTarget(delegate
            {
                toReplace.Text = _textField.Text;
            }, UIControlEvent.EditingDidEndOnExit);

            // Need KVO since if text set programmatically, need to update
            _textField.AddObserver("Text", default(NSKeyValueObservingOptions), delegate
            {
                toReplace.Text = _textField.Text;
            });
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