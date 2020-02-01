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

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years
{
    public class AddYearViewModel : BaseMainScreenViewModelChild
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

        private AddYearViewModel(BaseViewModel parent) : base(parent)
        {
        }

        public static AddYearViewModel CreateForAdd(BaseViewModel parent)
        {
            return new AddYearViewModel(parent)
            {
                State = OperationState.Adding
            };
        }

        public static AddYearViewModel CreateForEdit(BaseViewModel parent, ViewItemYear yearToEdit)
        {
            var viewModel = new AddYearViewModel(parent)
            {
                State = OperationState.Editing,
                YearToEdit = yearToEdit,
                Name = yearToEdit.Name
            };
            
            viewModel.ListenToItem(yearToEdit.Identifier).Deleted += viewModel.Year_Deleted;

            return viewModel;
        }

        private void Year_Deleted(object sender, EventArgs e)
        {
            RemoveViewModel(this);
        }


        public string Name { get; set; } = "";

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
