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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using InterfacesDroid.Adapters;
using InterfacesDroid.Views;
using Android.Support.V7.Widget;
using PowerPlannerAndroid.Adapters;
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesDroid.Helpers;
using System.ComponentModel;
using ToolsPortable;
using PowerPlannerAppDataLibrary;
using PowerPlannerAndroid.Views.Controls;

namespace PowerPlannerAndroid.Views
{
    public class AddHomeworkView : PopupViewHost<AddHomeworkViewModel>
    {
        private Spinner _spinnerClass;
        private Spinner _spinnerWeight;
        private Spinner _spinnerTimeOption;

        public AddHomeworkView(ViewGroup root) : base(Resource.Layout.AddHomework, root)
        {
        }

        public override void OnViewModelLoadedOverride()
        {
            FindViewById<Button>(Resource.Id.ButtonDate).Click += ButtonDate_Click;

            _spinnerClass = FindViewById<Spinner>(Resource.Id.SpinnerClass);
            _spinnerClass.Adapter = SpinnerClassesAdapter.Create(ViewModel.Classes);
            _spinnerClass.ItemSelected += _spinnerClass_ItemSelected;
            UpdateSelectedClass();

            _spinnerWeight = FindViewById<Spinner>(Resource.Id.SpinnerWeightCategory);
            AssignWeightCategoriesAdapter();
            _spinnerWeight.ItemSelected += _spinnerWeight_ItemSelected;
            UpdateSelectedWeight();

            _spinnerTimeOption = FindViewById<Spinner>(Resource.Id.SpinnerTimeOption);
            AssignTimeOptionsAdapter();
            _spinnerTimeOption.ItemSelected += _spinnerTimeOption_ItemSelected;

            switch (ViewModel.Type)
            {
                case AddHomeworkViewModel.ItemType.Homework:
                    switch (ViewModel.State)
                    {
                        case AddHomeworkViewModel.OperationState.Adding:
                            Title = PowerPlannerResources.GetString("String_AddTask").ToUpper();
                            break;

                        case AddHomeworkViewModel.OperationState.Editing:
                            Title = PowerPlannerResources.GetString("String_EditTask").ToUpper();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                    break;

                case AddHomeworkViewModel.ItemType.Exam:
                    switch (ViewModel.State)
                    {
                        case AddHomeworkViewModel.OperationState.Adding:
                            Title = PowerPlannerResources.GetString("String_AddEvent").ToUpper();
                            break;

                        case AddHomeworkViewModel.OperationState.Editing:
                            Title = PowerPlannerResources.GetString("String_EditEvent").ToUpper();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                    break;
            }

            FindViewById<Button>(Resource.Id.ButtonStartTime).Click += StartTime_Click;
            FindViewById<Button>(Resource.Id.ButtonEndTime).Click += EndTime_Click;

            FindViewById<EditingImageAttachmentsWrapGrid>(Resource.Id.EditingImageAttachmentsWrapGrid).RequestedAddImage += AddHomeworkView_RequestedAddImage;

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            SetMenu(Resource.Menu.add_homework_menu);

            if (ViewModel.Name.Length == 0)
            {
                KeyboardHelper.FocusAndShow(FindViewById<EditText>(Resource.Id.EditTextName));
            }
        }

        private async void AddHomeworkView_RequestedAddImage(object sender, EventArgs e)
        {
            await ViewModel.AddNewImageAttachmentAsync();
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

        private void _spinnerTimeOption_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            string timeOption = ViewModel.TimeOptions.ElementAtOrDefault(e.Position);
            if (timeOption != null)
            {
                ViewModel.SelectedTimeOption = timeOption;
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.WeightCategories):
                    AssignWeightCategoriesAdapter();
                    break;

                case nameof(ViewModel.SelectedWeightCategory):
                    UpdateSelectedWeight();
                    break;

                case nameof(ViewModel.TimeOptions):
                    AssignTimeOptionsAdapter();
                    break;

                case nameof(ViewModel.SelectedTimeOption):
                    UpdateSelectedTimeOption();
                    break;

                case nameof(ViewModel.Repeats):
                    if (ViewModel.Repeats)
                    {
                        CreateRecurrenceControlIfNotExists();
                    }
                    break;
            }
        }

        private bool _createdRecurrenceControl;
        private void CreateRecurrenceControlIfNotExists()
        {
            if (_createdRecurrenceControl)
            {
                return;
            }

            _createdRecurrenceControl = true;

            var container = FindViewById<FrameLayout>(Resource.Id.RecurrenceControlContainer);
            var repeatsSubControl = new AddHomeworkRepeatsSubControl(container)
            {
                DataContext = ViewModel,
                LayoutParameters = new FrameLayout.LayoutParams(
                    FrameLayout.LayoutParams.MatchParent,
                    FrameLayout.LayoutParams.WrapContent)
            };
            container.AddView(repeatsSubControl);
        }

        private void AssignWeightCategoriesAdapter()
        {
            if (ViewModel.WeightCategories != null)
            {
                _spinnerWeight.Adapter = ObservableAdapter.Create<ViewItemWeightCategory>(
                       ViewModel.WeightCategories,
                       itemResourceId: Resource.Layout.SpinnerItemHomeworkWeightCategoryPreview,
                       dropDownItemResourceId: Resource.Layout.SpinnerItemWeightCategory);
            }
        }

        private void AssignTimeOptionsAdapter()
        {
            _spinnerTimeOption.Adapter = ObservableAdapter.Create(
                ViewModel.TimeOptions,
                itemResourceId: Resource.Layout.SpinnerItemTimeOptionPreview,
                dropDownItemResourceId: Resource.Layout.SpinnerItemTimeOption);

            UpdateSelectedTimeOption();
        }

        private void _spinnerWeight_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            ViewItemWeightCategory weight = ViewModel.WeightCategories.ElementAtOrDefault(e.Position);
            if (weight != null)
            {
                ViewModel.SelectedWeightCategory = weight;
            }
        }

