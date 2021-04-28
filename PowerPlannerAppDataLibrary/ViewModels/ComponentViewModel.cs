using System;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;

namespace PowerPlannerAppDataLibrary.ViewModels
{
    public class ComponentViewModel : BaseMainScreenViewModelDescendant
    {
        public string Title { get; protected set; }

        public ComponentViewModel(BaseViewModel parent) : base(parent)
        {
        }
    }
}
