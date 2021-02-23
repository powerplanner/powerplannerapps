using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Adapters;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAndroid.Views.SettingsViews
{
    public class ReminderSettingsView : PopupViewHost<ReminderSettingsViewModel>
    {
        public ReminderSettingsView(ViewGroup root) : base(Resource.Layout.SettingsReminders, root)
        {
            Title = PowerPlannerResources.GetString("Settings_MainPage_RemindersItem.Title");
        }

        public override void OnViewModelLoadedOverride()
        {
            var spinnerClassReminderOptions = FindViewById<Spinner>(Resource.Id.SpinnerClassReminders);
            spinnerClassReminderOptions.Adapter = ObservableAdapter.Create(
                list: ViewModel.ClassReminderOptions,
                itemResourceId: Resource.Layout.SpinnerItemClassRemindersOptionPreview);
            spinnerClassReminderOptions.SetSelection(Array.IndexOf(ViewModel.ClassReminderOptions, ViewModel.SelectedClassReminderOption));

            spinnerClassReminderOptions.ItemSelected += SpinnerClassReminderOptions_ItemSelected;
        }

        private void SpinnerClassReminderOptions_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            ViewModel.SelectedClassReminderOption = ViewModel.ClassReminderOptions[e.Position];
        }
    }
}