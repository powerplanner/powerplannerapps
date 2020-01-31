using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using PowerPlannerAppDataLibrary.Extensions;
using UIKit;

namespace PowerPlanneriOS.Extensions
{
    public class iOSPushExtension : PushExtension
    {
        private static Task<string> _getPushChannelUriTask;
        private static TaskCompletionSource<string> _channelTaskCompletionSource = new TaskCompletionSource<string>();

        public override Task<string> GetPushChannelUri()
        {
            if (_getPushChannelUriTask == null)
            {
                _getPushChannelUriTask = GetPushChannelUriHelper();
            }

            return _getPushChannelUriTask;
        }

        private Task<string> GetPushChannelUriHelper()
        {
            // Available since iOS 8.0+
            // This will start the process, then callback in AppDelegate.cs is triggered, which will call back to us
            // https://developer.apple.com/documentation/usernotifications/registering_your_app_with_apns
            try
            {
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return Task.FromResult<string>(null);
            }

            return _channelTaskCompletionSource.Task;
        }

        public static void RegisteredForRemoteNotifications(string channel)
        {
            System.Diagnostics.Debug.WriteLine("PushChannel: " + channel);
            _channelTaskCompletionSource.SetResult(channel);
        }

        public static void FailedToRegisterForRemoteNotifications(string error)
        {
            TelemetryExtension.Current?.TrackException(new Exception(error));

            _channelTaskCompletionSource.SetResult(null);
        }
    }
}