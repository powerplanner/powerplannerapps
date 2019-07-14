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

namespace PowerPlannerAndroid.Helpers
{
    public static class AlarmManagerHelper
    {
        public static void Schedule(Context context, Type receiverType, DateTime wakeTime, string uriData = null, bool wakeDevice = false)
        {
            Intent alarmIntent = new Intent(context, receiverType);

            if (uriData != null)
            { 
                alarmIntent.SetData(Android.Net.Uri.Parse(uriData));
            }

            PendingIntent pendingAlarmIntent = PendingIntent.GetBroadcast(context, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);

            AlarmManager alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);

            double milliseconds = (wakeTime - DateTime.Now).TotalMilliseconds;
            if (milliseconds < 5000)
            {
                // Ensure at least 5 seconds in the future
                milliseconds = 5000;
            }

            alarmManager.Set(wakeDevice ? AlarmType.RtcWakeup : AlarmType.Rtc, Java.Lang.JavaSystem.CurrentTimeMillis() + (long)milliseconds, pendingAlarmIntent);
        }
    }
}