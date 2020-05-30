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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using InterfacesDroid.Helpers;
using PowerPlannerAndroid.Adapters;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary;
using PowerPlannerSending;

namespace PowerPlannerAndroid.Views
{
    public class AddGradeView : PopupViewHost<AddGradeViewModel>
    {
        private Spinner _spinnerWeightCategory;

        public AddGradeView(ViewGroup root) : base(Resource.Layout.AddGrade, root)
        {
        }

        public override void OnViewModelLoadedOverride()
        {
            FindViewById<Button>(Resource.Id.ButtonDate).Click += ButtonDate_Click;

            _spinnerWeightCategory = FindViewById<Spinner>(Resource.Id.SpinnerWeightCategory);
            _spinnerWeightCategory.Adapter = SpinnerWeightCategoriesAdapter.Create(ViewModel.WeightCategories);
            _spinnerWeightCategory.ItemSelected += _spinnerWeightCategory_ItemSelected;
            UpdateSelectedWeightCategory();

            switch (ViewModel.State)
            {
                case AddGradeViewModel.OperationState.Adding:
                    Title = PowerPlannerResources.GetString("EditGradePage_HeaderAddString");
                    break;

                case AddGradeViewModel.OperationState.Editing:
                    Title = PowerPlannerResources.GetString("EditGradePage_HeaderEditString");
                    break;

                case AddGradeViewModel.OperationState.AddingWhatIf:
                    Title = PowerPlannerResources.GetString("EditGradePage_HeaderAddWhatIfString");
                    break;

                case AddGradeViewModel.OperationState.EditingWhatIf:
                    Title = PowerPlannerResources.GetString("EditGradePage_HeaderEditWhatIfString");
                    break;

                default:
                    throw new NotImplementedException();
            }

            SetMenu(Resource.Menu.add_grade_menu);

            if (ViewModel.Name.Length == 0)
            {
                KeyboardHelper.FocusAndShow(FindViewById<EditText>(Resource.Id.EditTextName));
            }
            else if (ViewModel.GradeReceived == Grade.UNGRADED)
            {
                KeyboardHelper.FocusAndShow(FindViewById<EditText>(Resource.Id.EditTextGradeReceived));
            }
        }

        private void UpdateSelectedWeightCategory()
        {
            if (ViewModel.SelectedWeightCategory == null)
            {
                throw new NullReferenceException("ViewModel.SelectedWeightCategory was null");
            }

            int viewModelPos = ViewModel.WeightCategories.IndexOf(ViewModel.SelectedWeightCategory);

            if (viewModelPos == -1)
            {
                throw new NullReferenceException("ViewModel.SelectedWeightCategory wasn't found in the ViewModel.WeightCategories list");
            }

            if (_spinnerWeightCategory.SelectedItemPosition != viewModelPos)
            {
                _spinnerWeightCategory.SetSelection(viewModelPos);
            }
        }

        private void _spinnerWeightCategory_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            ViewItemWeightCategory w = ViewModel.WeightCategories.ElementAtOrDefault(e.Position);

            if (w != null)
            {
                ViewModel.SelectedWeightCategory = w;
            }
        }

        private void ButtonDate_Click(object sender, EventArgs e)
        {
            new DatePickerDialog(Context, OnDatePicked, ViewModel.Date.Year, ViewModel.Date.Month - 1, ViewModel.Date.Day).Show();
        }

        private void OnDatePicked(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            ViewModel.Date = new DateTime(e.Year, e.Month + 1, e.DayOfMonth);
        }

        public override void OnMenuItemClicked(AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.MenuItemSave:
                    ViewModel.Save();
                    break;
            }
        }
    }
}