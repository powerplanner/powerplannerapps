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
    public class AddClassTimesViewModel : BaseMainScreenViewModelChild
    {
        public class ClassTimeGroup : BindableBase
        {
            private AddClassTimesViewModel _parent;

            public ClassTimeGroup(AddClassTimesViewModel parent)
            {
                _parent = parent;
            }

            public bool HasOnlyOneGroup => _parent.HasOnlyOneGroup;

            internal void NotifyHasOnlyOneGroupChanged()
            {
                OnPropertyChanged(nameof(HasOnlyOneGroup));
            }

            private bool _expanded;
            public bool Expanded
            {
                get => _expanded;
                set
                {
                    SetProperty(ref _expanded, value, nameof(Expanded));

                    if (value)
                    {
                        foreach (var other in _parent.Groups)
                        {
                            if (other != this && other.Expanded)
                            {
                                other.Expanded = false;
                            }
                        }
                    }
                }
            }

            private bool AutoAdjustEndTimes => _parent.AutoAdjustEndTimes;

            public void RemoveThisGroup()
            {
                // Note that we don't auto-expand another group since it's not obvious to the user what happened if we do that
                // (except if it's the last group)
                _parent.Groups.Remove(this);

                if (_parent.Groups.Count == 1)
                {
                    _parent.Groups.First().Expanded = true;
                }
            }

            private TimeSpan _startTime;

            public TimeSpan StartTime
            {
                get => _startTime;
                set
                {
                    if (AutoAdjustEndTimes)
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
                    else
                    {
                        SetProperty(ref _startTime, value, nameof(StartTime));
                    }
                }
            }

            private TimeSpan _endTime;

            public TimeSpan EndTime
            {
                get => _endTime;
                set
                {
                    if (AutoAdjustEndTimes)
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
                    else
                    {
                        SetProperty(ref _endTime, value, nameof(EndTime));
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
                internal set { _dayOfWeeks = value; value.CollectionChanged += DayOfWeeks_CollectionChanged; }
            }

            public bool Invalid
            {
                get => DayOfWeeks.Count == 0 || EndTime < StartTime;
            }

            /// <summary>
            /// For collapsed view
            /// </summary>
            public string DaysString
            {
                get
                {
                    if (DayOfWeeks.Count == 0)
                    {
                        return "No days selected";
                    }
                    else
                    {
                        return string.Join(", ", DayOfWeeks.Distinct().OrderBy(i => i).Select(i => DateTools.ToLocalizedString(i)));
                    }
                }
            }

            public string TimeString
            {
                get => PowerPlannerResources.GetStringTimeToTime(DateTimeFormatterExtension.Current.FormatAsShortTime(DateTime.Today.Add(StartTime)), DateTimeFormatterExtension.Current.FormatAsShortTime(DateTime.Today.Add(EndTime)));
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

                OnPropertyChanged(nameof(DaysString));
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
        }

        public ObservableCollection<ClassTimeGroup> Groups { get; private set; } = new ObservableCollection<ClassTimeGroup>();

        private bool _hasOnlyOneGroup;
        public bool HasOnlyOneGroup
        {
            get => _hasOnlyOneGroup;
            set
            {
                if (_hasOnlyOneGroup != value)
                {
                    SetProperty(ref _hasOnlyOneGroup, value, nameof(HasOnlyOneGroup));

                    foreach (var group in Groups)
                    {
                        group.NotifyHasOnlyOneGroupChanged();
                    }
                }
            }
        }

        protected override bool InitialAllowLightDismissValue => false;

        public enum OperationState { Adding, Editing }

        public OperationState State { get; private set; }

        public override string GetPageName() => State == OperationState.Adding ? "AddClassTimeView" : "EditClassTimeView";

        /// <summary>
        /// View should initialize this to false if end times will be managed by the view
        /// </summary>
        public bool AutoAdjustEndTimes { get; set; } = true;

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

        public bool IsInDifferentTimeZone { get; private set; }

        public bool ShowAddAnotherTime => AddParams != null;

        // Trigger the start and end time texts.
        private AddClassTimesViewModel(BaseViewModel parent) : base(parent)
        {
            Groups.CollectionChanged += Groups_CollectionChanged;
        }

        private void Groups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            HasOnlyOneGroup = Groups.Count == 1;
        }

        public static AddClassTimesViewModel CreateForAdd(BaseViewModel parent, AddParameter addParams)
        {
            var model = new AddClassTimesViewModel(parent)
            {
                State = OperationState.Adding,
                AddParams = addParams,
                ClassName = addParams.Class.Name,
                IsInDifferentTimeZone = parent.FindAncestorOrSelf<MainScreenViewModel>().CurrentAccount.IsInDifferentTimeZone
            };

            model.Groups.Add(new ClassTimeGroup(model)
            {
                StartTime = TimeSpan.FromHours(9),
                EndTime = new TimeSpan(9, 50, 0),
                Room = "",
                ScheduleWeek = PowerPlannerSending.Schedule.Week.BothWeeks,
                DayOfWeeks = new MyObservableList<DayOfWeek>(),
                Expanded = true
            });

            return model;
        }

        public static AddClassTimesViewModel CreateForEdit(BaseViewModel parent, EditParameter editParams)
        {
            var model = new AddClassTimesViewModel(parent)
            {
                State = OperationState.Editing,
                EditParams = editParams,
                ClassName = editParams.GroupedSchedules.First().Class.Name,
                IsInDifferentTimeZone = parent.FindAncestorOrSelf<MainScreenViewModel>().CurrentAccount.IsInDifferentTimeZone
            };

            model.Groups.Add(new ClassTimeGroup(model)
            {
                StartTime = editParams.GroupedSchedules.First().StartTimeInSchoolTime.TimeOfDay,
                EndTime = editParams.GroupedSchedules.First().EndTimeInSchoolTime.TimeOfDay,
                Room = editParams.GroupedSchedules.First().Room,
                DayOfWeeks = new MyObservableList<DayOfWeek>(editParams.GroupedSchedules.Select(i => i.DayOfWeek).Distinct()),
                ScheduleWeek = editParams.GroupedSchedules.First().ScheduleWeek,
                Expanded = true
            });

            return model;
        }

        public string ClassName { get; private set; }

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

        public void AddAnotherTime()
        {
            var currExpanded = Groups.FirstOrDefault(i => i.Expanded);
            if (currExpanded != null)
            {
                currExpanded.Expanded = false;
            }

            Groups.Add(new ClassTimeGroup(this)
            {
                StartTime = currExpanded?.StartTime ?? new TimeSpan(9, 0, 0),
                EndTime = currExpanded?.EndTime ?? new TimeSpan(9, 50, 0),
                Room = currExpanded?.Room ?? "",
                ScheduleWeek = PowerPlannerSending.Schedule.Week.BothWeeks,
                DayOfWeeks = new MyObservableList<DayOfWeek>(),
                Expanded = true
            });
        }

        public void Save()
        {
            //if (StartTime >= EndTime)
            //{
            //    new PortableMessageDialog(PowerPlannerResources.GetString("EditingClassScheduleItemView_LowEndTime.Content"), PowerPlannerResources.GetString("EditingClassScheduleItemView_InvalidEndTime.Title")).Show();
            //    return;
            //}

            //if (DayOfWeeks.Count == 0)
            //{
            //    new PortableMessageDialog(PowerPlannerResources.GetString("EditingClassScheduleItemView_InvalidDaysOfWeek.Content"), PowerPlannerResources.GetString("EditingClassScheduleItemView_InvalidDaysOfWeek.Title")).Show();
            //    return;
            //}

            TryStartDataOperationAndThenNavigate(async delegate
            {
                var updatedAndNewSchedules = new List<DataItemSchedule>();

                Guid classId = AddParams != null ? AddParams.Class.Identifier : EditParams.GroupedSchedules.First().Class.Identifier;

                // Get the existing schedules
                List<ViewItemSchedule> existingSchedules = EditParams != null ? EditParams.GroupedSchedules.ToList() : new List<ViewItemSchedule>();

                foreach (var g in Groups)
                {
                    foreach (var dayOfWeek in g.DayOfWeeks)
                    {
                        // First try to find an existing schedule that already matches this day of week
                        ViewItemSchedule existingSchedule = existingSchedules.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);

                        // If we couldn't find one, try picking a schedule that doesn't have any day of week matches
                        if (existingSchedule == null)
                            existingSchedule = existingSchedules.FirstOrDefault(s => !g.DayOfWeeks.Contains(s.DayOfWeek));

                        // Remove the schedule we added
                        if (existingSchedule != null)
                            existingSchedules.Remove(existingSchedule);

                        DataItemSchedule dataItem = new DataItemSchedule()
                        {
                            Identifier = existingSchedule != null ? existingSchedule.Identifier : Guid.NewGuid(),
                            UpperIdentifier = classId,
                            StartTime = AsUtc(g.StartTime),
                            EndTime = AsUtc(g.EndTime),
                            Room = g.Room,
                            DayOfWeek = dayOfWeek,
                            ScheduleWeek = g.ScheduleWeek,
                            ScheduleType = PowerPlannerSending.Schedule.Type.Normal
                        };

                        updatedAndNewSchedules.Add(dataItem);
                    }
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
