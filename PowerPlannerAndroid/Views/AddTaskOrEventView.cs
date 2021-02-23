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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using InterfacesDroid.Adapters;
using InterfacesDroid.Views;
using PowerPlannerAndroid.Adapters;
using PowerPlannerAppDataLibrary.ViewItems;
using InterfacesDroid.Helpers;
using System.ComponentModel;
using ToolsPortable;
using PowerPlannerAppDataLibrary;
using PowerPlannerAndroid.Views.Controls;
using PowerPlannerAndroid.Vx;
using Android.Text;
using AndroidX.AppCompat.Widget;

namespace PowerPlannerAndroid.Views
{
    public class AddTaskOrEventView : VxPopupViewHost<AddTaskOrEventViewModel>
    {
        private Spinner _spinnerClass;
        private Spinner _spinnerWeight;
        private Spinner _spinnerTimeOption;
        private FrameLayout _recurrenceControlContainer;
        private EditText _editTextName;

        public AddTaskOrEventView(Context context) : base(context)
        {
        }

        public override void OnViewModelLoadedOverride()
        {
            View = VxVerticalScrollView(

                // Name
                new FullWidthEditText(Context)
                    .VxInputType(InputTypes.TextFlagCapSentences | InputTypes.TextFlagAutoCorrect)
                    .VxImeOptions(Android.Views.InputMethods.ImeAction.Done)
                    .VxHintLocalized("EditTaskOrEventPage_TextBoxName.Header")
                    .VxText(Binding(nameof(ViewModel.Name)))
                    .VxReference(ref _editTextName),

                new Divider(Context),

                // Classes
                new FullWidthSpinner(Context)
                    .VxAdapter(SpinnerClassesAdapter.Create(ViewModel.Classes))
                    .VxItemSelected(_spinnerClass_ItemSelected)
                    .VxReference(ref _spinnerClass),

                new Divider(Context),

                // Date
                new FullWidthButton(Context)
                    .VxText(Binding(nameof(ViewModel.Date), (DateTime date) => date.ToString("dddd, MMM d")))
                    .VxClick(ButtonDate_Click),

                new Divider(Context),

                // Weight category
                new FullWidthSpinner(Context)
                    .VxReference(ref _spinnerWeight)
                    .VxVisibility(Binding(nameof(ViewModel.IsWeightCategoryPickerVisible))),

                new Divider(Context),

                // Time options
                new FullWidthSpinner(Context)
                    .VxReference(ref _spinnerTimeOption),

                new Divider(Context),

                // Start and end times
                new LinearLayout(Context)
                    .VxPadding(0, 10, 0, 10)
                    .VxVisibility(Binding(nameof(ViewModel.IsStartTimePickerVisible)))
                    .VxChildren(

                        // Start time
                        new BorderlessButton(Context)
                            .VxLayoutParams().AutoWidth().Apply()
                            .VxText(Binding(nameof(ViewModel.StartTime), (TimeSpan value) => DateHelper.ToShortTimeString(DateTime.Today.Add(value))))
                            .VxTextColorResource(Resource.Color.foregroundFull)
                            .VxClick(StartTime_Click),

                        // To
                        new TextView(Context)
                            .VxLayoutParams().AutoWidth().Apply()
                            .VxTextLocalized("TextBlock_To.Text")
                            .VxVisibility(Binding(nameof(ViewModel.IsEndTimePickerVisible))),

                        // End time
                        new BorderlessButton(Context)
                            .VxLayoutParams().AutoWidth().Apply()
                            .VxText(Binding(nameof(ViewModel.EndTime), (TimeSpan value) => DateHelper.ToShortTimeString(DateTime.Today.Add(value))))
                            .VxTextColorResource(Resource.Color.foregroundFull)
                            .VxClick(EndTime_Click)
                            .VxVisibility(Binding(nameof(ViewModel.IsEndTimePickerVisible)))

                    ),

                new Divider(Context)
                    .VxVisibility(Binding(nameof(ViewModel.IsStartTimePickerVisible))),

                // Different time zone warning
                new VxTextView(Context, VxTextStyle.Small)
                    .VxTextLocalized("DifferentTimeZoneWarning.Text")
                    .VxPadding(16, 8, 16, 8)
                    .VxTextColor(new Android.Graphics.Color(252, 0, 0))
                    .VxVisibility(Binding(nameof(ViewModel.IsInDifferentTimeZone))),

                new Divider(Context)
                    .VxVisibility(Binding(nameof(ViewModel.IsInDifferentTimeZone))),

                // Details
                new FullWidthEditText(Context)
                    .VxHintLocalized("EditTaskOrEventPage_TextBoxDetails.Header")
                    .VxText(Binding(nameof(ViewModel.Details)))
                    .VxInputType(InputTypes.TextFlagCapSentences | InputTypes.TextFlagAutoCorrect | InputTypes.TextFlagMultiLine)
                    .VxMinLines(3)
                    .VxGravity(GravityFlags.Top),

                new Divider(Context),

                // Repeats toggle
                new SwitchCompat(Context)
                    .VxTextLocalized("RepeatingEntry_CheckBoxRepeats.Content")
                    .VxTextSize(16)
                    .VxPadding(16)
                    .VxChecked(Binding(nameof(ViewModel.Repeats)))
                    .VxVisibility(Binding(nameof(ViewModel.IsRepeatsVisible))),

                // Repeats content
                new FrameLayout(Context)
                    .VxBackgroundResource(Resource.Color.controlBackground)
                    .VxPadding(12)
                    .VxLayoutParams().Margins(16, 6, 16, 12).Apply()
                    .VxVisibility(Binding(nameof(ViewModel.Repeats)))
                    .VxReference(ref _recurrenceControlContainer), // Contents will be dynamically added when needed

                new Divider(Context),

                // Image attachments
                new TextView(Context)
                    .VxTextLocalized("String_ImageAttachments")
                    .VxLayoutParams().Margins(16, 10, 16, 0).Apply()
                    .VxTextStyle(Android.Graphics.Typeface.DefaultBold),

                new EditingImageAttachmentsWrapGrid(Context)
                {
                    ImageAttachments = ViewModel.ImageAttachments
                }
                    .VxLayoutParams().Margins(16, 8, 16, 8).Apply()
                    .VxRequestedAddImage(AddTaskOrEventView_RequestedAddImage)

                );

            UpdateSelectedClass();

            AssignWeightCategoriesAdapter();
            _spinnerWeight.ItemSelected += _spinnerWeight_ItemSelected;
            UpdateSelectedWeight();

            AssignTimeOptionsAdapter();
            _spinnerTimeOption.ItemSelected += _spinnerTimeOption_ItemSelected;

            switch (ViewModel.Type)
            {
                case TaskOrEventType.Task:
                    switch (ViewModel.State)
                    {
                        case AddTaskOrEventViewModel.OperationState.Adding:
                            Title = PowerPlannerResources.GetString("String_AddTask").ToUpper();
                            break;

                        case AddTaskOrEventViewModel.OperationState.Editing:
                            Title = PowerPlannerResources.GetString("String_EditTask").ToUpper();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                    break;

                case TaskOrEventType.Event:
                    switch (ViewModel.State)
                    {
                        case AddTaskOrEventViewModel.OperationState.Adding:
                            Title = PowerPlannerResources.GetString("String_AddEvent").ToUpper();
                            break;

                        case AddTaskOrEventViewModel.OperationState.Editing:
                            Title = PowerPlannerResources.GetString("String_EditEvent").ToUpper();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                    break;
            }

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            SetMenu(Resource.Menu.add_taskorevent_menu);

            if (ViewModel.Name.Length == 0)
            {
                KeyboardHelper.FocusAndShow(_editTextName);
            }
        }

        private async void AddTaskOrEventView_RequestedAddImage(object sender, EventArgs e)
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

            var repeatsSubControl = new AddTaskOrEventRepeatsSubControl(_recurrenceControlContainer)
            {
                DataContext = ViewModel,
                LayoutParameters = new FrameLayout.LayoutParams(
                    FrameLayout.LayoutParams.MatchParent,
                    FrameLayout.LayoutParams.WrapContent)
            };
            _recurrenceControlContainer.AddView(repeatsSubControl);
        }

        private void AssignWeightCategoriesAdapter()
        {
            if (ViewModel.WeightCategories != null)
            {
                _spinnerWeight.Adapter = ObservableAdapter.Create<ViewItemWeightCategory>(
                       ViewModel.WeightCategories,
                       itemResourceId: Resource.Layout.SpinnerItemTaskOrEventWeightCategoryPreview,
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
                // This is null in the case where it's a semester task, not a class task
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