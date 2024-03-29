using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ToolsPortable;
using BareMvvm.Core.App;
using InterfacesDroid.Windows;

namespace InterfacesDroid.App
{
    public class AndroidDispatcher : PortableDispatcher
    {
        public override Task RunAsync(Action codeToExecute)
        {
            NativeDroidAppWindow window = (NativeDroidAppWindow)PortableApp.Current?.GetCurrentWindow()?.NativeAppWindow;

            if (window == null)
            {
                try
                {
                    codeToExecute();
                }
                catch { }
            }
            else
            {
                // Post method will queue up events if on the current UI thread, whereas the runOnUiThread would run immediately.
                window.Activity.FindViewById(Android.Resource.Id.Content).Post(codeToExecute);
            }
            
            return Task.FromResult(true);
        }
    }
}