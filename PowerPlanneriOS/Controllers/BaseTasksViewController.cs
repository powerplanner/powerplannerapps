using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using InterfacesiOS.Controllers;

namespace PowerPlanneriOS.Controllers
{
    public abstract class BaseTasksViewController<T> : BareMvvmUIViewController<T> where T : BaseMainScreenViewModelChild
    {
        public BaseTasksViewController()
        {
            Title = GetTitle();
        }

        protected abstract string GetTitle();
    }
}