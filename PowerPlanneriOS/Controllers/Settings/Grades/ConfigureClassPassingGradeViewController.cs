using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using InterfacesiOS.Controllers;
using InterfacesiOS.Converters;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Converters;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using UIKit;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers.Settings.Grades
{
    public class ConfigureClassPassingGradeViewController : BareMvvmUIViewControllerWithScrolling<ConfigureClassPassingGradeViewModel>
    {
        public ConfigureClassPassingGradeViewController()
        {
            Title = PowerPlannerResources.GetString("Settings_GradeOptions_ListItemPassingGrade.Title");

            PowerPlannerUIHelper.ConfigureForInputsStyle(this);

            var cancelButton = new UIBarButtonItem()
            {
                Title = PowerPlannerResources.GetStringCancel()
            };
            cancelButton.Clicked += new WeakEventHandler<EventArgs>(CancelButton_Clicked).Handler;
            NavigationItem.LeftBarButtonItem = cancelButton;

            var saveButton = new UIBarButtonItem()
            {
                Title = PowerPlannerResources.GetStringSave()
            };
            saveButton.Clicked += new WeakEventHandler<EventArgs>(SaveButton_Clicked).Handler;
            NavigationItem.RightBarButtonItem = saveButton;
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            StackView.AddSectionDivider();

            var renderedComponent = ViewModel.Render();
            renderedComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            StackView.AddArrangedSubview(renderedComponent);
            renderedComponent.StretchWidthAndHeight(StackView);

            StackView.AddSectionDivider();
        }

        private void SaveButton_Clicked(object sender, EventArgs e)
        {
            ViewModel.Save();
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            ViewModel.TryRemoveViewModelViaUserInteraction();
        }
    }
}