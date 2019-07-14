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
using PowerPlannerAndroid.Services;
using PowerPlannerAndroid.Helpers;

namespace PowerPlannerAndroid.Receivers
{
    [BroadcastReceiver]
    public class UpdateWidgetAgendaReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            // We don't need to do any async code, since notifying the widget manager
            // automatically creates a service that will do the async work, so there's no
            // need to create a service just for receiving this alarm.

            // Updating the widget does the following...
            // (1) Updates the widget
            // (2) Schedules the next alarm that will call this again in the future to update again
            WidgetsHelper.UpdateAgendaWidget();
        }
    }
}