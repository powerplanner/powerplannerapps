using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using PowerPlannerAppDataLibrary.DataLayer;
using System.Runtime.CompilerServices;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using PowerPlannerAppDataLibrary;
using InterfacesUWP;
using Microsoft.ApplicationInsights.DataContracts;
using Windows.System.Profile;
using Windows.Security.Cryptography;
using Microsoft.ApplicationInsights.Channel;

namespace PowerPlannerUWP.Extensions
{
    public class UWPTelemetryExtension : TelemetryExtension
    {
        private static TelemetryClient _client;
        private static string _systemId;
        private static IOperationHolder<RequestTelemetry> _session;

        static UWPTelemetryExtension()
        {
            try
            {
                var config = new TelemetryConfiguration();
                config.TelemetryChannel = new InMemoryChannel();
                config.ConnectionString = Secrets.AppCenterAppSecret;
                _client = new TelemetryClient(config);

                var id = SystemIdentification.GetSystemIdForPublisher();
                if (id.Id != null)
                {
                    _systemId = CryptographicBuffer.EncodeToBase64String(id.Id);
                }
                else
                {
                    _systemId = "notAvailable";
                }

                _client.Context.Component.Version = Variables.VERSION.ToString();
                _client.Context.Device.OperatingSystem = "Windows 10.0." + DeviceInfo.BuildNumber;
                _client.Context.Session.Id = Guid.NewGuid().ToString();
                // client.Context.Device.Id doesn't work (doesn't send up anything).
                _client.Context.GlobalProperties.Add("Device Id", _systemId);
            }
            catch
            {

            }
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

        public override void ReturnedToApp()
        {
            // Don't track another pageview
        }

        private List<string> _developerLogs = new List<string>();
        public override string GetDeveloperLogs()
        {
            return string.Join("\n", _developerLogs);
        }
    }
}
