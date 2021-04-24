using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using PowerPlannerAppDataLibrary.Converters;
using InterfacesiOS.Controllers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers.ClassViewControllers
{
    public class EditClassWeightCategoriesViewController : BareMvvmUIViewControllerWithScrolling<ConfigureClassWeightCategoriesViewModel>
    {
        public EditClassWeightCategoriesViewController()
        {
            Title = "Weight Categories";

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

            PowerPlannerUIHelper.ConfigureForInputsStyle(this);
        }

        private void SaveButton_Clicked(object sender, EventArgs e)
        {
            ViewModel.Save();
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            ViewModel.TryRemoveViewModelViaUserInteraction();
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            StackView.AddTopSectionDivider();

            var renderedComponent = ViewModel.Render();
            renderedComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            StackView.AddArrangedSubview(renderedComponent);
            renderedComponent.StretchWidth(StackView);

            StackView.AddBottomSectionDivider();
        }
    }
}