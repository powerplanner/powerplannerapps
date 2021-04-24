using CoreGraphics;
using InterfacesiOS.Views;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace InterfacesiOS.Controllers
{
    public class ModalCustomColorPickerViewController : ModalEditViewController
    {
        public BareUICustomColorPicker ColorPicker => ContentView as BareUICustomColorPicker;

        public ModalCustomColorPickerViewController(string headerText, UIViewController parent)
            : base(CreateCustomColorPicker(), headerText, parent) { }

        private static BareUICustomColorPicker CreateCustomColorPicker()
        {
            var picker = new BareUICustomColorPicker(new CGRect(0, 0, 50, 260));
            return picker;
        }
    }
}
