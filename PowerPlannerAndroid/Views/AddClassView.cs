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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesDroid.Views;
using Android.Support.V7.Widget;
using InterfacesDroid.Helpers;
using PowerPlannerAppDataLibrary;

namespace PowerPlannerAndroid.Views
{
    public class AddClassView : PopupViewHost<AddClassViewModel>
    {
        private MyColorPicker _colorPicker;

        public AddClassView(ViewGroup root) : base(Resource.Layout.AddClass, root)
        {
            _colorPicker = FindViewById<MyColorPicker>(Resource.Id.MyColorPicker);
        }

        public override void OnViewModelLoadedOverride()
        {
            switch (ViewModel.State)
            {
                case AddClassViewModel.OperationState.Editing:
                    Title = PowerPlannerResources.GetString("AddClassPage_EditTitle");
                    break;

                case AddClassViewModel.OperationState.Adding:
                    Title = PowerPlannerResources.GetString("AddClassPage_AddTitle");
                    break;
            }

            _colorPicker.SelectedColor = ColorTools.GetColor(ViewModel.Color);
            SetMenu(Resource.Menu.add_class_menu);

            FindViewById<Button>(Resource.Id.ButtonStartDate).Click += StartDate_Click;
            FindViewById<Button>(Resource.Id.ButtonEndDate).Click += EndDate_Click;

            if (ViewModel.Name.Length == 0)
            {
                KeyboardHelper.FocusAndShow(FindViewById<EditText>(Resource.Id.EditTextName));
            }
        }

        private void EndDate_Click(object sender, EventArgs e)
        {
            if (ViewModel.EndDate != null)
            {
                ShowDatePicker(OnEndDatePicked, ViewModel.EndDate.Value);
            }
            else
            {
                ShowDatePicker(OnEndDatePicked, DateTime.Today);
            }
        }

        private void ShowDatePicker(EventHandler<DatePickerDialog.DateSetEventArgs> pickedHandler, DateTime date)
        {
            new DatePickerDialog(Context, pickedHandler, date.Year, date.Month - 1, date.Day).Show();
        }

        private void OnEndDatePicked(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            ViewModel.EndDate = e.Date;
        }

        private void StartDate_Click(object sender, EventArgs e)
        {
            if (ViewModel.StartDate != null)
            {
                ShowDatePicker(OnStartDatePicked, ViewModel.StartDate.Value);
            }
            else
            {
                ShowDatePicker(OnStartDatePicked, DateTime.Today);
            }
        }

        private void OnStartDatePicked(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            ViewModel.StartDate = e.Date;
        }

        public override void OnMenuItemClicked(Android.Support.V7.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.MenuItemSave:
                    ViewModel.Color = ColorTools.ToArray(_colorPicker.SelectedColor);
                    ViewModel.Save();
                    break;
            }
        }
    }
}