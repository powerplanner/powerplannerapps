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
using Firebase.Messaging;
using PowerPlannerAppDataLibrary.Extensions;
using Android.App.Job;

namespace PowerPlannerAndroid.Services
{
    [Service(Exported = true)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        public override void OnMessageReceived(RemoteMessage message)
        {
            try
            {
                if (message.Data.TryGetValue("Action", out string action)
                    && action == "SyncAccount"
                    && message.Data.TryGetValue("AccountId", out string accountIdString)
                    && long.TryParse(accountIdString, out long accountId))
                {
                    ScheduleSyncJob(accountId);
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public override void OnDeletedMessages()
        {
            // TODO: Sync all accounts since messages were dropped/deleted
        }

        private void ScheduleSyncJob(long accountId)
        {
            // JobScheduler was added in API 21
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                return;
            }

            var extras = new PersistableBundle();
            extras.PutLong("AccountId", accountId);

            var builder = new JobInfo.Builder(unchecked((int)accountId), new ComponentName(this, Java.Lang.Class.FromType(typeof(MySyncService))))
                .SetRequiredNetworkType(NetworkType.Any)
                .SetExtras(extras);

            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                builder.SetRequiresBatteryNotLow(true);
            }

            var jobScheduler = (JobScheduler)this.GetSystemService(Context.JobSchedulerService);
            jobScheduler.Schedule(builder.Build());
        }

        public override void OnNewToken(string token)
        {
            // TODO: In future should have this kick off a job service that syncs all accounts
        }
    }
}