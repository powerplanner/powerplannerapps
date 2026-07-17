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

            BindingHost.SetBinding<ScheduleGroup, string>(nameof(g.DaysText), group => group.DaysText, daysText =>
            {
                FindViewById<TextView>(Resource.Id.ListItemEditingScheduleClassTime_DaysText).Text = daysText;
            });

            BindingHost.SetBinding<ScheduleGroup, string>(nameof(g.TimeText), group => group.TimeText, t =>
            {
                FindViewById<TextView>(Resource.Id.ListItemEditingScheduleClassTime_TimeText).Text = t;
            });

            BindingHost.SetBinding<ScheduleGroup, string>(nameof(g.Room), group => group.Room, t =>
            {
                FindViewById<TextView>(Resource.Id.ListItemEditingScheduleClassTime_RoomText).Text = t;
            });

            // TODO: Event for click
        }
    }
}