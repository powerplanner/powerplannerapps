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

        private IOSMessageDialog(PortableMessageDialog dialog)
        {
            var alert = UIAlertController.Create(dialog.Title, dialog.Content, UIAlertControllerStyle.Alert);

            if (dialog.PositiveText != null)
            {
                alert.AddAction(UIAlertAction.Create(dialog.NegativeText ?? "Cancel", UIAlertActionStyle.Cancel, _ => _completionSource.TrySetResult(false)));
                alert.AddAction(UIAlertAction.Create(dialog.PositiveText, UIAlertActionStyle.Default, _ => _completionSource.TrySetResult(true)));
            }
            else
            {
                alert.AddAction(UIAlertAction.Create(dialog.NegativeText ?? "Ok", UIAlertActionStyle.Default, _ => _completionSource.TrySetResult(false)));
            }

            _alert = alert;
        }

        private readonly UIAlertController _alert;

        private Task<bool> Show()
        {
            var viewController = GetTopViewController();
            viewController?.PresentViewController(_alert, true, null);
            return _completionSource.Task;
        }

        private static UIViewController GetTopViewController()
        {
            UIWindow window = null;

            foreach (var scene in UIApplication.SharedApplication.ConnectedScenes)
            {
                if (scene is UIWindowScene windowScene)
                {
                    foreach (var w in windowScene.Windows)
                    {
                        if (w.IsKeyWindow)
                        {
                            window = w;
                            break;
                        }
                    }
                    if (window != null) break;
                }
            }

            if (window == null) return null;

            var vc = window.RootViewController;
            while (vc?.PresentedViewController != null)
            {
                vc = vc.PresentedViewController;
            }
            return vc;
        }

        public static Task<bool> Show(PortableMessageDialog dialog)
        {
            return new IOSMessageDialog(dialog).Show();
        }
    }
}