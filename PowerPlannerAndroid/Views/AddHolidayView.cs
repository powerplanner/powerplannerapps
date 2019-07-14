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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Holiday;
using PowerPlannerAppDataLibrary;
using InterfacesDroid.Helpers;

namespace PowerPlannerAndroid.Views
{
    public class AddHolidayView : PopupViewHost<AddHolidayViewModel>
    {
        public AddHolidayView(ViewGroup root) : base(Resource.Layout.AddHoliday, root)
        {
        }

        public override void OnViewModelLoadedOverride()
        {
            if (ViewModel.State == AddHolidayViewModel.OperationState.Adding)
            {
                Title = PowerPlannerResources.GetString("String_AddHoliday").ToUpper();
                SetMenu(Resource.Menu.add_holiday_menu);
            }
            else
            {
                Title = PowerPlannerResources.GetString("String_EditHoliday").ToUpper();
                SetMenu(Resource.Menu.edit_holiday_menu);
            }

            FindViewById<Button>(Resource.Id.ButtonStartDate).Click += ButtonStartDate_Click;
            FindViewById<Button>(Resource.Id.ButtonEndDate).Click += ButtonEndDate_Click;

            if (ViewModel.Name.Length == 0)
            {
                KeyboardHelper.FocusAndShow(FindViewById<EditText>(Resource.Id.EditTextName));
            }
        }

        private void ButtonEndDate_Click(object sender, EventArgs e)
        {
            ShowDatePickerDialog(OnEndDatePicked, ViewModel.EndDate);
        }

        private void OnEndDatePicked(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            ViewModel.EndDate = e.Date;
        }

        private void ButtonStartDate_Click(object sender, EventArgs e)
        {
            ShowDatePickerDialog(OnStartDatePicked, ViewModel.StartDate);
        }

        private void OnStartDatePicked(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            ViewModel.StartDate = e.Date;
        }

        private void ShowDatePickerDialog(EventHandler<DatePickerDialog.DateSetEventArgs> handler, DateTime selectedDate)
        {
            new DatePickerDialog(Context, handler, selectedDate.Year, selectedDate.Month - 1, selectedDate.Day).Show();
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