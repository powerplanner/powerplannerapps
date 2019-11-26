using BareMvvm.Core.ViewModels;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public abstract class BaseMainScreenViewModelChild : BaseMainScreenViewModelDescendant
    {
        public BaseMainScreenViewModelChild(BaseViewModel parent) : base(parent) { }

        public override MainScreenViewModel MainScreenViewModel
        {
            get { return (MainScreenViewModel)Parent; }
        }
    }
}
