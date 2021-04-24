using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.Converters;
using InterfacesiOS.Controllers;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class EditClassCreditsViewController : BareMvvmUIViewControllerWithScrolling<ConfigureClassCreditsViewModel>
    {
        public EditClassCreditsViewController()
        {
            Title = "Credits";

            PowerPlannerUIHelper.ConfigureForInputsStyle(this);

            var cancelButton = new UIBarButtonItem()
            {
                Title = "Cancel"
            };
            cancelButton.Clicked += new WeakEventHandler<EventArgs>(CancelButton_Clicked).Handler;
            NavigationItem.LeftBarButtonItem = cancelButton;

            var saveButton = new UIBarButtonItem()
            {
                Title = "Save"
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