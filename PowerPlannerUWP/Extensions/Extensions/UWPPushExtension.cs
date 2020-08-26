using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Extensions.Telemetry;
using PowerPlannerAppDataLibrary.SyncLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.Networking.PushNotifications;

namespace PowerPlannerUWPLibrary.Extensions
{
    public class UWPPushExtension : PushExtension
    {
        private PushNotificationChannel _channel;

        public override async Task<string> GetPushChannelUri()
        {
            if (_channel == null)
            {
                try
                {
                    _channel = await TimeoutTask.Create(PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync().AsTask(), 3000);
                }
                catch (OperationCanceledException)
                {
                    TelemetryExtension.Current?.TrackEvent("CreatePushChannel_TimeoutExceeded");
                }
                catch (Exception)
                {
                    return null;
                }
                _channel.PushNotificationReceived += _channel_PushNotificationReceived;
            }

            return _channel.Uri.ToString();
        }

        private async void _channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            try
            {
                if (args.NotificationType == PushNotificationType.Raw)
                {
                    long accountId = GetAccountIdFromRawNotification(args.RawNotification);

                    if (accountId == 0)
                        return;

                    // Only look at loaded accounts, don't want to load a non-loaded one
                    var matchingAccount = AccountsManager.GetCurrentlyLoadedAccounts().FirstOrDefault(i => i.AccountId == accountId);

                    // If found, sync!
                    if (matchingAccount != null)
                    {
                        args.Cancel = true; // So any background tasks won't pick it up

                        try
                        {
                            await Sync.SyncAccountAsync(matchingAccount);
                        }

                        catch (OperationCanceledException) { }
                    }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public static long GetAccountIdFromRawNotification(RawNotification notif)
        {
            if (notif == null || notif.Content == null)
                return 0;

            long accountId;

            if (long.TryParse(notif.Content, out accountId))
                return accountId;
            else
                return 0;
        }
    }
}
