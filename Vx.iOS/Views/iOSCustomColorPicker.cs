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
            _header = new UILabel() { TextColor = Theme.Current.ForegroundColor.ToUI() };
            View.AddSubview(_header);

            _valueContainer = new ManualLayoutControl
            {
                ClipsToBounds = true,
                BackgroundColor = Theme.Current.BackgroundAlt1Color.ToUI()
            };
            _valueContainer.Layer.CornerRadius = 10;
            _valueContainer.TouchUpInside += _valueContainer_TouchUpInside;

            _swatch = new UIView { UserInteractionEnabled = false };
            _swatch.Layer.CornerRadius = SwatchSize / 2;
            _swatch.Layer.BorderWidth = 1;
            _swatch.Layer.BorderColor = Theme.Current.ForegroundColor.ToUI().CGColor;
            _valueContainer.Add(_swatch);

            _value = new UILabel { Lines = 1, TextColor = Theme.Current.ForegroundColor.ToUI() };
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

            var resp = await new ModalCustomColorPickerViewController(View, (VxView.Color?.Value ?? default).ToUI().CGColor).ShowAsync();

            if (resp?.Value != null)
            {
                var uiColor = resp.Value;
                var color = System.Drawing.Color.FromArgb((int)(uiColor.Components[0] * 255), (int)(uiColor.Components[1] * 255), (int)(uiColor.Components[2] * 255));
                UpdateColorDisplay(color);
                VxView.Color?.ValueChanged?.Invoke(color);
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
