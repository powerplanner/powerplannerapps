using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vx.Droid
{
    public class VxDroidDispatcher : IVxDispatcher
    {
        public Task RunAsync(Action action)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            VxDroidNative.MainActivity.RunOnUiThread(delegate
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                    return;
                }

                taskCompletionSource.SetResult(true);
            });

            return taskCompletionSource.Task;
        }
    }
}