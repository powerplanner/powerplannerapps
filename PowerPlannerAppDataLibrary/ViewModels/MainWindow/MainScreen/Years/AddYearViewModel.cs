using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using ToolsPortable;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.App;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years
{
    public class AddYearViewModel : PopupComponentViewModel
    {
        protected override bool InitialAllowLightDismissValue => false;

        public enum OperationState { Adding, Editing }

        public OperationState State { get; private set; }

        public override string GetPageName()
        {
            if (State == OperationState.Adding)
            {
                return "AddYearView";
            }
            else
            {
                return "EditYearView";
            }
        }

        public ViewItemYear YearToEdit { get; private set; }
        public bool IsCustomizeCreditsGpaChecked { get => GetState<bool>(); set => SetState(value); }
        public bool IsOverrideCreditsEnabled { get => GetState<bool>(); set => SetState(value); }
        public bool IsOverrideGpaEnabled { get => GetState<bool>(); set => SetState(value); }
        public double? OverriddenCredits { get => GetState<double?>(); set => SetState(value); }
        public double? OverriddenGpa { get => GetState<double?>(); set => SetState(value); }
        public double? ActualGpa { get; private set; }
        public double? ActualCredits { get; private set; }

        private AddYearViewModel(BaseViewModel parent, OperationState state) : base(parent)
        {
            State = state;

            Title = State == OperationState.Adding ? PowerPlannerResources.GetString("AddYearPage_Title_Adding") : PowerPlannerResources.GetString("AddYearPage_Title_Editing");

            PrimaryCommand = PopupCommand.Save(Save);
        }

        protected override View Render()
        {
            return RenderGenericPopupContent(

                new TextBox
                {
                    Header = PowerPlannerResources.GetString("AddYearPage_TextBoxName.Header"),
                    PlaceholderText = PowerPlannerResources.GetString("AddYearPage_TextBoxName.PlaceholderText"),
                    Text = VxValue.Create(Name, v => Name = v),
                    AutoFocus = true,
                    OnSubmit = Save
                },

                new CheckBox
                {
                    Text = "Override GPA/credits",
                    IsChecked = VxValue.Create(IsCustomizeCreditsGpaChecked, v => IsCustomizeCreditsGpaChecked = v),
                    Margin = new Thickness(0, 18, 0, 0)
                },

                IsCustomizeCreditsGpaChecked ? new TextBlock
                {
                    Text = PowerPlannerResources.GetString("ConfigureClassFinalGradeGpa_OverrideGpa.Title"),
                    Margin = new Thickness(0, 18, 0, 0)
                } : null,

                IsCustomizeCreditsGpaChecked ? new NumberTextBox
                {
                    Number = VxValue.Create(IsOverrideGpaEnabled ? OverriddenGpa : ActualGpa, v => OverriddenGpa = v),
                    Margin = new Thickness(0, 3, 0, 0),
                    IsEnabled = IsOverrideGpaEnabled
                } : null,

                IsCustomizeCreditsGpaChecked ? new Switch
                {
                    Title = "TODO Override credits",
                    IsOn = VxValue.Create(IsOverrideCreditsEnabled, v =>
                    {
                        IsOverrideCreditsEnabled = v;
                    }),
                    Margin = new Thickness(0, 24, 0, 0)
                } : null,

                IsCustomizeCreditsGpaChecked ? new NumberTextBox
                {
                    Number = VxValue.Create(IsOverrideCreditsEnabled ? OverriddenCredits : ActualCredits, v => OverriddenCredits = v),
                    Margin = new Thickness(0, 12, 0, 0),
                    IsEnabled = IsOverrideCreditsEnabled
                } : null,

                State == OperationState.Editing ? new Button
                {
                    Text = "TODO Delete Year",
                    Margin = new Thickness(0, 24, 0, 0),
                    Click = Delete
                } : null

            );
        }

        public static AddYearViewModel CreateForAdd(BaseViewModel parent)
        {
            return new AddYearViewModel(parent, OperationState.Adding);
        }

        public static AddYearViewModel CreateForEdit(BaseViewModel parent, ViewItemYear yearToEdit)
        {
            var viewModel = new AddYearViewModel(parent, OperationState.Editing)
            {
                YearToEdit = yearToEdit,
                Name = yearToEdit.Name,
                ActualGpa = yearToEdit.GPA != -1 ? yearToEdit.GPA : (double?)null,
                ActualCredits = yearToEdit.CreditsEarned != PowerPlannerSending.Grade.NO_CREDITS ? yearToEdit.CreditsEarned : (double?)null,
                IsOverrideGpaEnabled = yearToEdit.OverriddenGPA != PowerPlannerSending.Grade.UNGRADED,
                IsOverrideCreditsEnabled = yearToEdit.OverriddenCredits != PowerPlannerSending.Grade.UNGRADED
            };

            viewModel.IsCustomizeCreditsGpaChecked = viewModel.IsOverrideCreditsEnabled || viewModel.IsOverrideGpaEnabled;

            if (viewModel.IsOverrideCreditsEnabled)
            {
                viewModel.OverriddenCredits = yearToEdit.OverriddenCredits;
            }

            if (viewModel.IsOverrideGpaEnabled)
            {
                viewModel.OverriddenGpa = yearToEdit.OverriddenGPA;
            }
            
            viewModel.ListenToItem(yearToEdit.Identifier).Deleted += viewModel.Year_Deleted;

            return viewModel;
        }

        private void Year_Deleted(object sender, EventArgs e)
        {
            RemoveViewModel(this);
        }

        private string _name = "";

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value, nameof(Name)); }
        }

        public void Save()
        {
            TryStartDataOperationAndThenNavigate(async delegate
            {
                string name = Name;

                if (string.IsNullOrWhiteSpace(name))
                {
                    new PortableMessageDialog(PowerPlannerResources.GetString("AddYearPage_MessageNoName_Body"), PowerPlannerResources.GetString("AddYearPage_MessageNoName_Title")).Show();
                    return;
                }

                DataItemYear year;

                if (YearToEdit != null)
                    year = new DataItemYear()
                    {
                        Identifier = YearToEdit.Identifier
                    };

                else
                    year = new DataItemYear() { Identifier = Guid.NewGuid() };

                year.Name = name;
                year.OverriddenGPA = IsCustomizeCreditsGpaChecked && IsOverrideGpaEnabled && OverriddenGpa != null ? OverriddenGpa.Value : PowerPlannerSending.Grade.UNGRADED;
                year.OverriddenCredits = IsCustomizeCreditsGpaChecked && IsOverrideCreditsEnabled && OverriddenCredits != null ? OverriddenCredits.Value : PowerPlannerSending.Grade.UNGRADED;

                DataChanges changes = new DataChanges();
                changes.Add(year);

                await PowerPlannerApp.Current.SaveChanges(changes);
            }, delegate
            {
                this.RemoveViewModel();
            });
        }

        public void Delete()
        {
            if (YearToEdit == null)
            {
                this.RemoveViewModel();
                return;
            }

            TryStartDataOperationAndThenNavigate(async delegate
            {
                await MainScreenViewModel.DeleteItem(YearToEdit.Identifier);
            }, delegate
            {
                this.RemoveViewModel();
            });
        }
    }
}
