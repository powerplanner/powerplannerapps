using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
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
        public SemesterItemsViewGroup SemesterItemsViewGroup { get; private set; }

        public CalendarViewModel(BaseViewModel parent, Guid localAccountId, ViewItemSemester semester) : base(parent)
        {
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
            FirstDayOfWeek = acct.WeekChangesOn;

            SemesterItemsViewGroup = SemesterItemsViewGroup.Load(localAccountId, semester);
        }

        public DateTime Today { get; private set; } = DateTime.Today;

        public DayOfWeek FirstDayOfWeek { get; private set; }

        private DateTime _selectedDate = NavigationManager.GetSelectedDate();
        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                if (_selectedDate.Date == value.Date)
                    return;

                SetProperty(ref _selectedDate, value.Date, nameof(SelectedDate)); NavigationManager.SetSelectedDate(value);
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

        public void AddHomework(bool useSelectedDate = true)
        {
            AddItem(AddHomeworkViewModel.ItemType.Homework, useSelectedDate: useSelectedDate);
        }

        public void AddHomework(DateTime date)
        {
            AddItem(AddHomeworkViewModel.ItemType.Homework, date);
        }

        public void AddExam(bool useSelectedDate = true)
        {
            AddItem(AddHomeworkViewModel.ItemType.Exam, useSelectedDate: useSelectedDate);
        }

        public void AddExam(DateTime date)
        {
            AddItem(AddHomeworkViewModel.ItemType.Exam, date);
        }

        private void AddItem(AddHomeworkViewModel.ItemType type, DateTime? dueDate = null, bool useSelectedDate = true)
        {
            dueDate = GetDateForAdd(dueDate, useSelectedDate);

            MainScreenViewModel.ShowPopup(AddHomeworkViewModel.CreateForAdd(MainScreenViewModel, new AddHomeworkViewModel.AddParameter()
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

        public void ShowItem(BaseViewItemHomeworkExam item)
        {
            MainScreenViewModel.ShowItem(item);
        }

        public void OpenDay(DateTime date)
        {
            NavigationManager.SetSelectedDate(date.Date);
            MainScreenViewModel.SetContent(new DayViewModel(MainScreenViewModel, MainScreenViewModel.CurrentLocalAccountId, MainScreenViewModel.CurrentSemester), preserveBack: true);
        }

        public void ViewClass(ViewItemClass c)
        {
            MainScreenViewModel.Navigate(new ClassViewModel(MainScreenViewModel, MainScreenViewModel.CurrentLocalAccountId, c.Identifier, DateTime.Today, SemesterItemsViewGroup.Semester));
        }

        public void ViewSchedule()
        {
            MainScreenViewModel.Navigate(new ScheduleViewModel(MainScreenViewModel));
        }

        public async Task MoveItem(BaseViewItemHomeworkExam item, DateTime toDate)
        {
            try
            {
                if (item.Date.Date == toDate.Date)
                {
                    return;
                }

                DataChanges changes = new DataChanges();

                var editedDataItem = item.CreateBlankDataItem();
                editedDataItem.Date = DateTime.SpecifyKind(toDate.Date.Add(item.Date.TimeOfDay), DateTimeKind.Utc);
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
