using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using ToolsPortable;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule
{
    public class ScheduleViewModel : BaseMainScreenViewModelChild, IDataChangedEventHandler
    {
        public SemesterItemsViewGroup SemesterViewItemsGroup { get; private set; }

        public enum LayoutModes
        {
            Normal,
            SplitEditing,
            FullEditing,
            Welcome
        }

        private LayoutModes _layoutMode = LayoutModes.Normal;

        public LayoutModes LayoutMode
        {
            get { return _layoutMode; }
            set { SetProperty(ref _layoutMode, value, nameof(LayoutMode)); }
        }

        /// <summary>
        /// Boolean that represents whether the returning user UI should be shown. This should never dynamically change since to create an account you have to
        /// go to the settings page, and returning to the schedule page re-creates the view model
        /// </summary>
        public bool IsReturningUserVisible => Account.IsDefaultOfflineAccount;

        public AccountDataItem Account { get; private set; }

        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        public DayOfWeek FirstDayOfWeek { get; private set; }

        public DateTime Today { get; private set; } = DateTime.Today;

        public DateTime StartDate { get; private set; }

        private bool _hasAllDayItems;
        public bool HasAllDayItems
        {
            get { return _hasAllDayItems; }
            private set { SetProperty(ref _hasAllDayItems, value, nameof(HasAllDayItems)); }
        }

        private void UpdateHasAllDayItems()
        {
            HasAllDayItems = Items.Values.Any(i => i.HolidayAndAllDayItems.Count > 0);
        }

        private bool _hasTwoWeekSchedule;
        public bool HasTwoWeekSchedule
        {
            get { return _hasTwoWeekSchedule; }
            private set { SetProperty(ref _hasTwoWeekSchedule, value, nameof(HasTwoWeekSchedule)); }
        }

        private void UpdateHasTwoWeekSchedule()
        {
            try
            {
                HasTwoWeekSchedule = SemesterViewItemsGroup.Classes.SelectMany(i => i.Schedules).Any(i => i.ScheduleWeek != PowerPlannerSending.Schedule.Week.BothWeeks);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public DateTime DisplayStartDate
        {
            get
            {
                if (StartDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    // If there's no items on Sunday, skip Sunday
                    if (Items.Count > 0 && !Items[DayOfWeek.Sunday].HasItems())
                    {
                        return StartDate.AddDays(1);
                    }
                }

                return StartDate;
            }
        }

        public DateTime DisplayEndDate
        {
            get
            {
                var endDate = StartDate.AddDays(6);
                if (endDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    // If there's no items on Saturday, skip Saturday
                    if (Items.Count > 0 && !Items[DayOfWeek.Saturday].HasItems())
                    {
                        return endDate.AddDays(-1);
                    }
                }

                return endDate;
            }
        }

        /// <summary>
        /// How many days the schedule should show (sat/sun get dropped if there's no items on those days)
        /// </summary>
        public int CountOfDaysToDisplay => CachedComputation(delegate
        {
            return (int)(DisplayEndDate - DisplayStartDate).TotalDays + 1;
        }, new string[] { nameof(DisplayStartDate), nameof(DisplayEndDate) });

        private void NotifyDisplayDatesChanged()
        {
            OnPropertyChanged(nameof(DisplayStartDate), nameof(DisplayEndDate), nameof(CurrentWeek));
        }

        public PowerPlannerSending.Schedule.Week CurrentWeek
        {
            get { return Account.GetWeekOnDifferentDate(StartDate); }
        }

        public event EventHandler<DateTime> OnItemsForDateChanged;
        public event EventHandler OnFullReset;

        private bool IsDisplayingNextWeek()
        {
            // TODO: Remove, no longer used
            //return CurrentWeek != _originalCurrentWeek;
            return false;
        }

        public void NextWeek()
        {
            StartDate = StartDate.AddDays(7);
            HandleWeekChanged();
        }

        public void PreviousWeek()
        {
            StartDate = StartDate.AddDays(-7);
            HandleWeekChanged();
        }

        private void HandleWeekChanged()
        {
            InitializeArrangers();

            UpdateHasTwoWeekSchedule();

            if (CalibrateMaxStartEndTimes())
            {
                // No need to notify, this already did
                NotifyDisplayDatesChanged();
            }
            else
            {
                for (int i = 0; i < 7; i++)
                {
                    NotifyItemsChangedForDate(StartDate.AddDays(i));
                }
            }
        }

        private bool HasSchedules()
        {
            foreach (var c in SemesterViewItemsGroup.Classes)
            {
                if (c.Schedules.Count > 0)
                    return true;
            }

            return false;
        }

        private bool HasClasses()
        {
            return SemesterViewItemsGroup.Classes.Count > 0;
        }

        public class Params
        {
            public bool IsEditing { get; set; }
        }

        private Params _modelParams;
        private WeakEventHandler<EventArgs> _arrangerItemsChangedHandler;
        private double _heightOfHour;
        private double _spacingWhenNoAdditionalItems;
        private double _spacingWithAdditionalItems;
        public double WidthOfCollapsed { get; set; }

        public ScheduleViewModel(BaseViewModel parent, Params modelParams = null) : base(parent)
        {
            SemesterItemsViewGroup semesterGroup = SemesterItemsViewGroup.Load(MainScreenViewModel.CurrentLocalAccountId, MainScreenViewModel.CurrentSemester, trackChanges: true);
            if (semesterGroup == null)
            {
                throw new NullReferenceException("semesterGroup was null");
            }
            if (semesterGroup.Semester == null)
            {
                throw new NullReferenceException("semesterGroup.Semester was null");
            }

            if (modelParams == null)
                modelParams = new Params();
            _modelParams = modelParams;

            _arrangerItemsChangedHandler = new WeakEventHandler<EventArgs>(Arranger_OnItemsChanged);

            SemesterViewItemsGroup = semesterGroup;

            var acct = MainScreenViewModel.CurrentAccount;
            Account = acct ?? throw new NullReferenceException("MainScreenViewModel.CurrentAccount was null");
            FirstDayOfWeek = acct.WeekChangesOn;

            DateTime today = DateTime.Today;
            if (semesterGroup.Semester.IsDateDuringThisSemester(today))
            {
                StartDate = DateTools.Last(FirstDayOfWeek);
            }
            else if (!PowerPlannerSending.DateValues.IsUnassigned(semesterGroup.Semester.Start) && today < semesterGroup.Semester.Start)
            {
                // Semester hasn't started yet, jump to first week of semester
                StartDate = DateTools.Last(FirstDayOfWeek, semesterGroup.Semester.Start);
            }
            else
            {
                // Semester already ended, jump to last week of semester
                StartDate = DateTools.Last(FirstDayOfWeek, semesterGroup.Semester.End);
            }

            if (!HasSchedules() || modelParams.IsEditing)
                EnterEditMode();

            AccountDataStore.AddDataChangedEventHandler(this);

            UpdateHasTwoWeekSchedule();
        }

        /// <summary>
        /// View needs to call this once, providing the size info
        /// </summary>
        /// <param name="heightOfHour"></param>
        /// <param name="spacingWhenNoAdditionalItems"></param>
        /// <param name="spacingWithAdditionalItems"></param>
        /// <param name="widthOfCollapsed"></param>
        public void InitializeArrangers(double heightOfHour, double spacingWhenNoAdditionalItems, double spacingWithAdditionalItems, double widthOfCollapsed)
        {
            _heightOfHour = heightOfHour;
            _spacingWhenNoAdditionalItems = spacingWhenNoAdditionalItems;
            _spacingWithAdditionalItems = spacingWithAdditionalItems;
            WidthOfCollapsed = widthOfCollapsed;

            bool isItemsLoaded = SemesterViewItemsGroup.LoadingTask.IsCompleted;

            InitializeArrangers();
        }

        private void InitializeArrangers()
        {
            foreach (var arranger in Items.Values)
            {
                arranger.OnItemsChanged -= _arrangerItemsChangedHandler.Handler;
            }

            DateTime date = StartDate;
            for (int i = 0; i < 7; i++, date = date.AddDays(1))
            {
                var arranger = DayScheduleItemsArranger.Create(
                    account: Account,
                    semesterItems: SemesterViewItemsGroup,
                    scheduleGroup: MainScreenViewModel.ScheduleViewItemsGroup,
                    date: date,
                    heightOfHour: _heightOfHour,
                    spacingWhenNoAdditionalItems: _spacingWhenNoAdditionalItems,
                    spacingWithAdditionalItems: _spacingWithAdditionalItems,
                    widthOfCollapsed: WidthOfCollapsed,
                    includeTasksAndEventsAndHolidays: true);

                arranger.OnItemsChanged += _arrangerItemsChangedHandler.Handler;

                Items[date.DayOfWeek] = arranger;
            }

            CalibrateMaxStartEndTimes();
            UpdateHasAllDayItems();
            NotifyDisplayDatesChanged();
        }

        private bool CalibrateMaxStartEndTimes()
        {
            TimeSpan min = TimeSpan.MaxValue;
            TimeSpan max = TimeSpan.MinValue;
            foreach (var arranger in Items.Values.Where(i => i.IsValid() && i.HasSpecificTimeItems()))
            {
                if (min == TimeSpan.MaxValue || arranger.StartTime < min)
                {
                    min = arranger.StartTime;
                }
                if (max == TimeSpan.MinValue || arranger.EndTime > max)
                {
                    max = arranger.EndTime;
                }
            }

            if (min == TimeSpan.MaxValue)
            {
                return false;
            }

            foreach (var arranger in Items.Values)
            {
                arranger.StartTime = min;
                arranger.EndTime = max;
                arranger.CalculateOffsets();
            }

            bool timesChanged = false;
            if (StartTime != min)
            {
                StartTime = min;
                timesChanged = true;
            }
            if (EndTime != max)
            {
                EndTime = max;
                timesChanged = true;
            }

            if (timesChanged)
            {
                NotifyFullReset();
            }

            return timesChanged;
        }

        public bool IsValid()
        {
            return EndTime >= StartTime && Items.Values.Any(i => i.IsValid());
        }

        private void NotifyFullReset()
        {
            OnFullReset?.Invoke(this, new EventArgs());
        }

        private void Arranger_OnItemsChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateHasTwoWeekSchedule();
                NotifyItemsChangedForDate((sender as DayScheduleItemsArranger).Date);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void NotifyItemsChangedForDate(DateTime date)
        {
            if (CalibrateMaxStartEndTimes())
            {
                // Nothing, full reset has been triggered
            }
            else
            {
                OnItemsForDateChanged?.Invoke(this, date);
            }

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                NotifyDisplayDatesChanged();
            }

            UpdateHasAllDayItems();
        }

        public DayScheduleItemsArranger MondayItems { get { return Items[DayOfWeek.Monday]; } }
        public DayScheduleItemsArranger TuesdayItems { get { return Items[DayOfWeek.Tuesday]; } }
        public DayScheduleItemsArranger WednesdayItems { get { return Items[DayOfWeek.Wednesday]; } }
        public DayScheduleItemsArranger ThursdayItems { get { return Items[DayOfWeek.Thursday]; } }
        public DayScheduleItemsArranger FridayItems { get { return Items[DayOfWeek.Friday]; } }
        public DayScheduleItemsArranger SaturdayItems { get { return Items[DayOfWeek.Saturday]; } }
        public DayScheduleItemsArranger SundayItems { get { return Items[DayOfWeek.Sunday]; } }

        public Dictionary<DayOfWeek, DayScheduleItemsArranger> Items { get; private set; } = new Dictionary<DayOfWeek, DayScheduleItemsArranger>();

        public void EnterEditMode()
        {
            UpdateEditMode();
        }

        private void UpdateEditMode()
        {
            if (HasSchedules())
                LayoutMode = LayoutModes.SplitEditing;
            else if (HasClasses())
                LayoutMode = LayoutModes.FullEditing;
            else
                LayoutMode = LayoutModes.Welcome;
        }

        public void ExitEditMode()
        {
            LayoutMode = LayoutModes.Normal;
        }

        public void AddClass()
        {
            MainScreenViewModel.AddClass();
        }

        public void AddTime(ViewItemClass c, bool useNewStyle = false)
        {
            BaseViewModel model = AddClassTimesViewModel.CreateForAdd(MainScreenViewModel, new AddClassTimesViewModel.AddParameter()
            {
                Class = c
            });

            MainScreenViewModel.ShowPopup(model);
        }

        /// <summary>
        /// Finds related schedules and edits them as a batch
        /// </summary>
        /// <param name="schedule"></param>
        public void EditTimes(ViewItemSchedule schedule, bool useNewStyle = false)
        {
            var group = schedule.Class.GetGroupOfSchedulesWithSharedEditingValues(schedule);
            EditTimes(group, useNewStyle);
        }

        public void EditTimes(ViewItemSchedule[] schedules, bool useNewStyle = false)
        {
            BaseViewModel model = AddClassTimesViewModel.CreateForEdit(MainScreenViewModel, new AddClassTimesViewModel.EditParameter()
            {
                GroupedSchedules = schedules
            });

            MainScreenViewModel.ShowPopup(model);
        }

        public void EditClass(ViewItemClass c)
        {
            MainScreenViewModel.EditClass(c);
        }

        public override bool GoBack()
        {
            switch (LayoutMode)
            {
                case LayoutModes.SplitEditing:
                case LayoutModes.FullEditing:
                    ExitEditMode();
                    return true;
            }

            return base.GoBack();
        }

        public void ViewClass(ViewItemClass c)
        {
            MainScreenViewModel.ViewClass(c);
        }

        public void DataChanged(AccountDataStore dataStore, DataChangedEvent e)
        {
            try
            {
                if (LayoutMode != LayoutModes.Normal && e.LocalAccountId == MainScreenViewModel.CurrentLocalAccountId)
                {
                    Dispatcher.Run(delegate
                    {
                        try
                        {
                            if (LayoutMode != LayoutModes.Normal)
                            {
                                UpdateEditMode();
                            }
                        }
                        catch (Exception ex)
                        {
                            TelemetryExtension.Current?.TrackException(ex);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void LogIn()
        {
            ShowPopup(new ExistingUserViewModel(this));
        }
    }
}
