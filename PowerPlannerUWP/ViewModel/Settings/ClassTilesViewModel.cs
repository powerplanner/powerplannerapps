using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.Exceptions;
using PowerPlannerUWP.TileHelpers;

namespace PowerPlannerUWP.ViewModel.Settings
{
    public class ClassTilesViewModel : BaseSettingsViewModelWithAccount
    {
        public ClassTilesViewModel(BareMvvm.Core.ViewModels.BaseViewModel parent) : base(parent)
        {
        }

        protected override async Task LoadAsyncOverride()
        {
            await base.LoadAsyncOverride();

            if (Account.CurrentSemesterId == Guid.Empty)
            {
                return;
            }

            try
            {
                var viewItems = await ScheduleViewItemsGroup.LoadAsync(Account.LocalAccountId, Account.CurrentSemesterId, trackChanges: true, includeWeightCategories: false);

                HasSemester = true;

                // We lock since we're tracking changes, and can't let the list change while enumerating it initially
                using (await viewItems.DataChangeLock.LockForReadAsync())
                {
                    if (viewItems.Classes.Count == 0)
                    {
                        return;
                    }

                    HasClasses = true;

                    var classes = new List<ClassAndPinnedStatus>();

                    foreach (var c in viewItems.Classes)
                    {
                        classes.Add(new ClassAndPinnedStatus()
                        {
                            Class = c,
                            IsPinned = ClassTileHelper.IsPinned(Account.LocalAccountId, c.Identifier)
                        });
                    }

                    Classes = classes;
                }
            }
            catch (SemesterNotFoundException) { }
        }

        private bool _hasSemester;
        public bool HasSemester
        {
            get { return _hasSemester; }
            set { SetProperty(ref _hasSemester, value, nameof(HasSemester)); }
        }

        private bool _hasClasses;
        public bool HasClasses
        {
            get { return _hasClasses; }
            set { SetProperty(ref _hasClasses, value, nameof(HasClasses)); }
        }

        private List<ClassAndPinnedStatus> _classes;
        public List<ClassAndPinnedStatus> Classes
        {
            get { return _classes; }
            set { SetProperty(ref _classes, value, nameof(Classes)); }
        }

        public void SelectClass(ClassAndPinnedStatus c)
        {
            var pagedParent = FindAncestor<PagedViewModel>();
            pagedParent.Navigate(new ClassTileViewModel(pagedParent, c.Class));
        }
    }

    public class ClassAndPinnedStatus
    {
        public ViewItemClass Class { get; set; }

        public bool IsPinned { get; set; }
    }
}
