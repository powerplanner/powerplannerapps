using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using ToolsPortable;

namespace InterfacesiOS.Extensions
{
    internal class IOSMessageDialog
    {
        private UIAlertView _alertView;

        private IOSMessageDialog(PortableMessageDialog dialog)
        {
            IUIAlertViewDelegate del = null;
            _alertView = new UIAlertView(dialog.Title, dialog.Content, del, "Ok", null);
        }

        private void Show()
        {
            _alertView.Show();
        }

        public static void Show(PortableMessageDialog dialog)
        {
            new IOSMessageDialog(dialog).Show();
        }
    }
}