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
using PowerPlannerAndroid.Views.Controls;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;

namespace PowerPlannerAndroid.Views.SettingsViews.Grades
{
    public class ConfigureClassGradesListView : PopupViewHost<ConfigureClassGradesListViewModel>
    {
        public ConfigureClassGradesListView(ViewGroup root) : base(Resource.Layout.ConfigureClassGradesList, root)
        {
            FindViewById<SettingsListItem>(Resource.Id.ConfigureClassGradesAverageGradesItem).Click += delegate { ViewModel?.ConfigureAverageGrades(); };
            FindViewById<SettingsListItem>(Resource.Id.ConfigureClassGradesCreditsItem).Click += delegate { ViewModel?.ConfigureCredits(); };
            FindViewById<SettingsListItem>(Resource.Id.ConfigureClassGradesGpaTypeItem).Click += delegate { ViewModel?.ConfigureGpaType(); };
            FindViewById<SettingsListItem>(Resource.Id.ConfigureClassGradesGradeScaleItem).Click += delegate { ViewModel?.ConfigureGradeScale(); };
            FindViewById<SettingsListItem>(Resource.Id.ConfigureClassGradesPassingGradeItem).Click += delegate { ViewModel?.ConfigurePassingGrade(); };
            FindViewById<SettingsListItem>(Resource.Id.ConfigureClassGradesRoundGradesUpItem).Click += delegate { ViewModel?.ConfigureRoundGradesUp(); };
            FindViewById<SettingsListItem>(Resource.Id.ConfigureClassGradesWeightCategoriesItem).Click += delegate { ViewModel?.ConfigureWeightCategories(); };
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            Title = ViewModel.Class.Name.ToUpper();
        }
    }
}