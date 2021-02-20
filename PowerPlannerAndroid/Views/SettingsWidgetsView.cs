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
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views
{
    public class SettingsWidgetsView : PopupViewHost<WidgetsViewModel>
    {
        public SettingsWidgetsView(ViewGroup root) : base(Resource.Layout.SettingsWidgets, root)
        {
            Title = PowerPlannerResources.GetString("String_Widgets");

            FindViewById<View>(Resource.Id.SettingsWidgetAgenda).Click += delegate { ViewModel.OpenAgendaWidgetSettings(); };
            FindViewById<View>(Resource.Id.SettingsWidgetSchedule).Click += delegate { ViewModel.OpenScheduleWidgetSettings(); };
        }
    }
}