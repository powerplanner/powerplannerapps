using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using InterfacesDroid.Adapters;
using PowerPlannerAndroid.Views.Controls;
using PowerPlannerAndroid.Vx;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAndroid.Views.SettingsViews
{
    public class ReminderSettingsView : VxPopupViewHost<ReminderSettingsViewModel>
    {
        public ReminderSettingsView(Context context) : base(context)
        {
            Title = PowerPlannerResources.GetString("Settings_MainPage_RemindersItem.Title");
        }

        public override void OnViewModelLoadedOverride()
        {
            View = VxVerticalScrollView(

                // Overall description
                new VxTextView(Context, VxTextStyle.Medium)
                    .VxTextLocalized("Settings_RemindersWithClasses_Description.Text")
                    .VxPadding(16),

                new Divider(Context),

                // Class reminders
                new VxTextView(Context, VxTextStyle.Large)
                    .VxTextLocalized("Settings_RemindersWithClasses_Description.Text")
                    .VxPadding(16),

                // Remind me in X minutes
                new FullWidthSpinner(Context)
                    .VxAdapter(ObservableAdapter.Create(
                        list: ViewModel.ClassReminderOptions,
                        itemResourceId: Resource.Layout.SpinnerItemClassRemindersOptionPreview))
                    .VxSelection(Array.IndexOf(ViewModel.ClassReminderOptions, ViewModel.SelectedClassReminderOption))
                    .VxItemSelected(SpinnerClassReminderOptions_ItemSelected),

                // Explanation
                new VxTextView(Context, VxTextStyle.Small)
                    .VxTextLocalized("Settings_Reminders_ClassRemindersDescription.Text")
                    .VxPadding(16, 8, 16, 16),

                new Divider(Context),

                // Task/event reminders
                new VxTextView(Context, VxTextStyle.Large)
                    .VxTextLocalized("Settings_Reminders_TaskEventRemindersHeader.Text")
                    .VxPadding(16),

                // Day before
                new SwitchCompat(Context)
                    .VxTextLocalized("Settings_Reminders_ToggleDayBefore.Header")
                    .VxTextSize(16)
                    .VxPadding(16, 12, 16, 12)
                    .VxChecked(Binding(nameof(ViewModel.RemindersDayBefore))),

                // Explanation about day before
                new VxTextView(Context, VxTextStyle.Small)
                    .VxTextLocalized("Settings_Reminders_DayBeforeDescription.Text")
                    .VxPadding(16, 8, 16, 16),

                // Day of
                new SwitchCompat(Context)
                    .VxTextLocalized("Settings_Reminders_ToggleDayOf.Header")
                    .VxTextSize(16)
                    .VxPadding(16, 12, 16, 12)
                    .VxChecked(Binding(nameof(ViewModel.RemindersDayOf))),

                // Explanation about day before
                new VxTextView(Context, VxTextStyle.Small)
                    .VxTextLocalized("Settings_Reminders_DayOfDescription.Text")
                    .VxPadding(16, 8, 16, 16)

            );
        }

        private void SpinnerClassReminderOptions_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            ViewModel.SelectedClassReminderOption = ViewModel.ClassReminderOptions[e.Position];
        }
    }
}