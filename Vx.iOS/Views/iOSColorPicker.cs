using System;
using InterfacesiOS.Controllers;
using InterfacesiOS.Helpers;
using ToolsPortable;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSColorPicker : iOSView<ColorPicker, UIView>
    {
        private InternalColorPickerComponent _internalColorPickerComponent;

        public iOSColorPicker()
        {
            _internalColorPickerComponent = new InternalColorPickerComponent()
            {
                PickCustomColor = PickCustomColor,
                ColorChanged = c => VxView?.Color?.ValueChanged?.Invoke(c)
            };
            var rendered = _internalColorPickerComponent.Render();
            rendered.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(rendered);
            rendered.StretchWidthAndHeight(View);
        }

        protected override void ApplyProperties(ColorPicker oldView, ColorPicker newView)
        {
            base.ApplyProperties(oldView, newView);

            _internalColorPickerComponent.Header = newView.Header;
            _internalColorPickerComponent.IsEnabled = newView.IsEnabled;
            _internalColorPickerComponent.Color = newView.Color?.Value ?? default(System.Drawing.Color);
        }

        private void PickCustomColor()
        {
            ShowCustomColorPicker();
        }

        private ModalCustomColorPickerViewController m_modalCustomPicker;
        private async void ShowCustomColorPicker()
        {
            // Need to wait till previous modal is dismissed before showing this one
            await System.Threading.Tasks.Task.Delay(1);

            if (m_modalCustomPicker == null)
            {
                m_modalCustomPicker = new ModalCustomColorPickerViewController(VxView.Header, View.GetViewController());
                m_modalCustomPicker.OnModalEditSubmitted += new WeakEventHandler(ModalCustomPicker_OnModalEditSubmitted).Handler;
            }
            m_modalCustomPicker.ColorPicker.Color = (VxView.Color?.Value ?? default(System.Drawing.Color)).ToUI().CGColor;
            m_modalCustomPicker.ShowAsModal();
        }

        private void ModalCustomPicker_OnModalEditSubmitted(object sender, EventArgs e)
        {
            var uiColor = (sender as ModalCustomColorPickerViewController).ColorPicker.Color;
            var color = System.Drawing.Color.FromArgb((int)(uiColor.Components[0] * 255), (int)(uiColor.Components[1] * 255), (int)(uiColor.Components[2] * 255));
            _internalColorPickerComponent.Color = color;
            VxView.Color?.ValueChanged?.Invoke(color);
        }
    }
}
