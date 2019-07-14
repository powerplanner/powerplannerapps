using Foundation;
using InterfacesiOS.Controllers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years;
using System;
using UIKit;
using InterfacesiOS.Views;
using ToolsPortable;

namespace PowerPlanneriOS.Controllers
{
    public class AddYearViewController : PopupViewControllerWithScrolling<AddYearViewModel>
    {
        public AddYearViewController()
        {
            ConfigureForInputsStyle();
        }

        public override void OnViewModelAndViewLoadedOverride()
        {
            Title = ViewModel.State == AddYearViewModel.OperationState.Adding ? "Add Year" : "Edit Year";

            BackButtonText = "Cancel";
            PositiveNavBarButton = new PopupRightNavBarButtonItem("Save", delegate { Save(); });

            AddTopSectionDivider();

            AddTextField(new UITextField()
            {
                Placeholder = "Name (ex: Freshman)",
                ReturnKeyType = UIReturnKeyType.Done,
                EnablesReturnKeyAutomatically = true
            }, nameof(ViewModel.Name), firstResponder: ViewModel.State == AddYearViewModel.OperationState.Adding);

            AddSectionDivider();

            AddDeleteButtonWithConfirmation("Delete Year", ViewModel.Delete, "Delete year?", "Are you sure you want to delete this year and all of its semesters, classes, grades, and homework?");

            AddBottomSectionDivider();

            base.OnViewModelAndViewLoadedOverride();
        }

        protected override bool HandleKeyboardAction(UIReturnKeyType returnKeyType)
        {
            if (returnKeyType == UIReturnKeyType.Done)
            {
                Save();
                return true;
            }

            return base.HandleKeyboardAction(returnKeyType);
        }

        private void Save()
        {
            ViewModel.Save();
        }
    }
}