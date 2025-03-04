﻿using PowerPlannerAppDataLibrary.ViewItems;
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
using Vx.Views;
using System.Drawing;
using Vx;
using PowerPlannerAppDataLibrary.Views;
using PowerPlannerAppDataLibrary.Helpers;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar
{
    public interface ICalendarOrDayViewModel
    {
        void AddTask(DateTime date);
        void AddEvent(DateTime date);
        void AddHoliday(DateTime date);
        MainScreenViewModel MainScreenViewModel { get; }
    }

    public class CalendarViewModel : ComponentViewModel, ICalendarOrDayViewModel
    {
        private static DisplayStates _lastDisplayState;
        private static DateTime _initialSelectedDate;

        public override bool DelayFirstRenderTillSizePresent => true;

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
            _showPastCompleteItemsOnCalendar = acct.ShowPastCompleteItemsOnCalendar;
            FirstDayOfWeek = acct.WeekChangesOn;

            SemesterItemsViewGroup = SemesterItemsViewGroup.Load(localAccountId, semester);
        }

        public DateTime Today { get; private set; } = DateTime.Today;

        public DayOfWeek FirstDayOfWeek { get; private set; }

        /// <summary>
        /// Used by Android to display the correct title
        /// </summary>
        public new string Title => CachedComputation<string>(delegate
        {
            if (DisplayState == DisplayStates.Day)
            {
                return GetDateHeaderText(SelectedDate);
            }

            else
            {
                return DisplayMonth.ToString("MMMM yyyy");
            }
        }, new string[] { nameof(DisplayState), nameof(SelectedDate), nameof(DisplayMonth) });

        public override bool CanGoBack => CachedComputation<bool>(delegate
        {
            return DisplayState == DisplayStates.Day || (ViewSizeState == ViewSizeStates.FullSize && DisplayState == DisplayStates.Split);
        }, new string[] { nameof(DisplayState), nameof(ViewSizeState) });

        public override bool GoBack()
        {
            if (DisplayState == DisplayStates.Day)
            {
                if (ViewSizeState == ViewSizeStates.Compact)
                {
                    DisplayState = DisplayStates.Split;
                }
                else if (ViewSizeState == ViewSizeStates.FullSize)
                {
                    DisplayState = DisplayStates.FullCalendar;
                }
                else
                {
                    DisplayState = DisplayStates.CompactCalendar;
                }

                return true;
            }

            if (DisplayState == DisplayStates.Split && ViewSizeState == ViewSizeStates.FullSize)
            {
                DisplayState = DisplayStates.FullCalendar;
                return true;
            }

            return base.GoBack();
        }

        public bool IncludeToolbarPrevNextButtons => CachedComputation<bool>(delegate
        {
            return ViewSizeState == ViewSizeStates.FullSize;
        }, new string[] { nameof(ViewSizeState) });

        public bool IncludeToolbarFilterButton => CachedComputation<bool>(delegate
        {
            return DisplayState == DisplayStates.FullCalendar;
        }, new string[] { nameof(DisplayState) });

        private string GetDateHeaderText(DateTime date)
        {
            if (date.Date == DateTime.Today)
                return PowerPlannerResources.GetRelativeDateToday();

            else if (date.Date == DateTime.Today.AddDays(1))
                return PowerPlannerResources.GetRelativeDateTomorrow();

            else if (date.Date == DateTime.Today.AddDays(-1))
                return PowerPlannerResources.GetRelativeDateYesterday();

            return PowerPlannerAppDataLibrary.Helpers.DateHelpers.ToMediumDateString(date);
        }

        private bool _showPastCompleteItemsOnCalendar = false;
        public bool ShowPastCompleteItemsOnCalendar
        {
            get => _showPastCompleteItemsOnCalendar;
            set
            {
                if (_showPastCompleteItemsOnCalendar == value)
                {
                    return;
                }

                _showPastCompleteItemsOnCalendar = value;
                MainScreenViewModel.CurrentAccount.ShowPastCompleteItemsOnCalendar = value;
                OnPropertyChanged(nameof(ShowPastCompleteItemsOnCalendar));
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

        public void Next()
        {
            if (DisplayState == DisplayStates.Day)
            {
                SelectedDate = SelectedDate.AddDays(1);
            }
            else
            {
                DisplayMonth = DisplayMonth.AddMonths(1);
            }
        }

        public void Previous()
        {
            if (DisplayState == DisplayStates.Day)
            {
                SelectedDate = SelectedDate.AddDays(-1);
            }
            else
            {
                DisplayMonth = DisplayMonth.AddMonths(-1);
            }
        }

        private bool _showGoToTodayInPrimaryCommands = true;
        public bool ShowGoToTodayInPrimaryCommands
        {
            get => _showGoToTodayInPrimaryCommands;
            set => SetProperty(ref _showGoToTodayInPrimaryCommands, value, nameof(ShowGoToTodayInPrimaryCommands));
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
            FullyCompact,

            /// <summary>
            /// Big screen (like iPad/desktop), full size calendar can be displayed
            /// </summary>
            FullSize
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
                    SetProperty(ref _viewSizeState, value, nameof(ViewSizeState));

                    switch (value)
                    {
                        case ViewSizeStates.Compact:
                            DisplayState = DisplayStates.Split;
                            break;

                        case ViewSizeStates.FullyCompact:
                            if (DisplayState != DisplayStates.Day)
                            {
                                DisplayState = DisplayStates.CompactCalendar;
                            }
                            break;

                        case ViewSizeStates.FullSize:
                            DisplayState = DisplayStates.FullCalendar;
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
            CompactCalendar,

            /// <summary>
            /// Full size calendar
            /// </summary>
            FullCalendar
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
            if (ViewSizeState == ViewSizeStates.Compact || ViewSizeState == ViewSizeStates.FullSize)
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

                case ViewSizeStates.FullSize:
                    if (DisplayState == DisplayStates.Split)
                    {
                        DisplayState = DisplayStates.FullCalendar;
                    }
                    else
                    {
                        DisplayState = DisplayStates.Split;
                    }
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
                if (DisplayState == DisplayStates.FullCalendar || DisplayState == DisplayStates.CompactCalendar)
                {
                    SelectedDate = date.Date;
                    DisplayState = DisplayStates.Day;
                }
                else
                {
                    throw new InvalidOperationException("If using unified calendar/day tab and not in the FullCalendar state, you should set SelectedDate on the CalendarViewModel instead. Current DisplayState is " + DisplayState);
                }
            }
            else
            {
                NavigationManager.SetSelectedDate(date.Date);
                MainScreenViewModel.SetContent(new DayViewModel(MainScreenViewModel, MainScreenViewModel.CurrentLocalAccountId, MainScreenViewModel.CurrentSemester), preserveBack: true);
            }
        }

        public void ViewClass(ViewItemClass c)
        {
            MainScreenViewModel.ViewClass(c);
        }

        public void ViewSchedule()
        {
            MainScreenViewModel.Navigate(new ScheduleViewModel(MainScreenViewModel));
        }

        public static async Task MoveItem(ViewItemTaskOrEvent item, DateTime toDate)
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

        public void GoToToday()
        {
            DisplayMonth = Today;
            SelectedDate = Today;
        }

        protected override void Initialize()
        {
            base.Initialize();

            if (Size.Height > 0)
            {
                OnSizeChanged(Size, new SizeF());
            }
        }

        protected override void OnSizeChanged(SizeF size, SizeF previousSize)
        {
            if (previousSize.Height == 0 && size.Height != 0)
            {
                // Make sure it renders after getting size
                MarkDirty();
            }

            if (size.Height < 500)
                ViewSizeState = ViewSizeStates.FullyCompact;

            else if (size.Width < 676)
                ViewSizeState = ViewSizeStates.Compact;

            else
                ViewSizeState = ViewSizeStates.FullSize;

            ShowGoToTodayInPrimaryCommands = Size.Width >= 435;
        }

        protected override View Render()
        {
            if (Size.Height == 0)
            {
                return null;
            }

            Toolbar toolbar = null;

            // Add toolbar to all but UWP
            if (true)
            {
                toolbar = new Toolbar
                {
                    Title = CanGoBack ? "" : Title,
                    OnBack = CanGoBack ? () => GoBack() : (Action)null,
                    BackText = CanGoBack ? Title : null,
                    PrimaryCommands =
                    {
                        // Add will get inserted here unless it's used as floating action

                        new MenuItem
                        {
                            Text = PowerPlannerResources.GetString("String_Previous"),
                            Glyph = MaterialDesign.MaterialDesignIcons.ChevronLeft,
                            Click = Previous
                        },

                        new MenuItem
                        {
                            Text = PowerPlannerResources.GetString("String_Next"),
                            Glyph = MaterialDesign.MaterialDesignIcons.ChevronRight,
                            Click = Next
                        },

                        // Go to today would be shown for Android (but not iOS since we don't have an icon for it yet)
                    }
                };

                toolbar = VxPlatform.Current == Platform.Uwp ? toolbar.InnerToolbarThemed() : toolbar.PowerPlannerThemed();

                // iOS reverses the order of toolbar items, so to ensure prev and next are int he right order, we reverse
                if (Toolbar.DisplaysPrimaryCommandsRightToLeft)
                {
                    toolbar.PrimaryCommands.Reverse();
                }

                // Go to today button
                var goToTodayMenuItem = new MenuItem
                {
                    Text = PowerPlannerResources.GetString("String_GoToToday"),
                    Glyph = MaterialDesign.MaterialDesignIcons.Today,
                    Click = GoToToday
                };
                if (ShowGoToTodayInPrimaryCommands)
                {
                    toolbar.PrimaryCommands.Add(goToTodayMenuItem);
                }
                else
                {
                    // On narrow views, show it in secondary commands (without a glyph, since the Show past complete doesn't have a glyph either)
                    goToTodayMenuItem.Glyph = null;
                    toolbar.SecondaryCommands.Add(goToTodayMenuItem);
                }

                // Only on full calendar, show the option for past complete
                toolbar.SecondaryCommands.Add(new MenuItem
                {
                    Text = PowerPlannerResources.GetString(ShowPastCompleteItemsOnCalendar ? "HidePastCompleteItems" : "ShowPastCompleteItems.Text"),
                    Click = () => ShowPastCompleteItemsOnCalendar = !ShowPastCompleteItemsOnCalendar
                });

                // Add button is displayed as floating action button sometimes on Android
                if (VxPlatform.Current != Platform.Android || DisplayState == DisplayStates.FullCalendar || DisplayState == DisplayStates.CompactCalendar)
                {
                    toolbar.PrimaryCommands.Insert(0, ToolbarHelper.AddCommand(() => AddTask(), () => AddEvent(), () => AddHoliday()));
                }
            }

            CalendarComponent calendar = null;
            if (DisplayState != DisplayStates.Day)
            {
                calendar = new CalendarComponent(this);

                if (DisplayState == DisplayStates.Split)
                {
                    calendar.Height = 300 + (CalendarComponent.IntegratedTopControls ? 50 : 0);
                }
                else
                {
                    calendar.LinearLayoutWeight(1);
                }
            }

            View day = null;
            if (DisplayState == DisplayStates.Day || DisplayState == DisplayStates.Split)
            {
                day = new DayComponent
                {
                    Today = Today,
                    SemesterItemsViewGroup = SemesterItemsViewGroup,
                    ViewModel = this,
                    DisplayDate = SelectedDate,
                    OnDisplayDateChanged = value => SelectedDate = value,
                    IncludeHeader = DisplayState == DisplayStates.Split,
                    OnExpand = DisplayState == DisplayStates.Split && VxPlatform.Current != Platform.Uwp ? ExpandDay : (Action)null,
                    IncludeAdd = VxPlatform.Current == Platform.Uwp
                    
                };
                day.LinearLayoutWeight(1);
            }

            var root = new LinearLayout
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    toolbar,
                    calendar,
                    day
                }
            };

            // Add floating action button for Android
            if (VxPlatform.Current == Platform.Android)
            {
                return new FrameLayout
                {
                    Children =
                    {
                        root,

                        day != null ? new FloatingAddItemButton
                        {
                            AddTask = () => AddTask(),
                            AddEvent = () => AddEvent(),
                            AddHoliday = () => AddHoliday()
                        } : null
                    }
                };
            }

            return root;
        }
    }
}
