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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views
{
    public class QuickAddView : PopupViewHost<QuickAddViewModel>
    {
        public QuickAddView(ViewGroup root) : base(Resource.Layout.QuickAdd, root)
        {
            Title = PowerPlannerResources.GetString("QuickAddPage.Title").ToUpper();

            FindViewById<Button>(Resource.Id.ButtonAddTask).Click += delegate
            {
                ViewModel.AddHomework();
            };

            FindViewById<Button>(Resource.Id.ButtonAddEvent).Click += delegate
            {
                ViewModel.AddExam();
            };
        }
    }
}