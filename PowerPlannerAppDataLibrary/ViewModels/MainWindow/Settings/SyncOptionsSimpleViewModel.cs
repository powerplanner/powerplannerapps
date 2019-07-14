using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SyncOptionsSimpleViewModel : BaseViewModel
    {
        private PagedViewModel _pagedViewModel;

        public SyncOptionsSimpleViewModel(BaseViewModel parent) : base(parent)
        {
            _pagedViewModel = FindAncestor<PagedViewModel>();
        }

        public void OpenImageUploadOptions()
        {
            _pagedViewModel.Navigate(new ImageUploadOptionsViewModel(_pagedViewModel));
        }
    }
}
