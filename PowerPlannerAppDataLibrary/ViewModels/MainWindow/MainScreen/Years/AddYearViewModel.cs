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
using PowerPlannerAppDataLibrary.Components;

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
        public double? OverriddenCredits { get => GetState<double?>(); set => SetState(value); }
        public double? OverriddenGpa { get => GetState<double?>(); set => SetState(value); }

        private AddYearViewModel(BaseViewModel parent, OperationState state) : base(parent)
        {
            State = state;

            Title = State == OperationState.Adding ? PowerPlannerResources.GetString("AddYearPage_Title_Adding") : PowerPlannerResources.GetString("AddYearPage_Title_Editing");

            PrimaryCommand = PopupCommand.Save(Save);

            if (state == OperationState.Editing)
            {
                SecondaryCommands = new PopupCommand[]
                {
                    PopupCommand.Delete(ConfirmDelete)
                };
            }
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
                OverriddenGpa = yearToEdit.OverriddenGPA != PowerPlannerSending.Grade.UNGRADED ? yearToEdit.OverriddenGPA : (double?)null,
                OverriddenCredits = yearToEdit.OverriddenCredits != PowerPlannerSending.Grade.UNGRADED ? yearToEdit.OverriddenCredits : (double?)null
            };

            viewModel.IsCustomizeCreditsGpaChecked = viewModel.OverriddenGpa != null || viewModel.OverriddenCredits != null;
            
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
                year.OverriddenGPA = IsCustomizeCreditsGpaChecked && OverriddenGpa != null ? OverriddenGpa.Value : PowerPlannerSending.Grade.UNGRADED;
                year.OverriddenCredits = IsCustomizeCreditsGpaChecked && OverriddenCredits != null ? OverriddenCredits.Value : PowerPlannerSending.Grade.UNGRADED;

                DataChanges changes = new DataChanges();
                changes.Add(year);

                await PowerPlannerApp.Current.SaveChanges(changes);
            }, delegate
            {
                this.RemoveViewModel();
            });
        }

        public async void ConfirmDelete()
        {
            if (await PowerPlannerApp.ConfirmDeleteAsync(PowerPlannerResources.GetString("MessageDeleteYear_Body"), PowerPlannerResources.GetString("MessageDeleteYear_Title")))
            {
                Delete();
            }
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
