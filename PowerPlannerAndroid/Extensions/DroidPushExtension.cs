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
using PowerPlannerAppDataLibrary.Extensions;
using Android.Gms.Common;
using Firebase.Messaging;
using Android.Gms.Tasks;

namespace PowerPlannerAndroid.Extensions
{
    public class DroidPushExtension : PushExtension
    {
        private bool _tooManyRegistrations = false;
        public override async Task<string> GetPushChannelUri()
        {
            if (_tooManyRegistrations)
            {
                return null;
            }

            try
            {
                // If Google Play services available
                if (GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(Application.Context) == ConnectionResult.Success)
                {
                    string token;
                    try
                    {
                        token = (await GmsTaskExtensions.ToAwaitableTask(FirebaseMessaging.Instance.GetToken()))?.ToString();
                    }
                    catch (Java.IO.IOException ex) when (ex.Message != null && ex.Message.Contains("TOO_MANY_REGISTRATIONS"))
                    {
                        // TOO_MANY_REGISTRATIONS means too many stale tokens have accumulated.
                        // This is probably just occurring on Firebase test devices. There's no resolution other than devce hard reset.
                        // See https://github.com/firebase/firebase-android-sdk/issues/6958 for more info.
                        TelemetryExtension.Current?.TrackEvent("Error_PushChannel_TooManyRegistrations");
                        token = null;
                        _tooManyRegistrations = true;
                    }

                    System.Diagnostics.Debug.WriteLine("FirebaseToken: " + token);
                    return token;
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            return null;
        }
    }

    public static class GmsTaskExtensions
    {
        public static Task<Java.Lang.Object> ToAwaitableTask(this Android.Gms.Tasks.Task task)
        {
            var taskCompletionSource = new TaskCompletionSource<Java.Lang.Object>();
            var taskCompleteListener = new TaskCompleteListener(taskCompletionSource);
            task.AddOnCompleteListener(taskCompleteListener);

            return taskCompletionSource.Task;
        }

        private class TaskCompleteListener : Java.Lang.Object, IOnCompleteListener
        {
            private readonly TaskCompletionSource<Java.Lang.Object> taskCompletionSource;

            public TaskCompleteListener(TaskCompletionSource<Java.Lang.Object> taskCompletionSource)
            {
                this.taskCompletionSource = taskCompletionSource;
            }

            public void OnComplete(Android.Gms.Tasks.Task task)
            {
                if (task.IsCanceled)
                {
                    this.taskCompletionSource.SetCanceled();
                }
                else if (task.IsSuccessful)
                {
                    this.taskCompletionSource.SetResult(task.Result);
                }
                else
                {
                    this.taskCompletionSource.SetException(task.Exception);
                }
            }
        }
    }
}