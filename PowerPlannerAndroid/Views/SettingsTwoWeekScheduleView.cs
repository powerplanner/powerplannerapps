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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using InterfacesDroid.Adapters;

namespace PowerPlannerAndroid.Views
{
    public class SettingsTwoWeekScheduleView : InterfacesDroid.Views.PopupViewHost<TwoWeekScheduleSettingsViewModel>
    {
        public SettingsTwoWeekScheduleView(ViewGroup root) : base(Resource.Layout.SettingsTwoWeekSchedule, root)
        {
        }

        public override void OnViewModelLoadedOverride()
        {
            var spinnerCurrentWeek = FindViewById<Spinner>(Resource.Id.SpinnerCurrentWeek);
            spinnerCurrentWeek.Adapter = ObservableAdapter.Create(
                new PowerPlannerSending.Schedule.Week[] { PowerPlannerSending.Schedule.Week.WeekOne, PowerPlannerSending.Schedule.Week.WeekTwo },
                itemResourceId: Resource.Layout.SpinnerItemCurrentWeekPreview,
                dropDownItemResourceId: Resource.Layout.SpinnerItemScheduleWeek);
            switch (ViewModel.CurrentWeek)
            {
                case PowerPlannerSending.Schedule.Week.WeekOne:
                    spinnerCurrentWeek.SetSelection(0);
                    break;

                case PowerPlannerSending.Schedule.Week.WeekTwo:
                    spinnerCurrentWeek.SetSelection(1);
                    break;
            }
            spinnerCurrentWeek.ItemSelected += SpinnerCurrentWeek_ItemSelected;

            var spinnerWeekChangesOn = FindViewById<Spinner>(Resource.Id.SpinnerWeekChangesOn);
            spinnerWeekChangesOn.Adapter = ObservableAdapter.Create(
                new DayOfWeek[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
                itemResourceId: Resource.Layout.SpinnerItemWeekChangesOnPreview,
                dropDownItemResourceId: Resource.Layout.SpinnerItemDayOfWeek);
            spinnerWeekChangesOn.SetSelection((int)ViewModel.WeekChangesOn);
            spinnerWeekChangesOn.ItemSelected += SpinnerWeekChangesOn_ItemSelected;
        }

        private void SpinnerWeekChangesOn_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            ViewModel.WeekChangesOn = (DayOfWeek)e.Position;
        }

        private void SpinnerCurrentWeek_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            switch (e.Position)
            {
                case 0:
                    ViewModel.CurrentWeek = PowerPlannerSending.Schedule.Week.WeekOne;
                    break;

                case 1:
                    ViewModel.CurrentWeek = PowerPlannerSending.Schedule.Week.WeekTwo;
                    break;
            }
        }
    }
}