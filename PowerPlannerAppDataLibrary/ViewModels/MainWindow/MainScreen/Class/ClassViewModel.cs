using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class
{
    public class ClassViewModel : BaseMainScreenViewModelChild
    {
        private ClassViewItemsGroup _classViewItemsGroup;
        public ClassViewItemsGroup ViewItemsGroupClass
        {
            get { return _classViewItemsGroup; }
            private set { SetProperty(ref _classViewItemsGroup, value, nameof(ViewItemsGroupClass)); }
        }

        private Guid _localAccountId;
        public Guid ClassId { get; private set; }
        private DateTime _today;
        private ViewItemSemester _semester;
        public ClassViewModel(BaseViewModel parent, Guid localAccountId, Guid classId, DateTime today, ViewItemSemester semester) : base(parent)
        {
            _localAccountId = localAccountId;
            ClassId = classId;
            _today = today;
            _semester = semester;

            ListenToItem(classId).Deleted += Class_Deleted;

            ListenToLocalEditsFor<DataLayer.DataItems.DataItemMegaItem>().ChangedItems += ItemsLocallyEditedListener_ChangedItems;
        }

        private Guid[] _lastChangedItemsIdentifiers;
        /// <summary>
        /// Gets the last changed item and then resets it to null
        /// </summary>
        /// <returns></returns>
        public ViewItemTaskOrEvent GetLastChangedTaskOrEvent()
        {
            // Grab reference of it since this could change in a background thread
            var lastIdentifiers = _lastChangedItemsIdentifiers;
            _lastChangedItemsIdentifiers = null;

            if (lastIdentifiers != null)
            {
                if (ViewItemsGroupClass.Homework != null)
                {
                    var item = ViewItemsGroupClass.Homework.FirstOrDefault(i => lastIdentifiers.Contains(i.Identifier));
                    if (item != null)
                    {
                        return item;
                    }
                    if (ViewItemsGroupClass.PastCompletedHomework != null)
                    {
                        item = ViewItemsGroupClass.PastCompletedHomework.FirstOrDefault(i => lastIdentifiers.Contains(i.Identifier));
                        if (item != null)
                        {
                            return item;
                        }
                    }
                }
                if (ViewItemsGroupClass.Exams != null)
                {
                    var item = ViewItemsGroupClass.Exams.FirstOrDefault(i => lastIdentifiers.Contains(i.Identifier));
                    if (item != null)
                    {
                        return item;
                    }
                    if (ViewItemsGroupClass.PastCompletedExams != null)
                    {
                        item = ViewItemsGroupClass.PastCompletedExams.FirstOrDefault(i => lastIdentifiers.Contains(i.Identifier));
                        if (item != null)
                        {
                            return item;
                        }
                    }
                }
                return null;
            }

            return null;
        }

        private void ItemsLocallyEditedListener_ChangedItems(object sender, Guid[] identifiers)
        {
            // Note that this executes on a background thread
            _lastChangedItemsIdentifiers = identifiers;
        }

        private void Class_Deleted(object sender, EventArgs e)
        {
            try
            {
                // Replace this one with the generic classes view model
                MainScreenViewModel.Replace(this, new ClassesViewModel(MainScreenViewModel));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        protected override async Task LoadAsyncOverride()
        {
            try
            {
                ViewItemsGroupClass = await ClassViewItemsGroup.LoadAsync(_localAccountId, ClassId, _today, _semester);

                DetailsViewModel = new ClassDetailsViewModel(this);
                HomeworkViewModel = new ClassHomeworkOrExamsViewModel(this, TaskOrEventType.Task);
                ExamsViewModel = new ClassHomeworkOrExamsViewModel(this, TaskOrEventType.Event);
                TimesViewModel = new ClassTimesViewModel(this);
                GradesViewModel = new Class.ClassGradesViewModel(this);
            }
            catch (ClassViewItemsGroup.ClassNotFoundExcetion)
            {
                var parent = FindAncestor<PagedViewModel>();
                parent.Replace(this, new ClassesViewModel(parent));
                throw;
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                RemoveViewModel();
            }
        }

        public ClassDetailsViewModel DetailsViewModel { get; private set; }

        public ClassTimesViewModel TimesViewModel { get; private set; }

        public ClassHomeworkOrExamsViewModel HomeworkViewModel { get; private set; }

        public ClassHomeworkOrExamsViewModel ExamsViewModel { get; private set; }

        public ClassGradesViewModel GradesViewModel { get; private set; }

        /// <summary>
        /// View should set this
        /// </summary>
        public BaseViewModel CurrentViewModel { get; set; }

        public void EditClass()
        {
            MainScreenViewModel.ShowPopup(AddClassViewModel.CreateForEdit(MainScreenViewModel, ViewItemsGroupClass.Class));
        }

        public void EditClassWithDetails()
        {
            var viewModel = AddClassViewModel.CreateForEdit(MainScreenViewModel, ViewItemsGroupClass.Class);
            viewModel.IncludesEditingDetails = true;
            MainScreenViewModel.ShowPopup(viewModel);
        }

        public void EditDetails()
        {
            MainScreenViewModel.ShowPopup(new EditClassDetailsViewModel(MainScreenViewModel, ViewItemsGroupClass.Class));
        }

        public void EditTimes()
        {
            MainScreenViewModel.Navigate(new ScheduleViewModel(MainScreenViewModel, new ScheduleViewModel.Params()
            {
                IsEditing = true
            }));
        }

        public void ConfigureGrades()
        {
            if (ConfigureClassGradesViewModel.UsePopups)
            {
                MainScreenViewModel.ShowPopup(new ConfigureClassGradesListViewModel(MainScreenViewModel, ViewItemsGroupClass.Class));
            }
            else
            {
                MainScreenViewModel.ShowPopup(new ConfigureClassGradesViewModel(MainScreenViewModel, ViewItemsGroupClass.Class));
            }
        }

        /// <summary>
        /// Assumes you've already confirmed that user wants to delete the class.
        /// </summary>
        public async void DeleteClass()
        {
            try
            {
                await MainScreenViewModel.DeleteItem(ClassId);
            }

            catch { }
        }

        protected override BaseViewModel GetChildContent()
        {
            return CurrentViewModel;
        }
    }
}
