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
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAndroid.Extensions;
using PowerPlannerAppDataLibrary.DataLayer;

namespace PowerPlannerAndroid.Receivers
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    public class MyBootReceiver : BroadcastReceiver
    {
        public override async void OnReceive(Context context, Intent intent)
        {
            var pendingResult = base.GoAsync();

            try
            {
                var allAccounts = await AccountsManager.GetAllAccounts();

                foreach (var account in allAccounts)
                {
                    // Update/schedule both day-of and day-before
                    await AndroidRemindersExtension.UpdateAndScheduleNotifications(context, account, fromForeground: false);
                }
            }

            catch (Exception ex)
            {
                // Avoid throwing here, since otherwise we'll get a "Power Planner has stopped responding" dialog when the user isn't even using the app
                TelemetryExtension.Current?.TrackException(ex);
            }

            pendingResult.Finish();
        }
    }
}