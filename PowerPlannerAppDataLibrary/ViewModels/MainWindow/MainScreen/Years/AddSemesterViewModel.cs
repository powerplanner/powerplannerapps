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

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years
{
    public class AddSemesterViewModel : BaseMainScreenViewModelChild
    {
        protected override bool InitialAllowLightDismissValue => false;

        public enum OperationState { Adding, Editing }

        public OperationState State { get; private set; }

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

        public ViewItemSemester SemesterToEdit { get; private set; }

        /// <summary>
        /// View should set this if start/end is supported (otherwise we won't save start/end)
        /// </summary>
        public bool SupportsStartEnd { get; set; }

        public class AddParameter
        {
            public Guid YearIdentifier;
        }

        public AddParameter AddParams { get; private set; }

        private AddSemesterViewModel(BaseViewModel parent) : base(parent)
        {
        }

        public static AddSemesterViewModel CreateForAdd(BaseViewModel parent, AddParameter addParams)
        {
            return new AddSemesterViewModel(parent)
            {
                State = OperationState.Adding,
                AddParams = addParams
            };
        }

        public static AddSemesterViewModel CreateForEdit(BaseViewModel parent, ViewItemSemester semesterToEdit)
        {
            var viewModel = new AddSemesterViewModel(parent)
            {
                State = OperationState.Editing,
                SemesterToEdit = semesterToEdit,
                Name = semesterToEdit.Name
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


        public string Name { get; set; } = "";


        public DateTime? StartDate { get; set; }


        public DateTime? EndDate { get; set; }

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

                if (SupportsStartEnd)
                {
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
                }

                DataChanges changes = new DataChanges();
                changes.Add(semester);

                await PowerPlannerApp.Current.SaveChanges(changes);
            }, delegate
            {
                this.RemoveViewModel();
            });
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