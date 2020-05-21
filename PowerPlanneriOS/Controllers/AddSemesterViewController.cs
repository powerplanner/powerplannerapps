using Foundation;
using InterfacesiOS.Controllers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years;
using System;
using UIKit;
using InterfacesiOS.Views;
using ToolsPortable;

namespace PowerPlanneriOS.Controllers
{
    public class AddSemesterViewController : PopupViewControllerWithScrolling<AddSemesterViewModel>
    {
        public AddSemesterViewController()
        {
            ConfigureForInputsStyle();
        }

        public override void OnViewModelAndViewLoadedOverride()
        {
            Title = ViewModel.State == AddSemesterViewModel.OperationState.Adding ? "Add Semester" : "Edit Semester";

            ViewModel.SupportsStartEnd = true;

            BackButtonText = "Cancel";
            PositiveNavBarButton = new PopupRightNavBarButtonItem("Save", delegate { Save(); });

            AddTopSectionDivider();

            AddTextField(new UITextField()
            {
                Placeholder = "Name (ex: Fall)",
                ReturnKeyType = UIReturnKeyType.Done,
                EnablesReturnKeyAutomatically = true
            }, nameof(ViewModel.Name), firstResponder: ViewModel.State == AddSemesterViewModel.OperationState.Adding);

            AddSectionDivider();

            var datePickerStart = new BareUIInlineDatePicker(this, left: 16, right: 16)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HeaderText = "Start Date"
            };
            BindingHost.SetDateBinding(datePickerStart, nameof(ViewModel.StartDate));
            StackView.AddArrangedSubview(datePickerStart);
            datePickerStart.StretchWidth(StackView);
            datePickerStart.SetHeight(44);

            AddDivider();

            var datePickerEnd = new BareUIInlineDatePicker(this, left: 16, right: 16)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HeaderText = "End Date"
            };
            BindingHost.SetDateBinding(datePickerEnd, nameof(ViewModel.EndDate));
            StackView.AddArrangedSubview(datePickerEnd);
            datePickerEnd.StretchWidth(StackView);
            datePickerEnd.SetHeight(44);

            AddSectionDivider();

            AddDeleteButtonWithConfirmation("Delete Semester", ViewModel.Delete, "Delete semester?", "Are you sure you want to delete this semester and all of its classes, grades, tasks, and events?");

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