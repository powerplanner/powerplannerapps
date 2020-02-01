using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public class SyncErrorsViewModel : BaseMainScreenViewModelChild
    {
        public SyncErrorsViewModel(BaseViewModel parent, LoggedError[] syncErrors) : base(parent)
        {
            SyncErrors = syncErrors;
        }

        public LoggedError[] SyncErrors { get; private set; }
    }
}
