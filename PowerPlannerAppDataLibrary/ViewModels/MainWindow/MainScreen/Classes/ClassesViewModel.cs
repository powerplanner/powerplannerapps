using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes
{
    public class ClassesViewModel : BaseMainScreenViewModelChild
    {
        public ClassesViewModel(BaseViewModel parent) : base(parent) { }

        public void AddClass()
        {
            MainScreenViewModel.AddClass();
        }
    }
}
