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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views
{
    public class QuickAddView : PopupViewHost<QuickAddViewModel>
    {
        public QuickAddView(ViewGroup root) : base(Resource.Layout.QuickAdd, root)
        {
            Title = PowerPlannerResources.GetString("QuickAddPage.Title").ToUpper();

            FindViewById<Button>(Resource.Id.ButtonAddTask).Text = R.S("String_AddTask");
            FindViewById<Button>(Resource.Id.ButtonAddTask).Click += delegate
            {
                ViewModel.AddTask();
            };

            FindViewById<Button>(Resource.Id.ButtonAddEvent).Text = R.S("String_AddEvent");
            FindViewById<Button>(Resource.Id.ButtonAddEvent).Click += delegate
            {
                ViewModel.AddEvent();
            };
        }
    }
}