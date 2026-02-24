using System;
using System.Collections.Generic;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.DataLayer;
using System.Runtime.CompilerServices;
using PowerPlanneriOS.App;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using UIKit;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary;

namespace PowerPlanneriOS.Extensions
{
    public class iOSTelemetryExtension : TelemetryExtension
    {
        private static TelemetryClient _client;

        static iOSTelemetryExtension()
        {
            try
            {
                var config = new TelemetryConfiguration();
                config.TelemetryChannel = new InMemoryChannel();
                config.ConnectionString = Secrets.AppCenterAppSecret;
                _client = new TelemetryClient(config);

                var device = UIDevice.CurrentDevice;

                _client.Context.Component.Version = Variables.VERSION.ToString();
                _client.Context.Device.OperatingSystem = device.SystemName + " " + device.SystemVersion;
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
                switch (UIDevice.CurrentDevice.UserInterfaceIdiom)
                {
                    case UIUserInterfaceIdiom.Phone:
                        return "Phone";

                    case UIUserInterfaceIdiom.Pad:
                        return "Tablet";

                    case UIUserInterfaceIdiom.Mac:
                        return "PC";

                    default:
                        return UIDevice.CurrentDevice.UserInterfaceIdiom.ToString();
                }
            }
            catch { return "Phone"; }
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