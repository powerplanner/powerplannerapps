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
using PowerPlannerAndroid.ViewModel.Settings;

namespace PowerPlannerAndroid.Views
{
    public class SettingsWidgetScheduleView : InterfacesDroid.Views.PopupViewHost<WidgetScheduleViewModel>
    {
        public SettingsWidgetScheduleView(ViewGroup root) : base(Resource.Layout.SettingsWidgetSchedule, root)
        {
        }
    }
}