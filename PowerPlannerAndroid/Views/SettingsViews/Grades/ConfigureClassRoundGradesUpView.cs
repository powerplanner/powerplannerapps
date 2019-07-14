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
    public class ConfigureClassRoundGradesUpView : PopupViewHost<ConfigureClassRoundGradesUpViewModel>
    {
        public ConfigureClassRoundGradesUpView(ViewGroup root) : base(Resource.Layout.ConfigureClassRoundGradesUp, root)
        {
            Title = PowerPlannerResources.GetString("ClassPage_TextBlockRoundGradesUpHelpHeader.Text").ToUpper();
        }
    }
}