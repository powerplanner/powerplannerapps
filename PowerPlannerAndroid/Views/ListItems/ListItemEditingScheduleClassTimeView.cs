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
using InterfacesDroid.Views;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.Helpers;

namespace PowerPlannerAndroid.Views.ListItems
{
    public class ListItemEditingScheduleClassTimeView : InflatedViewWithBindingHost
    {
        public ListItemEditingScheduleClassTimeView(ViewGroup root, ViewItemSchedule[] schedules) : base(Resource.Layout.ListItemEditingScheduleClassTime, root)
        {
            var g = new ScheduleGroup(schedules);
            DataContext = g;

            BindingHost.SetBinding<string>(nameof(g.DaysText), daysText =>
            {
                FindViewById<TextView>(Resource.Id.ListItemEditingScheduleClassTime_DaysText).Text = daysText;
            });

            BindingHost.SetBinding<string>(nameof(g.TimeText), t =>
            {
                FindViewById<TextView>(Resource.Id.ListItemEditingScheduleClassTime_TimeText).Text = t;
            });

            BindingHost.SetBinding<string>(nameof(g.Room), t =>
            {
                FindViewById<TextView>(Resource.Id.ListItemEditingScheduleClassTime_RoomText).Text = t;
            });

            // TODO: Event for click
        }
    }
}