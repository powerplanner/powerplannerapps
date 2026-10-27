using System;
using CoreGraphics;
using InterfacesiOS.Controllers;
using InterfacesiOS.Helpers;
using ToolsPortable;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSColorPicker : iOSView<ColorPicker, ManualLayoutView>
    {
        private InternalColorPickerComponent _internalColorPickerComponent;
        private UIView _rendered;

        public iOSColorPicker()
        {
            _internalColorPickerComponent = new InternalColorPickerComponent()
            {
                PickCustomColor = PickCustomColor,
                ColorChanged = c => VxView?.Color?.ValueChanged?.Invoke(c)
            };
            _rendered = _internalColorPickerComponent.Render();
            View.AddSubview(_rendered);

            View.MeasureAction = Measure;
            View.LayoutAction = LayoutSubviews;
        }

        private CGSize Measure(CGSize availableSize)
        {
            return _rendered.SizeThatFits(availableSize);
        }

        private void LayoutSubviews()
        {
            _rendered.Frame = View.Bounds;
        }

        protected override void ApplyProperties(ColorPicker oldView, ColorPicker newView)
        {
            base.ApplyProperties(oldView, newView);

            _internalColorPickerComponent.Header = newView.Header;
            _internalColorPickerComponent.IsEnabled = newView.IsEnabled;
            _internalColorPickerComponent.Color = newView.Color?.Value ?? default;

            View.InvalidateIntrinsicContentSize();
            View.SetNeedsLayout();
        }

        private void PickCustomColor()
        {
            ShowCustomColorPicker();
        }

        private async void ShowCustomColorPicker()
        {
            // Need to wait till previous modal is dismissed before showing this one
            await System.Threading.Tasks.Task.Delay(1);

            var resp = await new ModalCustomColorPickerViewController(View, (VxView.Color?.Value ?? default).ToUI().CGColor).ShowAsync();

            if (resp?.Value != null)
            {
                var uiColor = resp.Value;
                var color = System.Drawing.Color.FromArgb((int)(uiColor.Components[0] * 255), (int)(uiColor.Components[1] * 255), (int)(uiColor.Components[2] * 255));
                _internalColorPickerComponent.Color = color;
                VxView.Color?.ValueChanged?.Invoke(color);
            }
        }
    }
}