        private void UpdateSelectedTimeOption()
        {
            if (ViewModel.SelectedTimeOption == null)
            {
                return;
            }

            int pos = ViewModel.TimeOptions.FindIndex(i => i == ViewModel.SelectedTimeOption);
            if (pos == -1)
            {
                return;
            }

            if (_spinnerTimeOption.SelectedItemPosition != pos)
            {
                _spinnerTimeOption.SetSelection(pos);
            }
        }

        private void UpdateSelectedWeight()
        {
            if (ViewModel.SelectedWeightCategory == null || ViewModel.WeightCategories == null)
            {
                // This is null in the case where it's a task, not a homework
                return;
            }

            int pos = ViewModel.WeightCategories.ToList().IndexOf(ViewModel.SelectedWeightCategory);
            if (pos == -1)
            {
                return;
            }

            if (_spinnerWeight.SelectedItemPosition != pos)
            {
                _spinnerWeight.SetSelection(pos);
            }
        }

        private void UpdateSelectedClass()
        {
            if (ViewModel.Class == null)
            {
                throw new NullReferenceException("ViewModel.Class was null");
            }

            int viewModelPos = ViewModel.Classes.IndexOf(ViewModel.Class);

            if (viewModelPos == -1)
            {
                throw new NullReferenceException("ViewModel.Class wasn't found in the ViewModel.Classes list");
            }

            if (_spinnerClass.SelectedItemPosition != viewModelPos)
            {
                _spinnerClass.SetSelection(viewModelPos);
            }
        }

        private void _spinnerClass_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            ViewItemClass c = ViewModel.Classes.ElementAtOrDefault(e.Position);

            if (c != null)
            {
                ViewModel.Class = c;
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

        public override void OnMenuItemClicked(Android.Support.V7.Widget.Toolbar.MenuItemClickEventArgs e)
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