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

namespace InterfacesDroid.Extensions
{
    public class AndroidMessageDialog
    {
        private PortableMessageDialog _rootDialog;
        private AlertDialog _alertDialog;

        private AndroidMessageDialog(PortableMessageDialog dialog)
        {
            _rootDialog = dialog;

            var builder = new AlertDialog.Builder((PortableApp.Current.GetCurrentWindow().NativeAppWindow as NativeDroidAppWindow).Activity);

            if (!string.IsNullOrWhiteSpace(dialog.Title))
                builder.SetTitle(dialog.Title);

            if (!string.IsNullOrWhiteSpace(dialog.Content))
                builder.SetMessage(dialog.Content);

            _alertDialog = builder.Create();
        }

        private void Show()
        {
            _alertDialog.Show();
        }

        public static void Show(PortableMessageDialog dialog)
        {
            new AndroidMessageDialog(dialog).Show();
        }
    }
}