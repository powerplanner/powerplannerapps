using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Holiday;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar
{
    public class CalendarViewModel : BaseMainScreenViewModelChild
    {
        private static DisplayStates _lastDisplayState;
        private static DateTime _initialSelectedDate;

        public static void SetInitialDisplayState(DisplayStates displayState, DateTime selectedDate)
        {
            _lastDisplayState = displayState;
            _initialSelectedDate = selectedDate;
        }

        public SemesterItemsViewGroup SemesterItemsViewGroup { get; private set; }

        public CalendarViewModel(BaseViewModel parent, Guid localAccountId, ViewItemSemester semester) : base(parent)
        {
            // iOS uses this to show the day when day before notification is clicked
            if (_initialSelectedDate != DateTime.MinValue)
            {
                NavigationManager.SetSelectedDate(_initialSelectedDate);
                NavigationManager.SetDisplayMonth(_initialSelectedDate);
                _selectedDate = NavigationManager.GetSelectedDate();
                _displayMonth = NavigationManager.GetDisplayMonth();
                _initialSelectedDate = DateTime.MinValue;
            }

            Initialize(localAccountId, semester);
        }

#if DEBUG
        ~CalendarViewModel()
        {
            System.Diagnostics.Debug.WriteLine("CalendarViewModel disposed");
        }
#endif

        private void Initialize(Guid localAccountId, ViewItemSemester semester)
        {
            var acct = MainScreenViewModel.CurrentAccount;
            if (acct == null)
            {
                throw new NullReferenceException("MainScreenViewModel.CurrentAccount was null");
            }
            _showPastCompleteItemsOnFullCalendar = acct.ShowPastCompleteItemsOnFullCalendar;
            FirstDayOfWeek = acct.WeekChangesOn;

            SemesterItemsViewGroup = SemesterItemsViewGroup.Load(localAccountId, semester);
        }

        public DateTime Today { get; private set; } = DateTime.Today;

        public DayOfWeek FirstDayOfWeek { get; private set; }

        private bool _showPastCompleteItemsOnFullCalendar = false;
        public bool ShowPastCompleteItemsOnFullCalendar
        {
            get => _showPastCompleteItemsOnFullCalendar;
            set
            {
                if (_showPastCompleteItemsOnFullCalendar == value)
                {
                    return;
                }

                _showPastCompleteItemsOnFullCalendar = value;
                MainScreenViewModel.CurrentAccount.ShowPastCompleteItemsOnFullCalendar = value;
                OnPropertyChanged(nameof(ShowPastCompleteItemsOnFullCalendar));
                TelemetryExtension.Current?.TrackEvent("ToggledShowPastCompleteItems", new Dictionary<string, string>()
                {
                    { "Value", value.ToString() }
                });

                try
                {
                    _ = AccountsManager.Save(MainScreenViewModel.CurrentAccount);
                }
                catch { }
            }
        }

        private DateTime _selectedDate = NavigationManager.GetSelectedDate();
        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                if (_selectedDate.Date == value.Date)
                    return;

                SetProperty(ref _selectedDate, value.Date, nameof(SelectedDate)); NavigationManager.SetSelectedDate(value);

                if (DisplayState == DisplayStates.CompactCalendar)
                {
                    DisplayState = DisplayStates.Day;
                }
            }
        }

        private DateTime _displayMonth = NavigationManager.GetDisplayMonth();
        public DateTime DisplayMonth
        {
            get { return _displayMonth; }
            set
            {
                if (DateTools.SameMonth(_displayMonth, value))
                {
                    return;
                }

                value = DateTools.GetMonth(value);

                SetProperty(ref _displayMonth, value, nameof(DisplayMonth));
                NavigationManager.SetDisplayMonth(value);
            }
        }

        public enum ViewSizeStates
        {
            /// <summary>
            /// Split calendar/day can be displayed
            /// </summary>
            Compact,

            /// <summary>
            /// Only calendar (or day) can be displayed
            /// </summary>
            FullyCompact
        }

        private ViewSizeStates _viewSizeState;
        /// <summary>
        /// Only used in iOS right now. The view should set this based on the view's size.
        /// </summary>
        public ViewSizeStates ViewSizeState
        {
            get => _viewSizeState;
            set
            {
                if (_viewSizeState != value)
                {
                    _viewSizeState = value;

                    switch (value)
                    {
                        case ViewSizeStates.Compact:
                            DisplayState = DisplayStates.Split;
                            break;

                        case ViewSizeStates.FullyCompact:
                            if (DisplayState == DisplayStates.Split)
                            {
                                DisplayState = DisplayStates.CompactCalendar;
                            }
                            break;
                    }
                }
            }
        }

        public enum DisplayStates
        {
            /// <summary>
            /// Display the split calendar/day.
            /// </summary>
            Split,

            /// <summary>
            /// Only display the day, along with a back button to return to calendar.
            /// </summary>
            Day,

            /// <summary>
            /// Only display the compact calendar.
            /// </summary>
            CompactCalendar
        }

        // Use previous selected display state from before
        private DisplayStates _displayState = _lastDisplayState;

        /// <summary>
        /// Only used in iOS and Android. The view should listen and display according to this property.
        /// </summary>
        public DisplayStates DisplayState
        {
            get => _displayState;
            private set
            {
                // Remember the selected display state
                _lastDisplayState = value;

                SetProperty(ref _displayState, value, nameof(DisplayState));
            }
        }

        public void ExpandDay()
        {
            if (ViewSizeState == ViewSizeStates.Compact)
            {
                DisplayState = DisplayStates.Day;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void BackToCalendar()
        {
            switch (ViewSizeState)
            {
                case ViewSizeStates.Compact:
                    DisplayState = DisplayStates.Split;
                    break;

                case ViewSizeStates.FullyCompact:
                    DisplayState = DisplayStates.CompactCalendar;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void AddTask(bool useSelectedDate = true)
        {
            AddItem(TaskOrEventType.Task, useSelectedDate: useSelectedDate);
        }

        public void AddTask(DateTime date)
        {
            AddItem(TaskOrEventType.Task, date);
        }

        public void AddEvent(bool useSelectedDate = true)
        {
            AddItem(TaskOrEventType.Event, useSelectedDate: useSelectedDate);
        }

        public void AddEvent(DateTime date)
        {
            AddItem(TaskOrEventType.Event, date);
        }

        private void AddItem(TaskOrEventType type, DateTime? dueDate = null, bool useSelectedDate = true)
        {
            dueDate = GetDateForAdd(dueDate, useSelectedDate);

            MainScreenViewModel.ShowPopup(AddTaskOrEventViewModel.CreateForAdd(MainScreenViewModel, new AddTaskOrEventViewModel.AddParameter()
            {
                SemesterIdentifier = MainScreenViewModel.CurrentSemesterId,
                Classes = MainScreenViewModel.Classes,
                SelectedClass = null,
                Type = type,
                DueDate = dueDate
            }));
        }

        public void AddHoliday(bool useSelectedDate = true)
        {
            AddHolidayHelper(useSelectedDate: useSelectedDate);
        }

        public void AddHoliday(DateTime date)
        {
            AddHolidayHelper(date: date);
        }

        private void AddHolidayHelper(DateTime? date = null, bool useSelectedDate = true)
        {
            date = GetDateForAdd(date, useSelectedDate);
            if (date == null)
            {
                date = DateTime.Today;
            }

            MainScreenViewModel.ShowPopup(AddHolidayViewModel.CreateForAdd(MainScreenViewModel, new AddHolidayViewModel.AddParameter()
            {
                SemesterIdentifier = MainScreenViewModel.CurrentSemesterId,
                StartDate = date.Value,
                EndDate = date.Value
            }));
        }

        private DateTime? GetDateForAdd(DateTime? dueDate, bool useSelectedDate)
        {
            if (dueDate == null)
            {
                if (useSelectedDate)
                {
                    dueDate = SelectedDate.Date;
                }
                else
                {
                    // If we're in the current month, we'll use the default adding behavior
                    if (DateTools.SameMonth(DisplayMonth, Today))
                    {
                        dueDate = null;
                    }

                    else
                    {
                        var prevAddDate = NavigationManager.GetPreviousAddItemDate();

                        // If in same month we've previously added to
                        if (prevAddDate != null && DateTools.SameMonth(DisplayMonth, prevAddDate.Value))
                        {
                            // Preserve that date
                            dueDate = prevAddDate;
                        }

                        // Otherwise
                        else
                        {
                            // Use first day of the month
                            dueDate = DateTools.GetMonth(DisplayMonth);
                        }
                    }
                }
            }

            return dueDate;
        }

        public void ShowItem(ViewItemTaskOrEvent item)
        {
            MainScreenViewModel.ShowItem(item);
        }

        public void OpenDay(DateTime date)
        {
            if (PowerPlannerApp.UseUnifiedCalendarDayTabItem)
            {
                throw new InvalidOperationException("If using unified calendar/day tab, you should set SelectedDate on the CalendarViewModel instead.");
            }

            NavigationManager.SetSelectedDate(date.Date);
            MainScreenViewModel.SetContent(new DayViewModel(MainScreenViewModel, MainScreenViewModel.CurrentLocalAccountId, MainScreenViewModel.CurrentSemester), preserveBack: true);
        }

        public void ViewClass(ViewItemClass c)
        {
            MainScreenViewModel.ViewClass(c);
        }

        public void ViewSchedule()
        {
            MainScreenViewModel.Navigate(new ScheduleViewModel(MainScreenViewModel));
        }

        public async Task MoveItem(ViewItemTaskOrEvent item, DateTime toDate)
        {
            try
            {
                if (item.EffectiveDateForDisplayInDateBasedGroups.Date == toDate.Date)
                {
                    return;
                }

                DataChanges changes = new DataChanges();

                var editedDataItem = item.CreateBlankDataItem();
                editedDataItem.Date = DateTime.SpecifyKind(toDate.Date.Add(item.DateInSchoolTime.TimeOfDay), DateTimeKind.Utc);
                changes.Add(editedDataItem);

                await PowerPlannerApp.Current.SaveChanges(changes);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void ViewHoliday(ViewItemHoliday h)
        {
            MainScreenViewModel.ShowPopup(AddHolidayViewModel.CreateForEdit(MainScreenViewModel, h));
        }
    }
}
