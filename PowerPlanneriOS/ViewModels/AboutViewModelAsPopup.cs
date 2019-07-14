using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using BareMvvm.Core.ViewModels;

namespace PowerPlanneriOS.ViewModels
{
    public class AboutViewModelAsPopup : AboutViewModel
    {
        public AboutViewModelAsPopup(BaseViewModel parent) : base(parent) { }
    }
}