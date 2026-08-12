using System;
using CoreGraphics;
using Foundation;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class UICheckBoxControl : ManualLayoutControl
    {
    }

    public class iOSCheckBox : iOSView<CheckBox, UICheckBoxControl>
    {
        private UILabel _label;
        private UIButton _checkbox;
        private bool _isChecked;

        public iOSCheckBox()
        {
            _label = new UILabel()
            {
                Lines = 0,
                Font = UIFont.PreferredBody
            };
            View.Add(_label);

            _checkbox = new UIButton(UIButtonType.System);
            _checkbox.SetPreferredSymbolConfiguration(UIImageSymbolConfiguration.Create(UIFont.PreferredBody.PointSize), UIControlState.Normal);
            UpdateCheckboxImage();
            View.Add(_checkbox);

            View.LayoutAction = LayoutSubviews;
            View.MeasureAction = Measure;

            // Idk why, but on the add task page, if the keyboard is up, this doesn't get hit
            // even though on the inline edit controls the same code works. I investigated for 20 mins
            // and couldn't figure it out. It works on the edit schedule times page for some reason.
            View.TouchUpInside += View_TouchUpInside;

            _checkbox.TouchUpInside += _checkbox_TouchUpInside;
        }

        private CGSize Measure(CGSize availableSize)
        {
            var checkboxSize = _checkbox.SizeThatFits(availableSize);
            var labelSize = _label.SizeThatFits(new CGSize(Math.Max(0, availableSize.Width - checkboxSize.Width - 12), availableSize.Height));
            return new CGSize(checkboxSize.Width + 12 + labelSize.Width, Math.Max(checkboxSize.Height, labelSize.Height) + 10);
        }

        private void LayoutSubviews()
        {
            var checkboxSize = _checkbox.SizeThatFits(View.Bounds.Size);
            _checkbox.Frame = new CGRect(0, (View.Bounds.Height - checkboxSize.Height) / 2, checkboxSize.Width, checkboxSize.Height);
            _label.Frame = new CGRect(checkboxSize.Width + 12, 5, Math.Max(0, View.Bounds.Width - checkboxSize.Width - 12), Math.Max(0, View.Bounds.Height - 10));
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
