using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace PowerPlannerUWP.ViewModel.MainWindow.MainScreen.TaskOrEvents
{
    public class TaskOrEventListViewItemViewModel : BaseMainScreenViewModelChild
    {
        public TaskOrEventListViewItemViewModel(BaseViewModel parent) : base(parent)
        {
        }

        public List<MenuFlyoutItem> GetTestItems { get; private set; } = new List<MenuFlyoutItem>
        {
            new MenuFlyoutItem {Text="1"},
            new MenuFlyoutItem {Text="2"},
            new MenuFlyoutItem {Text="3"}
        };
    }
}
