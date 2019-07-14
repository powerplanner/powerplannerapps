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
    public class ListItemEditingScheduleClassTimeView : InflatedViewWithBinding
    {
        public ListItemEditingScheduleClassTimeView(ViewGroup root, ViewItemSchedule[] schedules) : base(Resource.Layout.ListItemEditingScheduleClassTime, root)
        {
            DataContext = new ScheduleGroup(schedules);

            // TODO: Event for click
        }
    }
}