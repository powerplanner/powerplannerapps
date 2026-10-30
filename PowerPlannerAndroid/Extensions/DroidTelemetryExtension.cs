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

        static DroidTelemetryExtension()
        {
            try
            {
                var config = new TelemetryConfiguration();
                config.TelemetryChannel = new InMemoryChannel();
                config.ConnectionString = Secrets.AppCenterAppSecret;
                _client = new TelemetryClient(config);

                _client.Context.Component.Version = Variables.VERSION.ToString();
                _client.Context.Device.OperatingSystem = "Android " + Build.VERSION.Release;
                _client.Context.Device.Type = GetDeviceType();
                _client.Context.Session.Id = Guid.NewGuid().ToString();
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

        public override void TrackMetric(string metricId, double metricValue)
        {
            try
            {
                _developerLogs.Add("Metric: " + metricId + ":" + metricValue);

                _client.GetMetric(metricId).TrackValue(metricValue);
            }
            catch { }
        }

        public override void TrackMetric(string metricId, double metricValue, string dim1Label, string dim1Value)
        {
            string str = "Metric: " + metricId + ":" + metricValue + ":" + dim1Label + ":" + dim1Value;
            _developerLogs.Add(str);

            _client.GetMetric(metricId, dim1Label).TrackValue(metricValue, dim1Value);
        }

        public override void TrackMetric(string metricId, double metricValue, string dim1Label, string dim1Value, string dim2Label, string dim2Value)
        {
            string str = "Metric: " + metricId + ":" + metricValue + ":" + dim1Label + ":" + dim1Value + ":" + dim2Label + ":" + dim2Value;
            _developerLogs.Add(str);

            _client.GetMetric(metricId, dim1Label, dim2Label).TrackValue(metricValue, dim1Value, dim2Value);
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
        public override async void UpdateCurrentUser(AccountDataItem account)
        {
            base.UpdateCurrentUser(account);

            if (CurrentAccountId != 0)
            {
                _client.Context.User.AccountId = ACCOUNTID_ONLINE;
                _client.Context.User.AuthenticatedUserId = CurrentAccountId.ToString();
            }
            else
            {
                _client.Context.User.AccountId = ACCOUNTID_LOCAL;
                _client.Context.User.AuthenticatedUserId = account != null ? account.LocalAccountId.ToString() : null;
            }

            if (!_hasStartedSession)
            {
                _hasStartedSession = true;

                var ownsInAppPurchase = await PowerPlannerAppDataLibrary.App.PowerPlannerApp.Current.OwnsInAppPurchaseAsync();
                var isPremium = ownsInAppPurchase || (account != null && account.IsLifetimePremiumAccount);

                _client.TrackEvent("StartSessionLog", new Dictionary<string, string>
                {
                    { "IsPremium", isPremium.ToString() },
                    { "OwnsInAppPurchase", ownsInAppPurchase.ToString() },
                    { "IsLifetimePremiumAccount", account != null && account.IsLifetimePremiumAccount ? "true" : "false"  }
                });
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