using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using ToolsPortable;
using System.Threading.Tasks;

namespace InterfacesiOS.Extensions
{
    internal class IOSMessageDialog
    {
        private TaskCompletionSource<bool> _completionSource = new TaskCompletionSource<bool>();
        private UIAlertView _alertView;

        private IOSMessageDialog(PortableMessageDialog dialog)
        {
            IUIAlertViewDelegate del = null;
            if (dialog.PositiveText != null)
            {
                _alertView = new UIAlertView(dialog.Title, dialog.Content, del, dialog.NegativeText ?? "Cancel", dialog.PositiveText);
            }
            else
            {
                _alertView = new UIAlertView(dialog.Title, dialog.Content, del, dialog.NegativeText ?? "Ok");
            }
            _alertView.Clicked += _alertView_Clicked;
        }

        private void _alertView_Clicked(object sender, UIButtonEventArgs e)
        {
            _completionSource.SetResult(e.ButtonIndex == 1);
        }

        private Task<bool> Show()
        {
            _alertView.Show();
            return _completionSource.Task;
        }

        public static Task<bool> Show(PortableMessageDialog dialog)
        {
            return new IOSMessageDialog(dialog).Show();
        }
    }
}