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
using PowerPlannerAppDataLibrary;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class ClassGradesViewController : BareMvvmUIViewController<ClassGradesViewModel>
    {
        public ClassGradesViewController()
        {
            Title = PowerPlannerResources.GetString("ClassPage_PivotItemGrades.Header");
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();
            
            var rendered = ViewModel.Render();
            rendered.TranslatesAutoresizingMaskIntoConstraints = false;
            View.Add(rendered);
            rendered.StretchWidthAndHeight(View);
        }
    }
}