using System;
using System.Collections.Generic;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using System.Runtime.CompilerServices;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using PowerPlannerAndroid.App;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary;
using Android.OS;
using Android.Content.Res;
using Android.Content.PM;

namespace PowerPlannerAndroid.Extensions
{
    public class DroidTelemetryExtension : TelemetryExtension
    {
        private static TelemetryClient _client;
        private static string _systemId;

        static DroidTelemetryExtension()
        {
            try
            {
                var config = new TelemetryConfiguration();
                config.TelemetryChannel = new InMemoryChannel();
                config.ConnectionString = Secrets.AppCenterAppSecret;
                _client = new TelemetryClient(config);

                _systemId = Settings.DeviceId;

                _client.Context.Component.Version = Variables.VERSION.ToString();
                _client.Context.Device.OperatingSystem = "Android " + Build.VERSION.Release;
                _client.Context.Device.Type = GetDeviceType();
                _client.Context.Session.Id = Guid.NewGuid().ToString();
                // client.Context.Device.Id doesn't work (doesn't send up anything).
                _client.Context.GlobalProperties.Add("Device Id", _systemId);
            }
            catch
            {

            }
        }

        private static string GetDeviceType()
        {
            try
            {
                var context = Android.App.Application.Context;

                if (IsChromebook())
                {
                    return "PC";
                }

                var metrics = context.Resources.DisplayMetrics;
                float yInches = metrics.HeightPixels / metrics.Ydpi;
                float xInches = metrics.WidthPixels / metrics.Xdpi;
                double diagonalInches = Math.Sqrt(xInches * xInches + yInches * yInches);

                if (diagonalInches >= 7)
                {
                    return "Tablet";
                }
                else
                {
                    return "Phone";
                }
            }
            catch { return "Phone"; }
        }

        private static bool IsChromebook()
        {
            try
            {
                var context = Android.App.Application.Context;

                bool isChromeOS = context.PackageManager.HasSystemFeature("org.chromium.arc.device_management");

                return isChromeOS;
            }
            catch { return false; }
        }

        public override void TrackEvent(string eventName, IDictionary<string, string> properties = null)
        {
            try
            {
                _developerLogs.Add(EventToString(eventName, properties));

                _client.TrackEvent(eventName, properties);
            }
            catch { }
        }

        private string _lastPageName;
        public override string LastPageName => _lastPageName;
        public override void TrackPageVisited(string pageName)
        {
            try
            {
                _lastPageName = pageName;

                _client.TrackPageView(pageName);
            }
            catch { }
        }

        public override void TrackException(Exception ex, [CallerMemberName] string exceptionName = null, IDictionary<string, string> properties = null)
        {
            try
            {
                _developerLogs.Add(ExceptionToString(ex, exceptionName, properties));

                _client.TrackException(ex, properties);
            }
            catch { }
        }

        private bool _hasStartedSession;
        public override void UpdateCurrentUser(AccountDataItem account)
        {
            base.UpdateCurrentUser(account);

            // Represents a local user identity. If they use two local accounts, that means there's two users, which is typically correct.
            _client.Context.User.Id = account != null ? account.LocalAccountId.ToString() : null;

            // Represents an online user identity.
            _client.Context.User.AuthenticatedUserId = CurrentAccountId == 0 ? null : CurrentAccountId.ToString();

            // Do NOT use User.AccountId, that's meant for things like which tenant.

            if (!_hasStartedSession)
            {
                _hasStartedSession = true;

                _client.TrackEvent("StartSessionLog");
            }
        }

        public override void SuspendingApp()
        {
            try
            {
                _client.Flush();
            }
            catch { }
        }

        private List<string> _developerLogs = new List<string>();
        public override string GetDeveloperLogs()
        {
            return string.Join("\n", _developerLogs);
        }
    }
}