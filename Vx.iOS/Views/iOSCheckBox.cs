using System;
using CoreGraphics;
using Foundation;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSCheckBox : iOSView<CheckBox, UIControl>
    {
        private UILabel _label;
        private UIButton _checkbox;
        private bool _isChecked;

        public iOSCheckBox()
        {
            _label = new UILabel()
            {
                Lines = 0,
                Font = UIFont.PreferredBody,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            View.Add(_label);
            _label.StretchHeight(View, 5, 5); // We pad to match how Windows checkboxes are padded

            _checkbox = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _checkbox.SetPreferredSymbolConfiguration(UIImageSymbolConfiguration.Create(UIFont.PreferredBody.PointSize), UIControlState.Normal);
            UpdateCheckboxImage();
            View.Add(_checkbox);
            _checkbox.StretchHeight(View, 5, 5);

            // Idk why, but on the add task page, if the keyboard is up, this doesn't get hit
            // even though on the inline edit controls the same code works. I investigated for 20 mins
            // and couldn't figure it out. It works on the edit schedule times page for some reason.
            View.TouchUpInside += View_TouchUpInside;

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[checkbox]-12-[label]|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null,
                new NSDictionary(
                    "label", _label,
                    "checkbox", _checkbox
                    )));

            _checkbox.TouchUpInside += _checkbox_TouchUpInside;
        }

        private void UpdateCheckboxImage()
        {
            var symbolName = _isChecked ? "checkmark.square.fill" : "square";
            _checkbox.SetImage(UIImage.GetSystemImage(symbolName), UIControlState.Normal);
        }

        private void _checkbox_TouchUpInside(object sender, EventArgs e)
        {
            Toggle();
        }

        private void View_TouchUpInside(object sender, EventArgs e)
        {
            if (!VxView.IsEnabled)
            {
                return;
            }

            Toggle();
        }

        private void Toggle()
        {
            _isChecked = !_isChecked;
            UpdateCheckboxImage();

            if (VxView.IsChecked != null && VxView.IsChecked.Value != _isChecked)
            {
                VxView.IsChecked.ValueChanged?.Invoke(_isChecked);
            }
        }

        protected override void ApplyProperties(CheckBox oldView, CheckBox newView)
        {
            base.ApplyProperties(oldView, newView);

            _label.Text = newView.Text;

            _isChecked = newView.IsChecked?.Value ?? false;
            UpdateCheckboxImage();

            _checkbox.Enabled = newView.IsEnabled;
            _checkbox.Alpha = newView.IsEnabled ? 1.0f : 0.4f;
        }
    }
}
