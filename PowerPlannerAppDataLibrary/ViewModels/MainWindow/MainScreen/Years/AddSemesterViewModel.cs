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

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years
{
    public class AddSemesterViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public enum OperationState { Adding, Editing }

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

                new CheckBox
                {
                    Text = PowerPlannerResources.GetString("AddYearPage_OverrideGpaCredits.Header"),
                    IsChecked = VxValue.Create(IsCustomizeCreditsGpaChecked, v => IsCustomizeCreditsGpaChecked = v),
                    Margin = new Thickness(0, 18, 0, 0)
                },

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
                } : null
            );
        }

        public ViewItemSemester SemesterToEdit { get; private set; }

        public class AddParameter
        {
            public Guid YearIdentifier;
        }

        public AddParameter AddParams { get; private set; }

        private AddSemesterViewModel(BaseViewModel parent, OperationState state) : base(parent)
        {
            State = state;

            Title = State == OperationState.Adding ? PowerPlannerResources.GetString("EditSemesterPage_Title_Adding") : PowerPlannerResources.GetString("EditSemesterPage_Title_Editing");

            if (State == OperationState.Adding)
            {
                PrimaryCommand = PopupCommand.Save(Save);
            }
            else
            {
                Commands = new PopupCommand[]
                {
                    PopupCommand.Save(Save),
                    PopupCommand.Delete(ConfirmDelete)
                };
            }
        }

        public static AddSemesterViewModel CreateForAdd(BaseViewModel parent, AddParameter addParams)
        {
            return new AddSemesterViewModel(parent, OperationState.Adding)
            {
                AddParams = addParams
            };
        }

        public static AddSemesterViewModel CreateForEdit(BaseViewModel parent, ViewItemSemester semesterToEdit)
        {
            var viewModel = new AddSemesterViewModel(parent, OperationState.Editing)
            {
                SemesterToEdit = semesterToEdit,
                Name = semesterToEdit.Name,
                OverriddenGpa = semesterToEdit.OverriddenGPA != PowerPlannerSending.Grade.UNGRADED ? semesterToEdit.OverriddenGPA : (double?)null,
                OverriddenCredits = semesterToEdit.OverriddenCredits != PowerPlannerSending.Grade.UNGRADED ? semesterToEdit.OverriddenCredits : (double?)null
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

        public void Save()
        {
            TryStartDataOperationAndThenNavigate(async delegate
            {
                string name = Name;

                if (string.IsNullOrWhiteSpace(name))
                {
                    new PortableMessageDialog(PowerPlannerResources.GetStringNoNameMessageBody(), PowerPlannerResources.GetStringNoNameMessageHeader()).Show();
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

                else
                    throw new NullReferenceException("Either editing semester or add semester param must be initialized.");

                semester.Name = name;
                semester.OverriddenGPA = IsCustomizeCreditsGpaChecked && OverriddenGpa != null ? OverriddenGpa.Value : PowerPlannerSending.Grade.UNGRADED;
                semester.OverriddenCredits = IsCustomizeCreditsGpaChecked && OverriddenCredits != null ? OverriddenCredits.Value : PowerPlannerSending.Grade.UNGRADED;

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

                DataChanges changes = new DataChanges();
                changes.Add(semester);

                await PowerPlannerApp.Current.SaveChanges(changes);
            }, delegate
            {
                this.RemoveViewModel();
            });
        }

        public async void ConfirmDelete()
        {
            if (await PowerPlannerApp.ConfirmDeleteAsync(PowerPlannerResources.GetString("MessageDeleteSemester_Body"), PowerPlannerResources.GetString("MessageDeleteSemester_Title")))
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