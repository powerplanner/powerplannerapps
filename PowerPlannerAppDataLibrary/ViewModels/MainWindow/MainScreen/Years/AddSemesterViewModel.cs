using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;
using static PowerPlannerSending.Schedule;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years
{
    public class AddSemesterViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public enum OperationState { Adding, Editing, Copying }

        public OperationState State { get; private set; }

        public bool IsCustomizeCreditsGpaChecked { get => GetState<bool>(); set => SetState(value); }
        public double? OverriddenCredits { get => GetState<double?>(); set => SetState(value); }
        public double? OverriddenGpa { get => GetState<double?>(); set => SetState(value); }

        public override string GetPageName()
        {
            if (State == OperationState.Adding)
            {
                return "AddSemesterView";
            }
            else if (State == OperationState.Copying)
            {
                return "CopySemesterView";
            }
            else
            {
                return "EditSemesterView";
            }
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(

                new TextBox
                {
                    Header = PowerPlannerResources.GetString("EditSemesterPage_TextBoxName.Header"),
                    PlaceholderText = PowerPlannerResources.GetString("EditSemesterPage_TextBoxName.PlaceholderText"),
                    Text = VxValue.Create(Name, v => Name = v),
                    AutoFocus = true,
                    OnSubmit = Save
                },

                // Year picker (for when copying semester)
                State == OperationState.Copying ? new ComboBox
                {
                    Margin = new Thickness(0, 18, 0, 0),
                    Header = PowerPlannerResources.GetString("EditSemesterPage_Year.Header"),
                    Items = _copyAvailableYears,
                    SelectedItem = VxValue.Create<object>(_copySelectedYear.Value, v => _copySelectedYear.Value = v as ViewItemYear),
                    ItemTemplate = y => new TextBlock
                    {
                        Text = (y as ViewItemYear).Name
                    }
                } : null,

                new LinearLayout
                {
                    Margin = new Thickness(0, 18, 0, 0),
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new DatePicker
                        {
                            Header = PowerPlannerResources.GetString("EditSemesterPage_DatePickerStart.Header"),
                            Value = VxValue.Create(StartDate, v => StartDate = v),
                            Margin = new Thickness(0, 0, 9, 0)
                        }.LinearLayoutWeight(1),

                        new DatePicker
                        {
                            Header = PowerPlannerResources.GetString("EditSemesterPage_DatePickerEnd.Header"),
                            Value = VxValue.Create(EndDate, v => EndDate = v),
                            Margin = new Thickness(9, 0, 0, 0)
                        }.LinearLayoutWeight(1)
                    }
                },

                State != OperationState.Copying ? new CheckBox
                {
                    Text = PowerPlannerResources.GetString("AddYearPage_OverrideGpaCredits.Header"),
                    IsChecked = VxValue.Create(IsCustomizeCreditsGpaChecked, v => IsCustomizeCreditsGpaChecked = v),
                    Margin = new Thickness(0, 18, 0, 0)
                } : null,

                IsCustomizeCreditsGpaChecked ? new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 12, 0, 0),
                    Children =
                    {
                        new NumberTextBox
                        {
                            Number = VxValue.Create(OverriddenGpa, v => OverriddenGpa = v),
                            Margin = new Thickness(0, 0, 9, 0),
                            Header = PowerPlannerResources.GetString("ConfigureClassFinalGradeGpa_OverrideGpa.Title")
                        }.LinearLayoutWeight(1),

                        new NumberTextBox
                        {
                            Number = VxValue.Create(OverriddenCredits, v => OverriddenCredits = v),
                            Margin = new Thickness(9, 0, 0, 0),
                            Header = PowerPlannerResources.GetString("AddYearPage_OverrideCredits.Header")
                        }.LinearLayoutWeight(1)
                    }
                } : null,

                // Classes section (for copying semester)
                State == OperationState.Copying ? new TextBlock
                {
                    Text = PowerPlannerResources.GetString("MainMenuItem_Classes"),
                    Margin = new Thickness(0, 18, 0, 4)
                } : null,

                State == OperationState.Copying ? RenderClassesForCopy() : null,

                new MultilineTextBox
                {
                    Header = PowerPlannerResources.GetString("EditTaskOrEventPage_TextBoxDetails.Header"),
                    Height = 140, // For now we're just going to leave height as fixed height, haven't implemented dynamic height in iOS
                    Text = VxValue.Create(Details, v => Details = v),
                    Margin = new Thickness(0, 18, 0, 0)
                }
            );
        }

        private View RenderClassesForCopy()
        {
            var layout = new LinearLayout
            {
                Margin = new Thickness(12,6,12,6)
            };

            foreach (var c in _copyAvailableClasses)
            {
                layout.Children.Add(new CheckBox
                {
                    Text = c.Name,
                    IsChecked = VxValue.Create(_copySelectedClasses.Contains(c), isChecked =>
                    {
                        if (isChecked)
                        {
                            _copySelectedClasses.Add(c);
                        }
                        else
                        {
                            _copySelectedClasses.Remove(c);
                        }

                        MarkDirty();
                    })
                });
            }

            layout.Children.Add(new TextBlock
            {
                // Schedules and grade settings from the selected classes will be copied. Tasks, events, and grades from within the classes will not be copied.
                Text = PowerPlannerResources.GetString("EditSemesterPage_String_CopiedItemsExplanation"),
                FontSize = Theme.Current.CaptionFontSize,
                Margin = new Thickness(0, 6, 0, 0)
            });

            return new Border
            {
                BackgroundColor = Theme.Current.BackgroundAlt1Color,
                Content = layout
            };
        }

        public ViewItemSemester SemesterToEdit { get; private set; }

        public class AddParameter
        {
            public Guid YearIdentifier;
        }

        public AddParameter AddParams { get; private set; }

        private ViewItemYear[] _copyAvailableYears;
        private VxState<ViewItemYear> _copySelectedYear = new VxState<ViewItemYear>();
        private ViewItemClass[] _copyAvailableClasses;
        private HashSet<ViewItemClass> _copySelectedClasses;

        private AddSemesterViewModel(BaseViewModel parent, OperationState state) : base(parent)
        {
            State = state;

            PrimaryCommand = PopupCommand.Save(Save);

            switch (state)
            {
                case OperationState.Adding:
                    Title = PowerPlannerResources.GetString("EditSemesterPage_Title_Adding");
                    break;

                case OperationState.Copying:
                    Title = PowerPlannerResources.GetString("EditSemesterPage_Title_Copying");
                    break;

                default:
                    Title = PowerPlannerResources.GetString("EditSemesterPage_Title_Editing");
                    SecondaryCommands = new PopupCommand[]
                    {
                    PopupCommand.Delete(ConfirmDelete)
                    };
                    break;
            }
        }

        public static AddSemesterViewModel CreateForAdd(BaseViewModel parent, AddParameter addParams)
        {
            return new AddSemesterViewModel(parent, OperationState.Adding)
            {
                AddParams = addParams
            };
        }

        public static AddSemesterViewModel CreateForCopy(BaseViewModel parent, ViewItemSemester semesterToCopy, ViewItemYear[] years)
        {
            var answer = new AddSemesterViewModel(parent, OperationState.Copying)
            {
                _copyAvailableYears = years,
                _copyAvailableClasses = semesterToCopy.Classes.ToArray(),
                Details = semesterToCopy.Details
            };
            answer._copySelectedClasses = new HashSet<ViewItemClass>(answer._copyAvailableClasses);
            answer._copySelectedYear.Value = semesterToCopy.Year;
            return answer;
        }

        public static AddSemesterViewModel CreateForEdit(BaseViewModel parent, ViewItemSemester semesterToEdit)
        {
            var viewModel = new AddSemesterViewModel(parent, OperationState.Editing)
            {
                SemesterToEdit = semesterToEdit,
                Name = semesterToEdit.Name,
                OverriddenGpa = semesterToEdit.OverriddenGPA != PowerPlannerSending.Grade.UNGRADED ? semesterToEdit.OverriddenGPA : (double?)null,
                OverriddenCredits = semesterToEdit.OverriddenCredits != PowerPlannerSending.Grade.UNGRADED ? semesterToEdit.OverriddenCredits : (double?)null,
                Details = semesterToEdit.Details
            };

            if (!PowerPlannerSending.DateValues.IsUnassigned(semesterToEdit.Start))
                viewModel.StartDate = semesterToEdit.Start.Date;

            if (!PowerPlannerSending.DateValues.IsUnassigned(semesterToEdit.End))
                viewModel.EndDate = semesterToEdit.End.Date;

            viewModel.ListenToItem(semesterToEdit.Identifier).Deleted += viewModel.Semester_Deleted;

            return viewModel;
        }

        private void Semester_Deleted(object sender, EventArgs e)
        {
            RemoveViewModel(this);
        }

        private string _name = "";

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value, nameof(Name)); }
        }

        private DateTime? _startDate;

        public DateTime? StartDate
        {
            get { return _startDate; }
            set { SetProperty(ref _startDate, value, nameof(StartDate)); }
        }

        private DateTime? _endDate;

        public DateTime? EndDate
        {
            get { return _endDate; }
            set { SetProperty(ref _endDate, value, nameof(EndDate)); }
        }

        private string _details = "";
        public string Details
        {
            get => _details;
            set => SetProperty(ref _details, value, nameof(Details));
        }

        public void Save()
        {
            string name = Name;

            if (string.IsNullOrWhiteSpace(name))
            {
                new PortableMessageDialog(PowerPlannerResources.GetStringNoNameMessageBody(), PowerPlannerResources.GetStringNoNameMessageHeader()).Show();
                return;
            }

            if (State == OperationState.Copying && _copySelectedYear.Value == null)
            {
                // Not localizing this since it should theoretically never get hit
                new PortableMessageDialog("You must select a year to copy the semester into.", "No year").Show();
                return;
            }

            DataItemSemester semester;

            if (SemesterToEdit != null)
                semester = new DataItemSemester()
                {
                    Identifier = SemesterToEdit.Identifier
                };

            else if (AddParams != null)
                semester = new DataItemSemester()
                {
                    Identifier = Guid.NewGuid(),
                    UpperIdentifier = AddParams.YearIdentifier
                };

            else if (State == OperationState.Copying)
            {
                semester = new DataItemSemester
                {
                    Identifier = Guid.NewGuid(),
                    UpperIdentifier = _copySelectedYear.Value.Identifier
                };
            }

            else
                throw new NullReferenceException("Either editing semester or add semester param must be initialized.");

            semester.Name = name;
            semester.OverriddenGPA = IsCustomizeCreditsGpaChecked && OverriddenGpa != null ? OverriddenGpa.Value : PowerPlannerSending.Grade.UNGRADED;
            semester.OverriddenCredits = IsCustomizeCreditsGpaChecked && OverriddenCredits != null ? OverriddenCredits.Value : PowerPlannerSending.Grade.UNGRADED;
            semester.Details = Details.Trim();

            if (StartDate != null)
            {
                semester.Start = DateTime.SpecifyKind(StartDate.Value.Date, DateTimeKind.Utc);

                if (!SqlDate.IsValid(semester.Start))
                    semester.Start = DateTime.Today;
            }

            else
            {
                semester.Start = PowerPlannerSending.DateValues.UNASSIGNED;
            }

            if (EndDate != null)
            {
                semester.End = DateTime.SpecifyKind(EndDate.Value.Date, DateTimeKind.Utc);

                if (!SqlDate.IsValid(semester.End))
                    semester.End = DateTime.Today;
            }

            else
            {
                semester.End = PowerPlannerSending.DateValues.UNASSIGNED;
            }


            if (!PowerPlannerSending.DateValues.IsUnassigned(semester.Start)
                && !PowerPlannerSending.DateValues.IsUnassigned(semester.End)
                && semester.Start > semester.End)
            {
                new PortableMessageDialog(PowerPlannerResources.GetString("EditSemesterPage_String_StartDateGreaterThanEndExplanation"), PowerPlannerResources.GetString("EditSemesterPage_String_InvalidStartDate")).Show();
                return;
            }

            TryStartDataOperationAndThenNavigate(async delegate
            {
                DataChanges changes = new DataChanges();
                changes.Add(semester);

                if (State == OperationState.Copying)
                {
                    foreach (var c in _copySelectedClasses)
                    {
                        var dataC = new DataItemClass
                        {
                            Identifier = Guid.NewGuid(),
                            UpperIdentifier = semester.Identifier,
                            Name = c.Name,
                            Credits = c.Credits,
                            DoesRoundGradesUp = c.DoesRoundGradesUp,
                            ShouldAverageGradeTotals = c.ShouldAverageGradeTotals,
                            PassingGrade = c.PassingGrade,
                            GpaType = c.GpaType,
                            RawColor = c.Color,
                            Details = c.Details
                        };
                        dataC.SetGradeScales(c.GradeScales);
                        changes.Add(dataC);

                        foreach (var w in c.WeightCategories)
                        {
                            changes.Add(new DataItemWeightCategory
                            {
                                Identifier = Guid.NewGuid(),
                                UpperIdentifier = dataC.Identifier,
                                Name = w.Name,
                                WeightValue = w.WeightValue
                            });
                        }

                        foreach (var s in c.Schedules)
                        {
                            changes.Add(new DataItemSchedule
                            {
                                Identifier = Guid.NewGuid(),
                                UpperIdentifier = dataC.Identifier,
                                DayOfWeek = s.DayOfWeek,
                                StartTime = s.DataItem.StartTime,
                                EndTime = s.DataItem.EndTime,
                                Room = s.Room,
                                ScheduleWeek = s.ScheduleWeek
                            });
                        }
                    }
                }

                await PowerPlannerApp.Current.SaveChanges(changes);

                if (State == OperationState.Copying)
                {
                    TelemetryExtension.Current?.TrackEvent("CopiedSemester", new Dictionary<string, string>()
                    {
                        { "AvailableClasses", _copyAvailableClasses.Length.ToString() },
                        { "SelectedClasses", _copySelectedClasses.Count.ToString() }
                    });
                }
            }, delegate
            {
                this.RemoveViewModel();
            });
        }

        public async void ConfirmDelete()
        {
            bool useConfirmationCheckbox = SemesterToEdit.Classes.Count > 0;

            if (await PowerPlannerApp.ConfirmDeleteAsync(PowerPlannerResources.GetString("MessageDeleteSemester_Body"), PowerPlannerResources.GetString("MessageDeleteSemester_Title"), useConfirmationCheckbox))
            {
                Delete();
            }
        }

        public void Delete()
        {
            if (SemesterToEdit == null)
            {
                this.RemoveViewModel();
                return;
            }

            TryStartDataOperationAndThenNavigate(async delegate
            {
                await MainScreenViewModel.DeleteItem(SemesterToEdit.Identifier);
            }, delegate
            {
                this.RemoveViewModel();
            });
        }
    }
}