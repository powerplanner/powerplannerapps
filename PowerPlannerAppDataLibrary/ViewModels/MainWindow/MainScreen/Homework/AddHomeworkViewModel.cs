using BareMvvm.Core.ViewModels;
using StorageEverywhere;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.Controls;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.ImageAttachments;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework
{
    public class AddHomeworkViewModel : BaseMainScreenViewModelChild
    {
        public IList<ViewItemClass> Classes { get; private set; }

        public enum OperationState { Adding, Editing }

        public OperationState State { get; private set; }

        protected override bool InitialAllowLightDismissValue => false;

        private TaskOrEventType _type;
        public TaskOrEventType Type
        {
            get { return _type; }
            private set
            {
                if (value == TaskOrEventType.Task)
                {
                    TimeOption_BeforeClass = PowerPlannerResources.GetString("TimeOption_BeforeClass");
                    TimeOption_StartOfClass = PowerPlannerResources.GetString("TimeOption_StartOfClass");
                    TimeOption_DuringClass = PowerPlannerResources.GetString("TimeOption_DuringClass");
                    TimeOption_EndOfClass = PowerPlannerResources.GetString("TimeOption_EndOfClass");
                    TimeOption_AllDay = PowerPlannerResources.GetString("TimeOption_EndOfDay");
                    TimeOption_Custom = PowerPlannerResources.GetString("TimeOption_CustomDueTime");
                }
                else
                {
                    TimeOption_DuringClass = PowerPlannerResources.GetString("TimeOption_DuringClass");
                    TimeOption_AllDay = PowerPlannerResources.GetString("TimeOption_AllDay");
                    TimeOption_Custom = PowerPlannerResources.GetString("TimeOption_CustomTime");
                }

                _type = value;
            }
        }

        public override string GetPageName()
        {
            if (State == OperationState.Adding)
            {
                if (Type == TaskOrEventType.Task)
                {
                    return "AddTaskView";
                }
                else
                {
                    return "AddEventView";
                }
            }
            else
            {
                if (Type == TaskOrEventType.Task)
                {
                    return "EditTaskView";
                }
                else
                {
                    return "EditEventView";
                }
            }
        }

        public string TimeOption_AllDay;
        public string TimeOption_BeforeClass;
        public string TimeOption_StartOfClass;
        public string TimeOption_EndOfClass;
        public string TimeOption_DuringClass;
        public string TimeOption_Custom;

        public class AddParameter
        {
            public Guid SemesterIdentifier { get; set; }

            public TaskOrEventType Type { get; set; }

            public DateTime? DueDate { get; set; }

            public IList<ViewItemClass> Classes { get; set; }

            /// <summary>
            /// Can be null
            /// </summary>
            public ViewItemClass SelectedClass { get; set; }

            public bool HideClassPicker { get; set; }

            public bool HideDatePicker { get; set; }
        }

        public class EditParameter
        {
            public ViewItemTaskOrEvent Item { get; set; }
        }

        public AccountDataItem Account { get; private set; }

        public AddParameter AddParams { get; private set; }
        public EditParameter EditParams { get; private set; }

        private AddHomeworkViewModel(BaseViewModel parent) : base(parent)
        {
        }

        public static AddHomeworkViewModel CreateForAdd(BaseViewModel parent, AddParameter addParams)
        {
            AccountDataItem account = parent.FindAncestor<MainWindowViewModel>()?.CurrentAccount;
            if (account == null)
            {
                throw new NullReferenceException("CurrentAccount was null");
            }

            if (addParams.Classes.Count == 0)
            {
                throw new InvalidOperationException("No classes");
            }

            bool intelligentlyPickDate = true;
            DateTime now = DateTime.Now;

            IList<ViewItemClass> classes = GetClassesWithNoClassClass(addParams.Classes);

            ViewItemClass c = addParams.SelectedClass;
            if (c == null)
            {
                if (addParams.Type == TaskOrEventType.Task || addParams.Type == TaskOrEventType.Event)
                {
                    var prevClassIdentifier = NavigationManager.GetPreviousAddItemClass();
                    if (prevClassIdentifier != null)
                    {
                        // Remember user's selection
                        c = classes.FirstOrDefault(i => i.Identifier == prevClassIdentifier);
                    }

                    if (c == null)
                    {
                        // If date is specified
                        if (addParams.DueDate != null)
                        {
                            // If today
                            if (addParams.DueDate.Value.Date == now.Date)
                            {
                                // Pick currently going on class
                                c = PowerPlannerApp.GetClosestClassBasedOnSchedule(now, account, addParams.Classes);
                            }

                            // If there wasn't a class going on (or wasn't today), pick first class on that day
                            if (c == null)
                            {
                                c = PowerPlannerApp.GetFirstClassOnDay(addParams.DueDate.Value, account, addParams.Classes);
                            }
                        }

                        // Otherwise
                        else
                        {
                            // Intelligently pick based on schedule
                            c = PowerPlannerApp.GetClosestClassBasedOnSchedule(now, account, addParams.Classes);
                        }
                    }

                    if (c == null)
                    {
                        // If there wasn't a class and we're just doing the dummy pick first,
                        // don't intelligently pick class date
                        intelligentlyPickDate = false;
                        c = classes.First();
                    }
                }
                else
                {
                    // Tasks don't have classes
                    intelligentlyPickDate = false;
                }
            }

            DateTime date;
            if (addParams.DueDate != null)
            {
                date = addParams.DueDate.Value;
            }
            else
            {
                var prevDate = NavigationManager.GetPreviousAddItemDate();
                if (prevDate != null)
                {
                    date = prevDate.Value;
                }
                else
                {
                    DateTime? nextClassDate = null;

                    if (intelligentlyPickDate)
                    {
                        nextClassDate = PowerPlannerApp.GetNextClassDate(account, c);
                    }

                    if (nextClassDate != null)
                    {
                        date = nextClassDate.Value;
                    }
                    else
                    {
                        date = DateTime.Today;
                    }
                }
            }

            return new AddHomeworkViewModel(parent)
            {
                Account = account,
                State = OperationState.Adding,
                AddParams = addParams,
                Classes = classes,
                Date = date.Date,
                Type = addParams.Type,
                IsClassPickerVisible = !addParams.HideClassPicker,
                IsWeightCategoryPickerVisible = true,
                ImageAttachments = new ObservableCollection<BaseEditingImageAttachmentViewModel>(),
                Class = c // Assign class last, since it also assigns weight categories, and updates time options from remembered times
            };
        }

        private static IList<ViewItemClass> GetClassesWithNoClassClass(IList<ViewItemClass> normalClasses)
        {
            List<ViewItemClass> classes = new List<ViewItemClass>(normalClasses);
            classes.Add(normalClasses.First().Semester.NoClassClass);
            return classes;
        }

        public static AddHomeworkViewModel CreateForEdit(BaseViewModel parent, EditParameter editParams)
        {
            AccountDataItem account = parent.FindAncestor<MainWindowViewModel>()?.CurrentAccount;
            if (account == null)
            {
                throw new NullReferenceException("CurrentAccount was null");
            }
            ViewItemClass c = editParams.Item.Class;
            TaskOrEventType type = editParams.Item.Type;

            if (c == null)
            {
                throw new NullReferenceException("Class of the item was null. Item id " + editParams.Item.Identifier);
            }

            if (c.Semester == null)
            {
                throw new NullReferenceException("Semester of the class was null. Item id " + editParams.Item.Identifier);
            }

            if (c.Semester.Classes == null)
            {
                throw new NullReferenceException("Classes of the semester was null. Item id " + editParams.Item.Identifier);
            }

            var model = new AddHomeworkViewModel(parent)
            {
                Account = account,
                State = OperationState.Editing,
                EditParams = editParams,
                Name = editParams.Item.Name,
                Classes = GetClassesWithNoClassClass(c.Semester.Classes),
                Date = editParams.Item.Date.Date,
                Details = editParams.Item.Details,
                Type = type,
                ImageNames = editParams.Item.ImageNames.ToArray(),
                Class = c // Assign class last, since it also assigns weight categories
            };

            // Assign existing image attachments
            model.ImageAttachments = new ObservableCollection<BaseEditingImageAttachmentViewModel>(editParams.Item.ImageNames.Select(i => new EditingExistingImageAttachmentViewModel(model, i)));

            switch (editParams.Item.GetActualTimeOption())
            {
                case DataItemMegaItem.TimeOptions.AllDay:
                    model.SelectedTimeOption = model.TimeOption_AllDay;
                    break;

                case DataItemMegaItem.TimeOptions.BeforeClass:
                    model.SelectedTimeOption = model.TimeOption_BeforeClass;
                    break;

                case DataItemMegaItem.TimeOptions.Custom:
                    model._startTime = new TimeSpan(editParams.Item.Date.Hour, editParams.Item.Date.Minute, 0);
                    model._endTime = editParams.Item.EndTime.TimeOfDay;
                    model.SelectedTimeOption = model.TimeOption_Custom;
                    break;

                case DataItemMegaItem.TimeOptions.DuringClass:
                    model.SelectedTimeOption = model.TimeOption_DuringClass;
                    break;

                case DataItemMegaItem.TimeOptions.EndOfClass:
                    model.SelectedTimeOption = model.TimeOption_EndOfClass;
                    break;

                case DataItemMegaItem.TimeOptions.StartOfClass:
                    model.SelectedTimeOption = model.TimeOption_StartOfClass;
                    break;
            }

            // We don't want to consider setting the initial time option as the user configuring the time option
            model._userChangedSelectedTimeOption = false;

            return model;
        }

        private ViewItemWeightCategory[] GetWeightCategories(ViewItemClass c)
        {
            if (c.WeightCategories == null)
            {
                string pageCrumbsInfo = "";
                try
                {
                    var topParent = this.GetRootParent();
                    pageCrumbsInfo = string.Join(",", topParent.GetDescendants());
                }
                catch { }

                throw new NullReferenceException("ViewItemClass.WeightCategories was null. ClassId: " + c.Identifier + ". PageCrumbs: " + pageCrumbsInfo);
            }

            List<ViewItemWeightCategory> answer = new List<ViewItemWeightCategory>(c.WeightCategories.Count + 2);

            answer.Add(ViewItemWeightCategory.UNASSIGNED);

            answer.AddRange(c.WeightCategories);

            answer.Add(ViewItemWeightCategory.EXCLUDED);

            return answer.ToArray();
        }

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value, nameof(Name)); }
        }

        private DateTime _date = DateTime.Today;
        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, value, nameof(Date)); UpdateTimeOptions(); }
        }

        private ViewItemClass _class;

        public ViewItemClass Class
        {
            get { return _class; }
            set
            {
                if (value == _class)
                {
                    return;
                }

                SetProperty(ref _class, value, nameof(Class));
                UpdateWeightCategories();

                IsWeightCategoryPickerVisible = !_class.IsNoClassClass;

                _programmaticallyChangingTimeOptions = true;
                UpdateTimeOptions();

                // If we're adding, not editing
                if (AddParams != null)
                {
                    // And if the user hasn't changed the time options themselves
                    if (!_userChangedTimeOptions)
                    {
                        // Auto-adjust the time options based on last time options used for that class
                        if (Type == TaskOrEventType.Task)
                        {
                            if (value.DataItem.LastTaskDueTime != null)
                            {
                                StartTime = value.DataItem.LastTaskDueTime.Value;
                            }

                            if (value.DataItem.LastTaskTimeOption != null && TimeOptions.Contains(value.DataItem.LastTaskTimeOption))
                            {
                                SelectedTimeOption = value.DataItem.LastTaskTimeOption;
                            }
                        }
                        else
                        {
                            if (value.DataItem.LastEventStartTime != null)
                            {
                                StartTime = value.DataItem.LastEventStartTime.Value;
                            }

                            if (value.DataItem.LastEventDuration != null)
                            {
                                EndTime = StartTime + value.DataItem.LastEventDuration.Value;
                            }

                            if (value.DataItem.LastEventTimeOption != null && TimeOptions.Contains(value.DataItem.LastEventTimeOption))
                            {
                                SelectedTimeOption = value.DataItem.LastEventTimeOption;
                            }
                        }
                    }
                }
                _programmaticallyChangingTimeOptions = false;
            }
        }

        private string _details = "";

        public string Details
        {
            get { return _details; }
            set { SetProperty(ref _details, value, nameof(Details)); }
        }

        private ViewItemWeightCategory[] _weightCategories;
        public ViewItemWeightCategory[] WeightCategories
        {
            get { return _weightCategories; }
            set { SetProperty(ref _weightCategories, value, nameof(WeightCategories)); }
        }

        private ViewItemWeightCategory _selectedWeightCategory;
        public ViewItemWeightCategory SelectedWeightCategory
        {
            get { return _selectedWeightCategory; }
            set { SetProperty(ref _selectedWeightCategory, value, nameof(SelectedWeightCategory)); }
        }

        public string[] TimeOptions { get; private set; } = new string[0];

        private bool _userChangedSelectedTimeOption;
        private string _selectedTimeOption;
        public string SelectedTimeOption
        {
            get { return _selectedTimeOption; }
            set
            {
                if (!_programmaticallyChangingTimeOptions)
                {
                    _userChangedSelectedTimeOption = true;
                }

                if (!TimeOptions.Contains(value))
                {
                    // If value not found, don't change current value (but raise property changed so listener changes back)
                    OnPropertyChanged(nameof(SelectedTimeOption));
                    return;
                }

                if (_selectedTimeOption != value)
                {
                    if (_selectedTimeOption != null)
                    {
                        _userChangedSelectedTimeOption = true;
                    }

                    SetProperty(ref _selectedTimeOption, value, nameof(SelectedTimeOption), nameof(IsStartTimePickerVisible), nameof(IsEndTimePickerVisible));
                }
            }
        }

        public string[] ImageNames { get; set; }

        private List<EditingExistingImageAttachmentViewModel> _removedImageAttachments = new List<EditingExistingImageAttachmentViewModel>();
        public ObservableCollection<BaseEditingImageAttachmentViewModel> ImageAttachments { get; private set; }

        public async Task AddNewImageAttachmentAsync()
        {
            try
            {
                if (!(await PowerPlannerApp.Current.IsFullVersionAsync()) && ImageAttachments.Count >= 1)
                {
                    PowerPlannerApp.Current.PromptPurchase("The free version only lets you attach one photo per item. Would you like to upgrade to premium and attach unlimited photos per item?");
                    return;
                }

                if (ImagePickerExtension.Current == null)
                {
                    throw new PlatformNotSupportedException("ImagePickerExtension wasn't implemented");
                }

                IFile[] files = await ImagePickerExtension.Current.PickImagesAsync();
                foreach (var file in files)
                {
                    ImageAttachments.Add(new EditingNewImageAttachmentViewModel(this, file));
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void RemoveImageAttachment(BaseEditingImageAttachmentViewModel imageAttachment)
        {
            ImageAttachments.Remove(imageAttachment);
            if (imageAttachment is EditingExistingImageAttachmentViewModel existing)
            {
                _removedImageAttachments.Add(existing);
            }
        }

        public bool IsClassPickerVisible { get; private set; } = true;

        public bool IsDatePickerVisible { get { return true; } }

        public bool IsStartTimePickerVisible
        {
            get { return SelectedTimeOption == TimeOption_Custom; }
        }

        private bool _isWeightCategoryPickerVisible = true;
        public bool IsWeightCategoryPickerVisible
        {
            get { return _isWeightCategoryPickerVisible; }
            private set { SetProperty(ref _isWeightCategoryPickerVisible, value, nameof(IsWeightCategoryPickerVisible)); }
        }

        public bool IsEndTimePickerVisible
        {
            get { return IsStartTimePickerVisible && Type != TaskOrEventType.Task; }
        }

        private bool _userChangedTimeOptions;
        private bool _programmaticallyChangingTimeOptions;

        private TimeSpan _startTime = TimeSpan.FromHours(9);
        public TimeSpan StartTime
        {
            get { return _startTime; }
            set
            {
                if (!_programmaticallyChangingTimeOptions)
                {
                    _userChangedTimeOptions = true;
                }

                if (value == _startTime)
                    return;

                if (value.TotalHours > 24)
                    value = TimeSpan.FromHours(24);

                TimeSpan diff = EndTime - StartTime;

                SetProperty(ref _startTime, value, nameof(StartTime));

                if (Type == TaskOrEventType.Event)
                {
                    var desiredEndTime = StartTime + diff;
                    if (desiredEndTime.TotalHours > 24)
                        desiredEndTime = new TimeSpan(23, 59, 0);

                    _endTime = desiredEndTime;
                    OnPropertyChanged(nameof(EndTime));
                }
            }
        }

        private TimeSpan _endTime = new TimeSpan(9, 50, 0);
        public TimeSpan EndTime
        {
            get { return _endTime; }
            set
            {
                if (!_programmaticallyChangingTimeOptions)
                {
                    _userChangedSelectedTimeOption = true;
                }

                if (value == _endTime)
                    return;

                if (value.TotalHours > 24)
                    value = TimeSpan.FromHours(24);

                TimeSpan diff = StartTime - EndTime;

                SetProperty(ref _endTime, value, nameof(EndTime));

                if (EndTime < StartTime)
                {
                    _startTime = EndTime + diff;
                    if (_startTime.TotalHours < 0)
                        _startTime = TimeSpan.FromHours(0);

                    OnPropertyChanged(nameof(StartTime));
                }
            }
        }

        /// <summary>
        /// This is only true when adding
        /// </summary>
        public bool IsRepeatsVisible => State == OperationState.Adding;

        private bool _repeats;
        public bool Repeats
        {
            get { return _repeats; }
            set
            {
                if (RecurrenceControlViewModel == null)
                {

                    RecurrenceControlViewModel = new RecurrenceControlViewModel()
                    {
                        // Default to repeat interval of once weekly, since probably most popular
                        RepeatIntervalAsString = "1",
                        SelectedRepeatOption = RecurrenceControlViewModel.RepeatOptions.Weekly,

                        // Should end 6 months from now
                        EndDate = Date.Date.AddMonths(6),

                        // Occurrences is probably more popular, so use that
                        SelectedEndOption = RecurrenceControlViewModel.EndOptions.Occurrences,

                        // We'll default to 5 occurrences
                        EndOccurrencesAsString = "5"
                    };

                    // We'll make sure this day of week is selected
                    RecurrenceControlViewModel.DayCheckBoxes.First(i => i.DayOfWeek == Date.DayOfWeek).IsChecked = true;

                    var dontWait = UpdateRepeatsPremiumStatusAsync();
                }

                SetProperty(ref _repeats, value, nameof(Repeats));
            }
        }

        private async Task UpdateRepeatsPremiumStatusAsync()
        {
            try
            {
                // If premium
                if (await PowerPlannerApp.Current.IsFullVersionAsync())
                {
                    IsRepeatingEntryEnabled = true;
                    ShowRepeatingPremiumTrial = false;
                    ShowRepeatingMustUpgradeToPremium = false;
                }

                // Otherwise we let them try it once
                else if (!Account.HasAddedRepeating)
                {
                    IsRepeatingEntryEnabled = true;
                    ShowRepeatingPremiumTrial = true;
                    ShowRepeatingMustUpgradeToPremium = false;
                }

                // Otherwise it's disabled and must upgrade
                else
                {
                    IsRepeatingEntryEnabled = false;
                    ShowRepeatingPremiumTrial = false;
                    ShowRepeatingMustUpgradeToPremium = true;
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private bool _showRepeatingPremiumTrial;
        public bool ShowRepeatingPremiumTrial
        {
            get { return _showRepeatingPremiumTrial; }
            private set { SetProperty(ref _showRepeatingPremiumTrial, value, nameof(ShowRepeatingPremiumTrial)); }
        }

        private bool _showRepeatingMustUpgradeToPremium;
        public bool ShowRepeatingMustUpgradeToPremium
        {
            get { return _showRepeatingMustUpgradeToPremium; }
            private set { SetProperty(ref _showRepeatingMustUpgradeToPremium, value, nameof(ShowRepeatingMustUpgradeToPremium)); }
        }

        private bool _isRepeatingEntryEnabled;
        public bool IsRepeatingEntryEnabled
        {
            get { return _isRepeatingEntryEnabled; }
            private set { SetProperty(ref _isRepeatingEntryEnabled, value, nameof(IsRepeatingEntryEnabled)); }
        }

        public RecurrenceControlViewModel RecurrenceControlViewModel { get; private set; }

        public void UpgradeToPremiumForRepeating()
        {
            TelemetryExtension.Current?.TrackEvent("Action_UpgradeToPremiumForRepeating");

            PowerPlannerApp.Current.PromptPurchase(null);
        }

        public override async void OnViewFocused()
        {
            base.OnViewFocused();

            // Update premium status on repeat control if they ended up buying
            if (ShowRepeatingMustUpgradeToPremium)
            {
                await UpdateRepeatsPremiumStatusAsync();

                if (!ShowRepeatingMustUpgradeToPremium)
                {
                    // They upgraded!
                    TelemetryExtension.Current?.TrackEvent("Action_UpgradedToPremiumForRepeating");
                }
            }
        }

        private void UpdateTimeOptions()
        {
            if (Class == null)
            {
                return;
            }

            if (Class.IsNoClassClass || !PowerPlannerApp.DoesClassOccurOnDate(Account, Date, Class))
            {
                MakeTimeOptionsLike(new string[]
                {
                    TimeOption_AllDay,
                    TimeOption_Custom
                });
            }
            else
            {
                switch (Type)
                {
                    case TaskOrEventType.Task:
                        MakeTimeOptionsLike(new string[]
                        {
                            TimeOption_BeforeClass,
                            TimeOption_StartOfClass,
                            TimeOption_DuringClass,
                            TimeOption_EndOfClass,
                            TimeOption_AllDay,
                            TimeOption_Custom
                        });
                        break;

                    case TaskOrEventType.Event:
                        MakeTimeOptionsLike(new string[]
                        {
                            TimeOption_AllDay,
                            TimeOption_DuringClass,
                            TimeOption_Custom
                        });
                        break;
                }
            }

            if (!TimeOptions.Contains(SelectedTimeOption))
            {
                if (Type == TaskOrEventType.Event && TimeOptions.Length == 3)
                {
                    SelectedTimeOption = TimeOptions[1];
                }
                else
                {
                    SelectedTimeOption = TimeOptions.First();
                }

                _userChangedSelectedTimeOption = false;
            }
            else
            {
                OnPropertyChanged(nameof(SelectedTimeOption));
            }
        }

        private void MakeTimeOptionsLike(string[] newOptions)
        {
            if (TimeOptions.SequenceEqual(newOptions))
            {
                return;
            }

            TimeOptions = newOptions;
            OnPropertyChanged(nameof(TimeOptions));
        }

        private void UpdateWeightCategories()
        {
            if (Class == null || Class.IsNoClassClass)
            {
                return;
            }

            WeightCategories = GetWeightCategories(Class);

            if (SelectedWeightCategory == null || !WeightCategories.Contains(SelectedWeightCategory))
            {
                ViewItemWeightCategory selectedWeightCategory = null;
                if (EditParams != null)
                {
                    selectedWeightCategory = WeightCategories.FirstOrDefault(i => i.Identifier == EditParams.Item.WeightCategoryIdentifier);
                }
                else
                {
                    selectedWeightCategory = WeightCategories.FirstOrDefault(i => i.Identifier == NavigationManager.SelectedWeightCategoryIdentifier);
                }

                if (selectedWeightCategory == null)
                {
                    SelectedWeightCategory = ViewItemWeightCategory.UNASSIGNED;
                }
                else
                {
                    SelectedWeightCategory = selectedWeightCategory;
                }
            }
        }

        public async void Save()
        {
            try
            {
                string name = Name.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    await new PortableMessageDialog(PowerPlannerResources.GetStringNoNameMessageBody(), PowerPlannerResources.GetStringNoNameMessageHeader()).ShowAsync();
                    return;
                }

                if (Class == null)
                {
                    await new PortableMessageDialog(PowerPlannerResources.GetStringNoClassMessageBody(), PowerPlannerResources.GetStringNoClassMessageHeader()).ShowAsync();
                    return;
                }

                List<DataItemMegaItem> bulkEntry = null;

                if (Repeats && IsRepeatingEntryEnabled)
                {
                    if (RecurrenceControlViewModel.ShowErrorIfInvalid())
                    {
                        return;
                    }

                    if (RecurrenceControlViewModel.EndDate.Date <= Date.Date)
                    {
                        TelemetryExtension.Current?.TrackEvent("UserError_InvalidRecurrence", new Dictionary<string, string>()
                        {
                            { "Error", "EndDateNotGreaterThanStart" },
                            { "UserData", "Start: " + Date.ToString("d") + " End: " + RecurrenceControlViewModel.EndDate.ToString("d") }
                        });

                        await new PortableMessageDialog("Your end date must be greater than the date that this series starts on.", "Repeating occurrence invalid").ShowAsync();
                        return;
                    }

                    bulkEntry = new List<DataItemMegaItem>();

                    foreach (var currDate in RecurrenceControlViewModel.GetEnumerableDates(Date.Date))
                    {
                        if (bulkEntry.Count >= 50)
                        {
                            TelemetryExtension.Current?.TrackEvent("UserError_TooManyOccurrences", new Dictionary<string, string>()
                            {
                                { "Error", "EndDateTooFarOut" },
                                { "UserData",  $"Date: {Date.ToString("d")} {RecurrenceControlViewModel}" }
                            });

                            await new PortableMessageDialog("The repeating end date you selected is too far into the future such that greater than 50 occurrences would be created. Please select a closer end date.", "Repeating occurrence invalid").ShowAsync();
                            return;
                        }

                        bulkEntry.Add(CreateDataItem(currDate));
                    }
                }

                DataChanges changes = new DataChanges();

                // If we're adding
                if (AddParams != null)
                {
                    try
                    {
                        if (Class.IsNoClassClass)
                        {
                            // Save into a single shared storage object (the semester overrides the data item and facilitates storing these properly)
                            PopulateClassSavedInfo(Class.DataItem);
                        }
                        else
                        {
                            // Save into the class itself
                            changes.Add(CreateClassSavedInfoDataItem());

                            // And ignore updating calendar integration on that class edit, since it doesn't affect calendar integration
                            changes.IgnoreEditedClassIdentifierFromCalendarIntegration(Class.Identifier);
                        }
                    }
                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                }

                DataItemMegaItem dataItem;
                bool repeating = false;
                if (bulkEntry != null && bulkEntry.Count > 0)
                {
                    dataItem = bulkEntry[0];

                    foreach (var item in bulkEntry)
                    {
                        changes.Add(item);
                    }

                    repeating = true;
                }

                // Otherwise not repeating, create item as normal
                else
                {
                    dataItem = CreateDataItem(Date.Date);
                    changes.Add(dataItem);
                }

                TryStartDataOperationAndThenNavigate(async delegate
                {
                    if (!repeating)
                    {
                        string[] updatedImageNames = await SaveImageAttachmentsAsync();
                        if (updatedImageNames != null)
                        {
                            dataItem.ImageNames = updatedImageNames;
                        }
                    }

                    await PowerPlannerApp.Current.SaveChanges(changes);

                    // Non-critical code
                    try
                    {
                        NavigationManager.SetPreviousAddItemClass(dataItem.UpperIdentifier);

                        if (!Class.IsNoClassClass)
                        {
                            NavigationManager.SelectedWeightCategoryIdentifier = dataItem.WeightCategoryIdentifier;
                        }

                        NavigationManager.SetPreviousAddItemDate(Date.Date);

                        if (SelectedTimeOption == TimeOption_AllDay)
                        {
                            TrackTimeOption("AllDay");
                        }
                        else if (SelectedTimeOption == TimeOption_BeforeClass)
                        {
                            TrackTimeOption("BeforeClass");
                        }
                        else if (SelectedTimeOption == TimeOption_Custom)
                        {
                            TrackTimeOption("Custom");
                        }
                        else if (SelectedTimeOption == TimeOption_DuringClass)
                        {
                            TrackTimeOption("DuringClass");
                        }
                        else if (SelectedTimeOption == TimeOption_EndOfClass)
                        {
                            TrackTimeOption("EndOfClass");
                        }
                        else if (SelectedTimeOption == TimeOption_StartOfClass)
                        {
                            TrackTimeOption("StartOfClass");
                        }

                        if (_userChangedSelectedTimeOption)
                        {
                            TelemetryExtension.Current?.TrackEvent("Action_CustomizedTimeOption");
                        }

                        if (bulkEntry != null && RecurrenceControlViewModel != null)
                        {
                            if (!Account.HasAddedRepeating)
                            {
                                Account.HasAddedRepeating = true;
                                await AccountsManager.Save(Account);
                            }

                            TelemetryExtension.Current?.TrackEvent("Action_RecurringBulkEntry", new Dictionary<string, string>()
                            {
                                { "RepeatInterval", RecurrenceControlViewModel.GetRepeatIntervalAsNumber().ToString() },
                                { "RepeatType", RecurrenceControlViewModel.SelectedRepeatOption.ToString() },
                                { "EndType", RecurrenceControlViewModel.SelectedEndOption.ToString() },
                                { "Occurrences", bulkEntry.Count.ToString() }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // Non-critical exception
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                }, delegate
                {
                    this.RemoveViewModel();
                });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async Task<string[]> SaveImageAttachmentsAsync()
        {
            var newImages = ImageAttachments.OfType<EditingNewImageAttachmentViewModel>().ToArray();
            if (_removedImageAttachments.Count > 0 || newImages.Length > 0)
            {
                var currAccount = Account;

                if (currAccount == null)
                    throw new NullReferenceException("Account was null");

                IFolder imagesFolderPortable = await FileHelper.GetOrCreateImagesFolder(currAccount.LocalAccountId);

                List<string> finalImageNames = ImageAttachments.Select(i => i.ImageAttachment.ImageName).ToList();

                // Delete images
                foreach (var removedImage in _removedImageAttachments)
                {
                    try
                    {
                        var file = await imagesFolderPortable.GetFileAsync(removedImage.ImageAttachment.ImageName);
                        await file.DeleteAsync();
                    }

                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                }

                // Add new images
                foreach (var newImage in newImages)
                {
                    try
                    {
                        await newImage.TempFile.MoveAsync(Path.Combine(imagesFolderPortable.Path, newImage.TempFile.Name), NameCollisionOption.ReplaceExisting);
                        if (newImage.TempFile is Helpers.TempFile tempFile)
                        {
                            tempFile.DetachTempDisposer();
                        }
                    }

                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);

                        try
                        {
                            finalImageNames.Remove(newImage.ImageAttachment.ImageName);
                        }
                        catch { }
                    }
                }

                // If there were new images, add them to the needing upload list
                var newImageNames = newImages.Select(i => i.ImageAttachment.ImageName).Intersect(finalImageNames).ToArray();
                if (newImageNames.Any())
                {
                    var dataStore = await AccountDataStore.Get(currAccount.LocalAccountId);
                    await dataStore.AddImagesToUploadAsync(newImageNames);
                }

                return finalImageNames.ToArray();
            }
            else
            {
                return null;
            }
        }

        private DataItemClass CreateClassSavedInfoDataItem()
        {
            DataItemClass dataItem = new DataItemClass()
            {
                Identifier = Class.Identifier
            };

            PopulateClassSavedInfo(dataItem);

            return dataItem;
        }

        private void PopulateClassSavedInfo(DataItemClass dataItem)
        {
            switch (Type)
            {
                case TaskOrEventType.Task:
                    dataItem.LastTaskTimeOption = SelectedTimeOption;
                    dataItem.LastTaskDueTime = StartTime;
                    break;

                case TaskOrEventType.Event:
                    dataItem.LastEventTimeOption = SelectedTimeOption;
                    dataItem.LastEventStartTime = StartTime;
                    dataItem.LastEventDuration = EndTime - StartTime;
                    break;

                default:
                    throw new NotImplementedException("Unknown item type");
            }
        }

        private DataItemMegaItem CreateDataItem(DateTime date)
        {
            DataItemMegaItem dataItem = new DataItemMegaItem();

            switch (Type)
            {
                case TaskOrEventType.Event:
                    if (Class.IsNoClassClass)
                    {
                        dataItem.MegaItemType = PowerPlannerSending.MegaItemType.Event;
                    }
                    else
                    {
                        dataItem.MegaItemType = PowerPlannerSending.MegaItemType.Exam;
                    }
                    break;

                case TaskOrEventType.Task:
                    if (Class.IsNoClassClass)
                    {
                        dataItem.MegaItemType = PowerPlannerSending.MegaItemType.Task;
                    }
                    else
                    {
                        dataItem.MegaItemType = PowerPlannerSending.MegaItemType.Homework;
                    }
                    break;

                default:
                    throw new NotImplementedException("Unknown type");
            }

            if (AddParams != null)
            {
                dataItem.Identifier = Guid.NewGuid();
            }

            else
            {
                dataItem.Identifier = EditParams.Item.Identifier;
            }

            dataItem.Name = Name;
            dataItem.Date = DateTime.SpecifyKind(date, DateTimeKind.Utc).Date;
            dataItem.Details = Details.Trim();

            dataItem.UpperIdentifier = Class.Identifier;
            if (Class.IsNoClassClass)
            {
                dataItem.WeightCategoryIdentifier = Guid.Empty;
            }
            else
            {
                dataItem.WeightCategoryIdentifier = SelectedWeightCategory.Identifier;
            }

            // 00 seconds represents default
            // - Homework: Start of class
            // - Exam
            //   - If no end time: During class
            //   - Otherwise
            //     - If end time is 23:59:59: All day
            //     - Else: Custom time

            // 01 seconds represents
            // - Homework: Before class

            // 02 seconds represents
            // - Homework: During class (currently not supported, might add in future)

            // 03 seconds represents
            // - Homework: End of class

            // 04 seconds represents
            // - Homework: Custom time

            // 23:59:59 represents
            // - Homework: End of day

            switch (Type)
            {
                case TaskOrEventType.Task:
                    if (SelectedTimeOption == TimeOption_BeforeClass)
                    {
                        dataItem.Date = dataItem.Date.AddSeconds(1);
                    }
                    else if (SelectedTimeOption == TimeOption_DuringClass)
                    {
                        dataItem.Date = dataItem.Date.AddSeconds(2);
                    }
                    else if (SelectedTimeOption == TimeOption_EndOfClass)
                    {
                        dataItem.Date = dataItem.Date.AddSeconds(3);
                    }
                    else if (SelectedTimeOption == TimeOption_Custom)
                    {
                        dataItem.Date = dataItem.Date.Add(StartTime).AddSeconds(4);
                    }
                    else if (SelectedTimeOption == TimeOption_AllDay)
                    {
                        dataItem.Date = dataItem.Date.AddDays(1).AddSeconds(-1);
                    }
                    else /* Start of class */
                    {
                    }
                    break;

                case TaskOrEventType.Event:
                    if (SelectedTimeOption == TimeOption_Custom)
                    {
                        dataItem.Date = dataItem.Date.Add(StartTime);
                        dataItem.EndTime = dataItem.Date.Date.Add(EndTime);
                    }
                    else if (SelectedTimeOption == TimeOption_AllDay)
                    {
                        dataItem.EndTime = dataItem.Date.AddDays(1).AddSeconds(-1);
                    }
                    else /* during class */
                    {
                        dataItem.EndTime = PowerPlannerSending.DateValues.UNASSIGNED;
                    }
                    break;
            }

            dataItem.ImageNames = ImageNames;

            return dataItem;
        }

        private void TrackTimeOption(string name)
        {
            string itemType;
            if (Type == TaskOrEventType.Task)
            {
                itemType = "Task";
            }
            else
            {
                itemType = "Event";
            }

            TelemetryExtension.Current?.TrackEvent($"TimeOption_{itemType}_{name}");
        }
    }
}
