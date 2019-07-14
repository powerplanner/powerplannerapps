using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Holiday;
using InterfacesiOS.Views;

namespace PowerPlanneriOS.Controllers
{
    public class AddHolidayViewController : PopupViewControllerWithScrolling<AddHolidayViewModel>
    {
        public AddHolidayViewController()
        {
            ConfigureForInputsStyle();
        }

        public override void OnViewModelLoadedOverride()
        {
            Title = ViewModel.State == AddHolidayViewModel.OperationState.Adding ? "Add Holiday" : "Edit Holiday";

            AddTopSectionDivider();

            AddTextField(new UITextField()
            {
                Placeholder = "Name",
                ReturnKeyType = UIReturnKeyType.Done,
                EnablesReturnKeyAutomatically = true
            }, nameof(ViewModel.Name), firstResponder: ViewModel.State == AddHolidayViewModel.OperationState.Adding);

            AddDivider();

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

            AddDeleteButtonWithConfirmation("Delete Holiday", ViewModel.Delete, "Delete holiday?", "Are you sure you want to delete this holiday?");

            AddBottomSectionDivider();

            BackButtonText = "Cancel";
            PositiveNavBarButton = new PopupRightNavBarButtonItem("Save", delegate { Save(); });

            base.OnViewModelLoadedOverride();
        }

        private void Save()
        {
            ViewModel.Save();
        }
    }
}