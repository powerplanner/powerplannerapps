using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule
{
    public class AddClassTimeViewModel : BaseMainScreenViewModelChild
    {
        protected override bool InitialAllowLightDismissValue => false;

        public enum OperationState { Adding, Editing }

        public OperationState State { get; private set; }

        public override string GetPageName() => State == OperationState.Adding ? "AddClassTimeView" : "EditClassTimeView";

        public class AddParameter
        {
            public ViewItemClass Class { get; set; }
        }

        public class EditParameter
        {
            public ViewItemSchedule[] GroupedSchedules { get; set; }
        }

        public AddParameter AddParams { get; private set; }

        public EditParameter EditParams { get; private set; }

        // Trigger the start and end time texts.
        private AddClassTimeViewModel(BaseViewModel parent) : base(parent) { }

        public static AddClassTimeViewModel CreateForAdd(BaseViewModel parent, AddParameter addParams)
        {
            return new AddClassTimeViewModel(parent)
            {
                State = OperationState.Adding,
                AddParams = addParams,
                StartTime = TimeSpan.FromHours(9),
                EndTime = new TimeSpan(9, 50, 0),
                Room = "",
                DayOfWeeks = new MyObservableList<DayOfWeek>() { DayOfWeek.Monday },
                ScheduleWeek = PowerPlannerSending.Schedule.Week.BothWeeks,
                ClassName = addParams.Class.Name
            };
        }

        public static AddClassTimeViewModel CreateForEdit(BaseViewModel parent, EditParameter editParams)
        {
            return new AddClassTimeViewModel(parent)
            {
                State = OperationState.Editing,
                EditParams = editParams,
                StartTime = editParams.GroupedSchedules.First().StartTime.TimeOfDay,
                EndTime = editParams.GroupedSchedules.First().EndTime.TimeOfDay,
                Room = editParams.GroupedSchedules.First().Room,
                DayOfWeeks = new MyObservableList<DayOfWeek>(editParams.GroupedSchedules.Select(i => i.DayOfWeek).Distinct()),
                ScheduleWeek = editParams.GroupedSchedules.First().ScheduleWeek,
                ClassName = editParams.GroupedSchedules.First().Class.Name
            };
        }

        public string ClassName { get; private set; }

        private TimeSpan _startTime;

        public TimeSpan StartTime
        {
            get => _startTime;
            set
            {

                // Automatically adjust the end time, maintaining the class as the same size it was before.
                var diff = EndTime - StartTime;
                SetProperty(ref _startTime, value, nameof(StartTime));
                
                var desiredEndTime = StartTime + diff;
                if (desiredEndTime.TotalHours >= 24)
                    desiredEndTime = new TimeSpan(23, 59, 0);

                _endTime = desiredEndTime;
                OnPropertyChanged(nameof(EndTime));
            }
        }

        private TimeSpan _endTime;

        public TimeSpan EndTime
        {
            get => _endTime;
            set
            {
                var diff = StartTime - EndTime;
                SetProperty(ref _endTime, value, nameof(EndTime));

                // If the EndTime is less than the StartTime, then push the StartTime back by the difference between the two.
                // So, if the EndTime is 10:30 and the StartTime is 10:40, the StartTime will become 10:20.
                if (EndTime < StartTime)
                {
                    _startTime = EndTime + diff;
                    if (_startTime.Ticks == 0)
                        _startTime = new TimeSpan(0);

                    OnPropertyChanged(nameof(StartTime));
                }
            }
        }

        private string _room = "";

        public string Room
        {
            get { return _room; }
            set { SetProperty(ref _room, value, nameof(Room)); }
        }

        private MyObservableList<DayOfWeek> _dayOfWeeks;
        public MyObservableList<DayOfWeek> DayOfWeeks
        {
            get { return _dayOfWeeks; }
            private set { _dayOfWeeks = value; value.CollectionChanged += DayOfWeeks_CollectionChanged; }
        }

        private void DayOfWeeks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var changes = new List<DayOfWeek>();

            if (e.OldItems != null)
            {
                changes.AddRange(e.OldItems.OfType<DayOfWeek>());
            }

            if (e.NewItems != null)
            {
                changes.AddRange(e.NewItems.OfType<DayOfWeek>());
            }

            changes = changes.Distinct().ToList();

            foreach (var change in changes)
            {
                OnPropertyChanged("Is" + change + "Checked");
            }
        }

        private void SetChecked(DayOfWeek dayOfWeek, bool isChecked)
        {
            if (isChecked)
            {
                if (!DayOfWeeks.Contains(dayOfWeek))
                    DayOfWeeks.InsertSorted(dayOfWeek);
            }

            else
            {
                DayOfWeeks.Remove(dayOfWeek);
            }
        }

        private bool GetIsChecked(DayOfWeek dayOfWeek)
        {
            return DayOfWeeks.Contains(dayOfWeek);
        }

        public bool IsMondayChecked
        {
            get { return GetIsChecked(DayOfWeek.Monday); }
            set { SetChecked(DayOfWeek.Monday, value); }
        }

        public bool IsTuesdayChecked
        {
            get { return GetIsChecked(DayOfWeek.Tuesday); }
            set { SetChecked(DayOfWeek.Tuesday, value); }
        }

        public bool IsWednesdayChecked
        {
            get { return GetIsChecked(DayOfWeek.Wednesday); }
            set { SetChecked(DayOfWeek.Wednesday, value); }
        }

        public bool IsThursdayChecked
        {
            get { return GetIsChecked(DayOfWeek.Thursday); }
            set { SetChecked(DayOfWeek.Thursday, value); }
        }

        public bool IsFridayChecked
        {
            get { return GetIsChecked(DayOfWeek.Friday); }
            set { SetChecked(DayOfWeek.Friday, value); }
        }

        public bool IsSaturdayChecked
        {
            get { return GetIsChecked(DayOfWeek.Saturday); }
            set { SetChecked(DayOfWeek.Saturday, value); }
        }

        public bool IsSundayChecked
        {
            get { return GetIsChecked(DayOfWeek.Sunday); }
            set { SetChecked(DayOfWeek.Sunday, value); }
        }

        private PowerPlannerSending.Schedule.Week _scheduleWeek;
        public PowerPlannerSending.Schedule.Week ScheduleWeek
        {
            get { return _scheduleWeek; }
            set { SetProperty(ref _scheduleWeek, value, nameof(ScheduleWeek)); }
        }

        private string[] _availableScheduleWeekStrings;
        /// <summary>
        /// The localized strings
        /// </summary>
        public string[] AvailableScheduleWeekStrings
        {
            get
            {
                if (_availableScheduleWeekStrings == null)
                {
                    _availableScheduleWeekStrings = new string[]
                    {
                        PowerPlannerResources.GetLocalizedWeek(PowerPlannerSending.Schedule.Week.BothWeeks),
                        PowerPlannerResources.GetLocalizedWeek(PowerPlannerSending.Schedule.Week.WeekOne),
                        PowerPlannerResources.GetLocalizedWeek(PowerPlannerSending.Schedule.Week.WeekTwo)
                    };
                }

                return _availableScheduleWeekStrings;
            }
        }

        public string ScheduleWeekString
        {
            get
            {
                switch (ScheduleWeek)
                {
                    case PowerPlannerSending.Schedule.Week.BothWeeks:
                        return AvailableScheduleWeekStrings[0];

                    case PowerPlannerSending.Schedule.Week.WeekOne:
                        return AvailableScheduleWeekStrings[1];

                    case PowerPlannerSending.Schedule.Week.WeekTwo:
                        return AvailableScheduleWeekStrings[2];

                    default:
                        throw new NotImplementedException();
                }
            }

            set
            {
                switch (AvailableScheduleWeekStrings.FindIndex(i => i == value))
                {
                    case 0:
                        ScheduleWeek = PowerPlannerSending.Schedule.Week.BothWeeks;
                        break;

                    case 1:
                        ScheduleWeek = PowerPlannerSending.Schedule.Week.WeekOne;
                        break;

                    case 2:
                        ScheduleWeek = PowerPlannerSending.Schedule.Week.WeekTwo;
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public void Delete()
        {
            TryStartDataOperationAndThenNavigate(async delegate
            {
                // Get the existing schedules
                List<ViewItemSchedule> existingSchedules = EditParams != null ? EditParams.GroupedSchedules.ToList() : new List<ViewItemSchedule>();

                if (existingSchedules.Count > 0)
                {
                    DataChanges changes = new DataChanges();

                    foreach (var s in existingSchedules)
                        changes.DeleteItem(s.Identifier);

                    await PowerPlannerApp.Current.SaveChanges(changes);
                }

            }, delegate
            {
                this.RemoveViewModel();
            });
        }

        public void Save(DateTime? timeStartedAdding = null)
        {
            TryStartDataOperationAndThenNavigate(async delegate
            {
                if (StartTime >= EndTime)
                {
                    new PortableMessageDialog(PowerPlannerResources.GetString("EditingClassScheduleItemView_LowEndTime.Content"), PowerPlannerResources.GetString("EditingClassScheduleItemView_InvalidEndTime.Title")).Show();
                    return;
                }

                if (DayOfWeeks.Count == 0)
                {
                    new PortableMessageDialog(PowerPlannerResources.GetString("EditingClassScheduleItemView_InvalidDaysOfWeek.Content"), PowerPlannerResources.GetString("EditingClassScheduleItemView_InvalidDaysOfWeek.Title")).Show();
                    return;
                }

                var updatedAndNewSchedules = new List<DataItemSchedule>();

                Guid classId = AddParams != null ? AddParams.Class.Identifier : EditParams.GroupedSchedules.First().Class.Identifier;

                // Get the existing schedules
                List<ViewItemSchedule> existingSchedules = EditParams != null ? EditParams.GroupedSchedules.ToList() : new List<ViewItemSchedule>();

                for (int i = 0; i < DayOfWeeks.Count; i++)
                {
                    DayOfWeek dayOfWeek = DayOfWeeks[i];

                    // First try to find an existing schedule that already matches this day of week
                    ViewItemSchedule existingSchedule = existingSchedules.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);

                    // If we couldn't find one, try picking a schedule that doesn't have any day of week matches
                    if (existingSchedule == null)
                        existingSchedule = existingSchedules.FirstOrDefault(s => !DayOfWeeks.Contains(s.DayOfWeek));

                    // Remove the schedule we added
                    if (existingSchedule != null)
                        existingSchedules.Remove(existingSchedule);

                    DataItemSchedule dataItem = new DataItemSchedule()
                    {
                        Identifier = existingSchedule != null ? existingSchedule.Identifier : Guid.NewGuid(),
                        UpperIdentifier = classId,
                        StartTime = AsUtc(StartTime),
                        EndTime = AsUtc(EndTime),
                        Room = Room,
                        DayOfWeek = dayOfWeek,
                        ScheduleWeek = ScheduleWeek,
                        ScheduleType = PowerPlannerSending.Schedule.Type.Normal
                    };

                    updatedAndNewSchedules.Add(dataItem);
                }

                // Deleted schedules are any remaining existing schedules
                var deletedScheduleIds = existingSchedules.Select(i => i.Identifier).ToArray();

                DataChanges changes = new DataChanges();

                foreach (var s in updatedAndNewSchedules)
                    changes.Add(s);

                foreach (var id in deletedScheduleIds)
                    changes.DeleteItem(id);

                if (!changes.IsEmpty())
                {
                    await PowerPlannerApp.Current.SaveChanges(changes);

                    if (timeStartedAdding != null)
                    {
                        TimeSpan duration = DateTime.UtcNow - timeStartedAdding.Value;

                        try
                        {
                            TelemetryExtension.Current?.TrackEvent("NewTimePicker_TestResult", new Dictionary<string, string>()
                            {
                                { "Duration", ((int)Math.Ceiling(duration.TotalSeconds)).ToString() },
                                { "IsEnabled", AbTestHelper.Tests.NewTimePicker.Value.ToString() }
                            });
                        }
                        catch { }
                    }
                }
            }, delegate
            {
                this.RemoveViewModel();
            });
        }

        private DateTime AsUtc(TimeSpan time)
        {
            return DateTime.SpecifyKind(DateTime.Today.Add(time), DateTimeKind.Utc);
        }
    }
}
