using CoreGraphics;
using InterfacesiOS.Views;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using Vx.iOS.Controllers;

namespace InterfacesiOS.Controllers
{
    public class ModalCustomColorPickerViewController : ImprovedModalEditViewController<BareUICustomColorPicker, CGColor>
    {
        public ModalCustomColorPickerViewController(UIView parentView, CGColor currentColor)
            : base(parentView, currentColor) { }

        protected override BareUICustomColorPicker GenerateControl()
        {
            const double idealWidth = 260;
            const double idealHeight = 260;
            double width;
            double height;
            if (View.Bounds.Width - 20 < idealWidth)
            {
                width = View.Bounds.Width - 20;
            }
            else
            {
                width = idealWidth;
            }
            if (View.Bounds.Height - 20 < idealHeight)
            {
                height = View.Bounds.Height - 20;
            }
            else
            {
                height = idealHeight;
            }

            var picker = new BareUICustomColorPicker(new CGRect(0, 0, width, height));
            return picker;
        }

        protected override void Dismiss()
        {
            Finish(Control.Color);
        }
    }
}
