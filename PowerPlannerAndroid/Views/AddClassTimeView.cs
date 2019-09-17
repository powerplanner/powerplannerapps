using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using InterfacesDroid.Adapters;
using ToolsPortable;

namespace PowerPlannerAndroid.Views
{
    public class AddClassTimeView : PopupViewHost<AddClassTimeViewModel>
    {
        private Spinner _spinnerWeek;

        public AddClassTimeView(ViewGroup root) : base(Resource.Layout.AddClassTime, root)
        {
            FindViewById<CheckBox>(Resource.Id.CheckBoxMonday).Text = DateTools.ToLocalizedString(DayOfWeek.Monday);
            FindViewById<CheckBox>(Resource.Id.CheckBoxTuesday).Text = DateTools.ToLocalizedString(DayOfWeek.Tuesday);
            FindViewById<CheckBox>(Resource.Id.CheckBoxWednesday).Text = DateTools.ToLocalizedString(DayOfWeek.Wednesday);
            FindViewById<CheckBox>(Resource.Id.CheckBoxThursday).Text = DateTools.ToLocalizedString(DayOfWeek.Thursday);
            FindViewById<CheckBox>(Resource.Id.CheckBoxFriday).Text = DateTools.ToLocalizedString(DayOfWeek.Friday);
            FindViewById<CheckBox>(Resource.Id.CheckBoxSaturday).Text = DateTools.ToLocalizedString(DayOfWeek.Saturday);
            FindViewById<CheckBox>(Resource.Id.CheckBoxSunday).Text = DateTools.ToLocalizedString(DayOfWeek.Sunday);
        }

        public override void OnViewModelLoadedOverride()
        {
            Title = ViewModel.ClassName.ToUpper();

            if (ViewModel.State == AddClassTimeViewModel.OperationState.Adding)
                SetMenu(Resource.Menu.add_class_time_menu);
            else
                SetMenu(Resource.Menu.edit_class_time_menu);

            FindViewById<Button>(Resource.Id.ButtonStartTime).Click += StartTime_Click;
            FindViewById<Button>(Resource.Id.ButtonEndTime).Click += EndTime_Click;

            _spinnerWeek = FindViewById<Spinner>(Resource.Id.SpinnerWeek);
            AssignWeekAdapter();
            _spinnerWeek.ItemSelected += _spinnerWeek_ItemSelected;
            UpdateSelectedWeek();
        }

        private void _spinnerWeek_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            switch (e.Position)
            {
                case 0:
                    ViewModel.ScheduleWeek = PowerPlannerSending.Schedule.Week.BothWeeks;
                    break;

                case 1:
                    ViewModel.ScheduleWeek = PowerPlannerSending.Schedule.Week.WeekOne;
                    break;

                case 2:
                    ViewModel.ScheduleWeek = PowerPlannerSending.Schedule.Week.WeekTwo;
                    break;
            }
        }

        private void UpdateSelectedWeek()
        {
            switch (ViewModel.ScheduleWeek)
            {
                case PowerPlannerSending.Schedule.Week.BothWeeks:
                    _spinnerWeek.SetSelection(0);
                    break;

                case PowerPlannerSending.Schedule.Week.WeekOne:
                    _spinnerWeek.SetSelection(1);
                    break;

                case PowerPlannerSending.Schedule.Week.WeekTwo:
                    _spinnerWeek.SetSelection(2);
                    break;
            }
        }

        private void AssignWeekAdapter()
        {
            _spinnerWeek.Adapter = ObservableAdapter.Create<PowerPlannerSending.Schedule.Week>(
                   new PowerPlannerSending.Schedule.Week[] { PowerPlannerSending.Schedule.Week.BothWeeks, PowerPlannerSending.Schedule.Week.WeekOne, PowerPlannerSending.Schedule.Week.WeekTwo },
                   itemResourceId: Resource.Layout.SpinnerItemScheduleWeekPreview,
                   dropDownItemResourceId: Resource.Layout.SpinnerItemScheduleWeek);
        }

        private void EndTime_Click(object sender, EventArgs e)
        {
            new TimePickerDialog(
                context: Context,
                callBack: OnEndTimePicked,
                hourOfDay: ViewModel.EndTime.Hours,
                minute: ViewModel.EndTime.Minutes,
                is24HourView: Android.Text.Format.DateFormat.Is24HourFormat(Context)).Show();
        }

        private void OnEndTimePicked(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            ViewModel.EndTime = new TimeSpan(e.HourOfDay, e.Minute, 0);
        }

        private void StartTime_Click(object sender, EventArgs e)
        {
            new TimePickerDialog(
                context: Context,
                callBack: OnStartTimePicked,
                hourOfDay: ViewModel.StartTime.Hours,
                minute: ViewModel.StartTime.Minutes,
                is24HourView: Android.Text.Format.DateFormat.Is24HourFormat(Context)).Show();
        }

        private void OnStartTimePicked(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            ViewModel.StartTime = new TimeSpan(e.HourOfDay, e.Minute, 0);
        }

        public override void OnMenuItemClicked(Android.Support.V7.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.MenuItemSave:
                    ViewModel.Save();
                    break;

                case Resource.Id.MenuItemDelete:
                    ViewModel.Delete();
                    break;
            }
        }
    }
}