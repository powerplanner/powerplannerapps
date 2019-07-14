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
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;

namespace PowerPlannerAndroid.Views.SettingsViews.Grades
{
    public class ConfigureClassAverageGradesView : PopupViewHost<ConfigureClassAverageGradesViewModel>
    {
        public ConfigureClassAverageGradesView(ViewGroup root) : base(Resource.Layout.ConfigureClassAverageGrades, root)
        {
            Title = PowerPlannerResources.GetString("ClassPage_TextBlockAverageGradesHelpHeader.Text").ToUpper();
        }
    }
}