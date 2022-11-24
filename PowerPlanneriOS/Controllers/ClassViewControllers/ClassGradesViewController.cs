using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using InterfacesiOS.Binding;
using PowerPlannerAppDataLibrary.Converters;
using PowerPlannerAppDataLibrary.ViewItems;
using System.Collections.Specialized;
using ToolsPortable;
using PowerPlanneriOS.Views;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using CoreGraphics;
using PowerPlannerAppDataLibrary.Extensions;
using InterfacesiOS.Helpers;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class ClassGradesViewController : BareMvvmUIViewController<ClassGradesViewModel>
    {
        private object _tabBarHeightListener;

        public ClassGradesViewController()
        {
            Title = "Grades";
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();
            
            var rendered = ViewModel.Render();
            rendered.TranslatesAutoresizingMaskIntoConstraints = false;
            View.Add(rendered);

            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                rendered.RemoveAllConstraints();
                rendered.StretchWidthAndHeight(View, 0, 0, 0, (float)MainScreenViewController.TAB_BAR_HEIGHT);
            });
        }
    }
}