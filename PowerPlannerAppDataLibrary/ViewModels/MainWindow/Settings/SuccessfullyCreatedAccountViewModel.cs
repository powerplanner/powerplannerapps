using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class SuccessfullyCreatedAccountViewModel : BaseViewModel
    {
        public SuccessfullyCreatedAccountViewModel(BaseViewModel parent) : base(parent)
        {
        }

        public string Username { get; set; }

        public string Email { get; set; }

        public void Continue()
        {
            RemoveViewModel();
        }
    }
}
