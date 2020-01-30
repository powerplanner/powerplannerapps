using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.App;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years
{
    public class YearsViewModel : BaseMainScreenViewModelChild
    {
        private YearsViewItemsGroup _yearsViewItemsGroup;
        public YearsViewItemsGroup YearsViewItemsGroup
        {
            get { return _yearsViewItemsGroup; }
            set { SetProperty(ref _yearsViewItemsGroup, value, "YearsViewItemsGroup"); }
        }

        public YearsViewModel(BaseViewModel parent) : base(parent)
        {
        }

        protected override async Task LoadAsyncOverride()
        {
            YearsViewItemsGroup = await YearsViewItemsGroup.LoadAsync(MainScreenViewModel.CurrentLocalAccountId);
        }

        public void AddYear()
        {
            ShowPopup(AddYearViewModel.CreateForAdd(MainScreenViewModel));
        }

        public async void AddSemester(Guid yearIdentifier)
        {
            await TryHandleUserInteractionAsync("AddSemester" + yearIdentifier, async (cancellationToken) =>
            {
                // If not full version and they already have a semester
                if (YearsViewItemsGroup.School.Years.Any(i => i.Semesters.Any()) && !await PowerPlannerApp.Current.IsFullVersionAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    PowerPlannerApp.Current.PromptPurchase(PowerPlannerResources.GetString("MessageFreeSemesterLimitReached"));
                    return;
                }

                ShowPopup(AddSemesterViewModel.CreateForAdd(MainScreenViewModel, new AddSemesterViewModel.AddParameter()
                {
                    YearIdentifier = yearIdentifier
                }));
            });
        }

        public void EditYear(ViewItemYear year)
        {
            ShowPopup(AddYearViewModel.CreateForEdit(MainScreenViewModel, year));
        }

        public void EditSemester(ViewItemSemester semester)
        {
            ShowPopup(AddSemesterViewModel.CreateForEdit(MainScreenViewModel, semester));
        }

        public async void OpenSemester(Guid semesterId)
        {
            await TryHandleUserInteractionAsync("OpenSemester" + semesterId, async delegate
            {
                await MainScreenViewModel.SetCurrentSemester(semesterId, alwaysNavigate: true);
            });
        }

        public override bool GoBack()
        {
            if (PowerPlannerApp.DoNotShowYearsInTabItems && !MainScreenViewModel.AvailableItems.Any())
            {
                return false;
            }

            return base.GoBack();
        }
    }
}
