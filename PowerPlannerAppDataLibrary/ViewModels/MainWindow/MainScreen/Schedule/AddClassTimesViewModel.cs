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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx;
using Vx.Extensions;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule
{
    public class AddClassTimesViewModel : PopupComponentViewModel
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
                                other.Validate();
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

            private bool _isInvalid;
            public bool IsInvalid
            {
                get => _isInvalid;
                set => SetProperty(ref _isInvalid, value, nameof(IsInvalid));
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

                if (IsDaysInvalid)
                {
                    if (DayOfWeeks.Count > 0)
                    {
                        IsDaysInvalid = false;
                        if (!IsTimesInvalid)
                        {
                            IsInvalid = false;
                        }
                    }
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

            internal void Validate()
            {
                if (DayOfWeeks.Count == 0)
                {
                    IsDaysInvalid = true;
                }
                else
                {
                    IsDaysInvalid = false;
                }

                if (EndTime <= StartTime)
                {
                    IsTimesInvalid = true;
                }
                else
                {
                    IsTimesInvalid = false;
                }

                IsInvalid = IsDaysInvalid | IsTimesInvalid;
            }

            private bool _isDaysInvalid;
            public bool IsDaysInvalid
            {
                get => _isDaysInvalid;
                set => SetProperty(ref _isDaysInvalid, value, nameof(IsDaysInvalid));
            }

            private bool _isTimesInvalid;
            public bool IsTimesInvalid
            {
                get => _isTimesInvalid;
                set => SetProperty(ref _isTimesInvalid, value, nameof(IsTimesInvalid));
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

            PrimaryCommand = PopupCommand.Save(Save);
            UseCancelForBack();
        }

        private void Groups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            HasOnlyOneGroup = Groups.Count == 1;
        }

        public static AddClassTimesViewModel CreateForAdd(BaseViewModel parent, AddParameter addParams)
        {
            var model = new AddClassTimesViewModel(parent)
            {
                Title = addParams.Class.Name,
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
                Title = editParams.GroupedSchedules.First().Class.Name,
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

        private const int SubgroupPadding = 12;

        private View _deleteButton;
        protected override View Render()
        {
            var layout = new LinearLayout()
            {
                Margin = new Thickness(Theme.Current.PageMargin)
            };

            foreach (var g in Groups)
            {
                View content;

                if (g.Expanded)
                {
                    var expanded = RenderExpanded(g);

                    if (Groups.Count > 1)
                    {
                        content = new Border
                        {
                            BackgroundColor = Theme.Current.PopupPageBackgroundAltColor,
                            BorderColor = Theme.Current.SubtleForegroundColor,
                            BorderThickness = new Thickness(1),
                            Padding = new Thickness(SubgroupPadding),
                            Content = expanded
                        };
                    }
                    else
                    {
                        content = expanded;
                    }
                }
                else
                {
                    content = RenderCollapsed(g);
                }

                content.Margin = new Thickness(0, 0, 0, 12);

                layout.Children.Add(content);
            }

            if (State == OperationState.Editing && Groups.Count == 1)
            {
                layout.Children.Add(new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 12, 0, 0),
                    Children =
                    {
                        new Button
                        {
                            Text = PowerPlannerResources.GetString("AddClassTime_AddAnotherTime.Content"),
                            Margin = new Thickness(0, 0, 6, 0),
                            Click = AddAnotherTime
                        }.LinearLayoutWeight(1),

                        new Button
                        {
                            Text = PowerPlannerResources.GetString("MenuItemDelete"),
                            Margin = new Thickness(6, 0, 0, 0),
                            ViewRef = view => _deleteButton = view,
                            Click = () =>
                            {
                                new ContextMenu
                                {
                                    Items =
                                    {
                                        new ContextMenuItem
                                        {
                                            Text = PowerPlannerResources.GetString("String_YesDelete"),
                                            Click = Delete
                                        }
                                    }
                                }.Show(_deleteButton);

                            }
                        }.LinearLayoutWeight(1)
                    }
                });
            }
            else
            {
                layout.Children.Add(new Button
                {
                    Text = PowerPlannerResources.GetString("AddClassTime_AddAnotherTime.Content"),
                    Margin = new Thickness(0, 12, 0, 0),
                    Click = AddAnotherTime
                });
            }

            return new ScrollView
            {
                Content = layout
            };
        }

        private View RenderExpanded(ClassTimeGroup group)
        {
            return new LinearLayout
            {
                Children =
                {
                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TimePicker
                            {
                                Header = PowerPlannerResources.GetString("EditingClassScheduleItemView_TimePickerStart.Header"),
                                Value = VxValue.Create(group.StartTime, v =>
                                {
                                    group.StartTime = v;
                                    MarkDirty();
                                }),
                                Margin = new Thickness(0, 0, 6, 0)
                            }.LinearLayoutWeight(VxPlatform.Current == Platform.Android ? 0 : 1),

                            // Only Android uses the "to"
                            VxPlatform.Current == Platform.Android ? new TextBlock
                            {
                                Text = PowerPlannerResources.GetString("TextBlock_To.Text"),
                                VerticalAlignment = VerticalAlignment.Center,
                                WrapText = false
                            } : null,

                            new EndTimePicker
                            {
                                Header = PowerPlannerResources.GetString("EditingClassScheduleItemView_TimePickerEnd.Header"),
                                StartTime = group.StartTime,
                                Value = VxValue.Create(group.EndTime, v =>
                                {
                                    group.EndTime = v;
                                    MarkDirty();
                                }),
                                Margin = new Thickness(6, 0, 0, 0)
                            }.LinearLayoutWeight(VxPlatform.Current == Platform.Android ? 0 : 1)
                        }
                    },

                    // Display time zone warning
                    IsInDifferentTimeZone ? new TextBlock
                    {
                        Text = PowerPlannerResources.GetString("DifferentTimeZoneWarning.Text"),
                        FontSize = Theme.Current.CaptionFontSize,
                        TextColor = Color.Red,
                        Margin = new Thickness(0, 6, 0, 0)
                    } : null,

                    new TextBox
                    {
                        Header = PowerPlannerResources.GetString("EditingClassScheduleItemView_TextBoxRoom.Header"),
                        Text = VxValue.Create(group.Room, v =>
                        {
                            group.Room = v;
                            MarkDirty();
                        }),
                        Margin = new Thickness(0, 12, 0, 0),
                        PlaceholderText = PowerPlannerResources.GetString("ex: Modern Languages 302")
                    },

                    new Border
                    {
                        BorderThickness = group.IsDaysInvalid ? new Thickness(2) : default(Thickness),
                        BorderColor = group.IsDaysInvalid ? Color.Red : default(Color),
                        Margin = new Thickness(-6, 6, -6, -6),
                        Padding = new Thickness(6),
                        Content = new LinearLayout
                        {
                            Margin = new Thickness(0, 0, 12, 0),
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new LinearLayout
                                {
                                    Margin = new Thickness(0, 0, 6, 0),
                                    Children =
                                    {
                                        new CheckBox
                                        {
                                            Text = DateTools.ToLocalizedString(DayOfWeek.Monday),
                                            IsChecked = VxValue.Create(group.IsMondayChecked, v =>
                                            {
                                                group.IsMondayChecked = v;
                                                MarkDirty();
                                            })
                                        },

                                        new CheckBox
                                        {
                                            Text = DateTools.ToLocalizedString(DayOfWeek.Tuesday),
                                            IsChecked = VxValue.Create(group.IsTuesdayChecked, v =>
                                            {
                                                group.IsTuesdayChecked = v;
                                                MarkDirty();
                                            })
                                        },

                                        new CheckBox
                                        {
                                            Text = DateTools.ToLocalizedString(DayOfWeek.Wednesday),
                                            IsChecked = VxValue.Create(group.IsWednesdayChecked, v =>
                                            {
                                                group.IsWednesdayChecked = v;
                                                MarkDirty();
                                            })
                                        },

                                        new CheckBox
                                        {
                                            Text = DateTools.ToLocalizedString(DayOfWeek.Thursday),
                                            IsChecked = VxValue.Create(group.IsThursdayChecked, v =>
                                            {
                                                group.IsThursdayChecked = v;
                                                MarkDirty();
                                            })
                                        }
                                    }
                                }.LinearLayoutWeight(1),

                                new LinearLayout
                                {
                                    Margin = new Thickness(6, 0, 0, 0),
                                    Children =
                                    {
                                        new CheckBox
                                        {
                                            Text = DateTools.ToLocalizedString(DayOfWeek.Friday),
                                            IsChecked = VxValue.Create(group.IsFridayChecked, v =>
                                            {
                                                group.IsFridayChecked = v;
                                                MarkDirty();
                                            })
                                        },

                                        new CheckBox
                                        {
                                            Text = DateTools.ToLocalizedString(DayOfWeek.Saturday),
                                            IsChecked = VxValue.Create(group.IsSaturdayChecked, v =>
                                            {
                                                group.IsSaturdayChecked = v;
                                                MarkDirty();
                                            })
                                        },

                                        new CheckBox
                                        {
                                            Text = DateTools.ToLocalizedString(DayOfWeek.Sunday),
                                            IsChecked = VxValue.Create(group.IsSundayChecked, v =>
                                            {
                                                group.IsSundayChecked = v;
                                                MarkDirty();
                                            })
                                        }
                                    }
                                }.LinearLayoutWeight(1)
                            }
                        }
                    },

                    group.IsDaysInvalid ? new TextBlock
                    {
                        Text = PowerPlannerResources.GetString("EditingClassScheduleItemView_InvalidDaysOfWeek.Content"),
                        TextColor = Color.Red,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 6, 0, 0)
                    } : null,

                    new ComboBox
                    {
                        Margin = new Thickness(0, 12, 0, 0),
                        Header = PowerPlannerResources.GetString("EditingClassScheduleItemView_WeekComboBox.Header"),
                        Items = group.AvailableScheduleWeekStrings,
                        SelectedItem = VxValue.Create<object>(group.ScheduleWeekString, val =>
                        {
                            group.ScheduleWeekString = val as string;
                            MarkDirty();
                        })
                    },

                    new TextBlock
                    {
                        Text = PowerPlannerResources.GetString("EditingClassScheduleItemView_TextBlockWeekDescription.Text"),
                        FontSize = Theme.Current.CaptionFontSize,
                        TextColor = Theme.Current.SubtleForegroundColor,
                        Margin = new Thickness(0, 6, 0, 0)
                    },

                    Groups.Count > 1 ? new Button
                    {
                        Text = PowerPlannerResources.GetString("AddClassTime_RemoveThisSchedule.Content"),
                        Margin = new Thickness(0, 12, 0, 0),
                        Click = () =>
                        {
                            Groups.Remove(group);
                            MarkDirty();
                        }
                    } : null
                }
            };
        }

        private View RenderCollapsed(ClassTimeGroup group)
        {
            return new Border
            {
                BackgroundColor = group.IsInvalid ? Color.FromArgb(50, 255, 0, 0) : Theme.Current.PopupPageBackgroundAltColor,
                BorderThickness = new Thickness(1),
                BorderColor = Theme.Current.SubtleForegroundColor,
                Content = new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new TransparentContentButton
                        {
                            Click = () =>
                            {
                                group.Expanded = true;
                                MarkDirty();
                            },
                            Content = new LinearLayout
                            {
                                Margin = new Thickness(SubgroupPadding),
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = group.TimeString,
                                        WrapText = false
                                    },

                                    new TextBlock
                                    {
                                        Text = group.DaysString,
                                        WrapText = false
                                    },

                                    string.IsNullOrWhiteSpace(group.Room) ? null : new TextBlock
                                    {
                                        Text = group.Room,
                                        WrapText = false
                                    },

                                    group.ScheduleWeek == PowerPlannerSending.Schedule.Week.BothWeeks ? null : new TextBlock
                                    {
                                        Text = group.ScheduleWeekString,
                                        WrapText = false
                                    }
                                }
                            }
                        }.LinearLayoutWeight(1),

                        new TransparentContentButton
                        {
                            Content = new FontIcon
                            {
                                Glyph = MaterialDesign.MaterialDesignIcons.Delete,
                                FontSize = 24,
                                Margin = new Thickness(SubgroupPadding)
                            },
                            Click = () =>
                            {
                                Groups.Remove(group);
                                MarkDirty();
                            }
                        }
                    }
                }
            };
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
                currExpanded.Validate();
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

            MarkDirty();
        }

        public void Save()
        {
            bool hasInvalid = false;
            ClassTimeGroup groupToExpand = null;
            bool alreadyHasInvalidExpandedGroup = false;

            foreach (var group in Groups)
            {
                group.Validate();

                if (group.IsInvalid)
                {
                    hasInvalid = true;

                    if (groupToExpand == null && !group.Expanded)
                    {
                        groupToExpand = group;
                    }

                    if (group.Expanded)
                    {
                        alreadyHasInvalidExpandedGroup = true;
                    }
                }
            }

            if (hasInvalid)
            {
                if (!alreadyHasInvalidExpandedGroup && groupToExpand != null)
                {
                    groupToExpand.Expanded = true;
                }

                MarkDirty();
                return;
            }

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
