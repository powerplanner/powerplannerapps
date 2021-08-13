using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ToolsPortable;
using InterfacesDroid.App;
using BareMvvm.Core.App;
using InterfacesDroid.Windows;
using System.Threading.Tasks;

namespace InterfacesDroid.Extensions
{
    public class AndroidMessageDialog
    {
        private AlertDialog _alertDialog;
        private TaskCompletionSource<bool> _resultSource = new TaskCompletionSource<bool>();

        private AndroidMessageDialog(PortableMessageDialog dialog)
        {
            var builder = new AlertDialog.Builder((PortableApp.Current.GetCurrentWindow().NativeAppWindow as NativeDroidAppWindow).Activity);

            if (!string.IsNullOrWhiteSpace(dialog.Title))
                builder.SetTitle(dialog.Title);

            if (!string.IsNullOrWhiteSpace(dialog.Content))
                builder.SetMessage(dialog.Content);

            if (!string.IsNullOrWhiteSpace(dialog.PositiveText))
                builder.SetPositiveButton(dialog.PositiveText, delegate
                {
                    _resultSource.SetResult(true);
                });

            if (!string.IsNullOrWhiteSpace(dialog.NegativeText))
            {
                builder.SetNegativeButton(dialog.NegativeText, delegate
                {
                    _resultSource.SetResult(false);
                });
            }

            _alertDialog = builder.Create();
        }

        private Task<bool> Show()
        {
            _alertDialog.CancelEvent += _alertDialog_CancelEvent;
            _alertDialog.Show();
            return _resultSource.Task;
        }

        private void _alertDialog_CancelEvent(object sender, EventArgs e)
        {
            _resultSource.SetResult(false);
        }

        public static Task<bool> Show(PortableMessageDialog dialog)
        {
            return new AndroidMessageDialog(dialog).Show();
        }
    }
}