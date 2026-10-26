using System;
using CoreGraphics;
using InterfacesiOS.Controllers;
using InterfacesiOS.Helpers;
using ToolsPortable;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSCustomColorPicker : iOSView<CustomColorPicker, ManualLayoutView>
    {
        private const int ValueHeight = 36;
        private const int SwatchSize = 20;

        private UILabel _header;
        private ManualLayoutControl _valueContainer;
        private UIView _swatch;
        private UILabel _value;

        private ModalCustomColorPickerViewController _modalCustomPicker;

        public iOSCustomColorPicker()
        {
            _header = new UILabel();
            View.AddSubview(_header);

            _valueContainer = new ManualLayoutControl
            {
                ClipsToBounds = true,
                BackgroundColor = UIColorCompat.TertiarySystemFillColor
            };
            _valueContainer.Layer.CornerRadius = 10;
            _valueContainer.TouchUpInside += _valueContainer_TouchUpInside;

            _swatch = new UIView { UserInteractionEnabled = false };
            _swatch.Layer.CornerRadius = 4;
            _swatch.Layer.BorderWidth = 1;
            _swatch.Layer.BorderColor = UIColor.Black.CGColor;
            _valueContainer.Add(_swatch);

            _value = new UILabel { Lines = 1 };
            _valueContainer.Add(_value);

            View.AddSubview(_valueContainer);

            View.LayoutAction = LayoutSubviews;
            View.MeasureAction = Measure;
            _valueContainer.LayoutAction = () =>
            {
                var bounds = _valueContainer.Bounds;
                _swatch.Frame = new CGRect(10, (bounds.Height - SwatchSize) / 2, SwatchSize, SwatchSize);
                var valueX = 10 + SwatchSize + 10;
                _value.Frame = new CGRect(valueX, 0, Math.Max(0, bounds.Width - valueX - 10), bounds.Height);
            };
        }

        private CGSize Measure(CGSize availableSize)
        {
            var headerSize = _header.SizeThatFits(new CGSize(availableSize.Width, UIViewWrapper.UnboundedSize));
            var valueSize = _value.SizeThatFits(new CGSize(UIViewWrapper.UnboundedSize, ValueHeight));
            var width = availableSize.Width >= UIViewWrapper.UnboundedSize ? Math.Max(headerSize.Width, valueSize.Width + 20 + SwatchSize + 10) : availableSize.Width;
            return new CGSize(width, headerSize.Height + 4 + ValueHeight);
        }

        private void LayoutSubviews()
        {
            var headerSize = _header.SizeThatFits(new CGSize(View.Bounds.Width, UIViewWrapper.UnboundedSize));
            _header.Frame = new CGRect(0, 0, View.Bounds.Width, headerSize.Height);
            _valueContainer.Frame = new CGRect(0, headerSize.Height + 4, View.Bounds.Width, ValueHeight);
        }

        private async void _valueContainer_TouchUpInside(object sender, EventArgs e)
        {
            if (!VxView.IsEnabled)
            {
                return;
            }

            // Need to wait till previous modal is dismissed before showing this one
            await System.Threading.Tasks.Task.Delay(1);

            if (_modalCustomPicker == null)
            {
                _modalCustomPicker = new ModalCustomColorPickerViewController(VxView.Header, View.GetViewController());
                _modalCustomPicker.OnModalEditSubmitted += new WeakEventHandler(ModalCustomPicker_OnModalEditSubmitted).Handler;
            }
            _modalCustomPicker.ColorPicker.Color = (VxView.Color?.Value ?? default(System.Drawing.Color)).ToUI().CGColor;
            _modalCustomPicker.ShowAsModal();
        }

        private void ModalCustomPicker_OnModalEditSubmitted(object sender, EventArgs e)
        {
            var uiColor = (sender as ModalCustomColorPickerViewController).ColorPicker.Color;
            var color = System.Drawing.Color.FromArgb((int)Math.Round(uiColor.Components[0] * 255), (int)Math.Round(uiColor.Components[1] * 255), (int)Math.Round(uiColor.Components[2] * 255));

            if (VxView.Color != null && VxView.Color.Value != color)
            {
                UpdateColorDisplay(color);
                VxView.Color.ValueChanged?.Invoke(color);
            }
        }

        private void UpdateColorDisplay(System.Drawing.Color color)
        {
            _swatch.BackgroundColor = color.ToUI();
            _value.Text = ColorHelper.ToString(new byte[] { color.R, color.G, color.B }).ToUpper();
        }

        protected override void ApplyProperties(CustomColorPicker oldView, CustomColorPicker newView)
        {
            base.ApplyProperties(oldView, newView);

            _header.Text = newView.Header;
            _valueContainer.Alpha = newView.IsEnabled ? 1f : 0.5f;

            if (newView.Color != null)
            {
                UpdateColorDisplay(newView.Color.Value);
            }

            View.InvalidateIntrinsicContentSize();
            View.SetNeedsLayout();
        }
    }
}
