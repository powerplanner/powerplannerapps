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
        public static void Schedule(Context context, Type receiverType, DateTime wakeTime, string uriData = null,         bool wakeDevice = false, bool exact = false)
        {
            Intent alarmIntent = new Intent(context, receiverType);

            if (uriData != null)
            { 
                alarmIntent.SetData(Android.Net.Uri.Parse(uriData));
            }

            PendingIntent pendingAlarmIntent = PendingIntent.GetBroadcast(context, 0, alarmIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            AlarmManager alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);

            double milliseconds = (wakeTime - DateTime.Now).TotalMilliseconds;
            if (milliseconds < 5000)
            {
                // Ensure at least 5 seconds in the future
                milliseconds = 5000;
            }

            var alarmType = wakeDevice ? AlarmType.RtcWakeup : AlarmType.Rtc;
            long triggerAtMillis = Java.Lang.JavaSystem.CurrentTimeMillis() + (long)milliseconds;

            if (exact)
            {
                // Plain Set() is inexact and gets deferred by Doze/App Standby, so time-sensitive alarms need the exact + allow-while-idle variants
                // Before API 31, exact alarms are always allowed, but starting in API 31, the user can revoke the permission for exact alarms, so we need to check if we can schedule them first
                if (!OperatingSystem.IsAndroidVersionAtLeast(31) || alarmManager.CanScheduleExactAlarms())
                {
                    try
                    {
                        alarmManager.SetExactAndAllowWhileIdle(alarmType, triggerAtMillis, pendingAlarmIntent);
                        return;
                    }
                    catch (Java.Lang.SecurityException)
                    {
                        // Exact alarm permission was revoked, fall through to the inexact variant
                    }
                }

                // Exact alarms not permitted, so this is the best we can do (still fires during Doze, but possibly a few minutes late)
                alarmManager.SetAndAllowWhileIdle(alarmType, triggerAtMillis, pendingAlarmIntent);
                return;
            }

            alarmManager.Set(alarmType, triggerAtMillis, pendingAlarmIntent);
        }
    }
}