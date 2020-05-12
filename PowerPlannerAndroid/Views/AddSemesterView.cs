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
using InterfacesDroid.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years;
using PowerPlannerAppDataLibrary;
using InterfacesDroid.Helpers;

namespace PowerPlannerAndroid.Views
{
    public class AddSemesterView : PopupViewHost<AddSemesterViewModel>
    {
        public AddSemesterView(ViewGroup root) : base(Resource.Layout.AddSemester, root)
        {
            SetMenu(Resource.Menu.add_semester_menu);
        }

        public override void OnViewModelLoadedOverride()
        {
            ViewModel.SupportsStartEnd = true;

            switch (ViewModel.State)
            {
                case AddSemesterViewModel.OperationState.Adding:
                    Title = PowerPlannerResources.GetString("EditSemesterPage_Title_Adding");
                    break;

                case AddSemesterViewModel.OperationState.Editing:
                    Title = PowerPlannerResources.GetString("EditSemesterPage_Title_Editing");
                    break;
            }

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

        public override void OnMenuItemClicked(AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.MenuItemSave:
                    ViewModel.Save();
                    break;

                case Resource.Id.MenuItemDelete:
                    PromptDelete();
                    break;
            }
        }

        private void PromptDelete()
        {
            var builder = new AlertDialog.Builder(Context);

            builder
                .SetTitle(PowerPlannerResources.GetString("MessageDeleteSemester_Title"))
                .SetMessage(PowerPlannerResources.GetString("MessageDeleteSemester_Body"))
                .SetPositiveButton(PowerPlannerResources.GetMenuItemDelete(), delegate { ViewModel.Delete(); })
                .SetNegativeButton(PowerPlannerResources.GetStringCancel(), delegate { });

            builder.Create().Show();
        }
    }
}