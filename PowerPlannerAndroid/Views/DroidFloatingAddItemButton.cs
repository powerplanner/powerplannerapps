using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAndroid.Views.Controls;
using PowerPlannerAppDataLibrary.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Droid;
using Vx.Droid.Views;

namespace PowerPlannerAndroid.Views
{
    internal class DroidFloatingAddItemButton : DroidView<FloatingAddItemButton, FloatingAddItemControl>
    {
        public DroidFloatingAddItemButton() : base(new FloatingAddItemControl(VxDroidExtensions.ApplicationContext))
        {
            View.OnRequestAddTask += View_OnRequestAddTask;
            View.OnRequestAddEvent += View_OnRequestAddEvent;
            View.OnRequestAddHoliday += View_OnRequestAddHoliday;
        }

        protected override void ApplyProperties(FloatingAddItemButton oldView, FloatingAddItemButton newView)
        {
            base.ApplyProperties(oldView, newView);

            View.SupportsAddHoliday = newView.AddHoliday != null;
        }

        private void View_OnRequestAddHoliday(object sender, EventArgs e)
        {
            VxView.AddHoliday?.Invoke();
        }

        private void View_OnRequestAddEvent(object sender, EventArgs e)
        {
            VxView.AddEvent?.Invoke();
        }

        private void View_OnRequestAddTask(object sender, EventArgs e)
        {
            VxView.AddTask?.Invoke();
        }
    }
}