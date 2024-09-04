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
using Microsoft.QueryStringDotNET;
using PowerPlannerAndroid.Extensions;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerAndroid.Receivers
{
    [BroadcastReceiver]
    public class UpdateDayBeforeNotificationReceiver : BroadcastReceiver
    {
        public override async void OnReceive(Context context, Intent intent)
        {
            var pendingResult = base.GoAsync();

            try
            {
                QueryString qs = QueryString.Parse(intent.DataString.Substring(intent.DataString.IndexOf('?') + 1));
                Guid localAccountId = Guid.Parse(qs["localAccountId"]);
                await AndroidRemindersExtension.UpdateAndScheduleDayBeforeNotification(context, localAccountId, fromForeground: false);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            try
            {
                // Sometimes throws ObjectDisposedException
                pendingResult.Finish();
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}