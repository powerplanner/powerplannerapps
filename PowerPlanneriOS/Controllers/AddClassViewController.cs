using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using InterfacesiOS.Views;
using ToolsPortable;
using PowerPlanneriOS.Views;
using PowerPlannerAppDataLibrary.DataLayer;

namespace PowerPlanneriOS.Controllers
{
    public class AddClassViewController : PopupViewControllerWithScrolling<AddClassViewModel>
    {
        public AddClassViewController()
        {
            ConfigureForInputsStyle();
        }

        public override void OnViewModelLoadedOverride()
        {
            Title = ViewModel.State == AddClassViewModel.OperationState.Adding ? "Add Class" : "Edit Class";

            AddTopSectionDivider();

            AddTextField(new UITextField()
            {
                Placeholder = "Name",
                ReturnKeyType = UIReturnKeyType.Done,
                EnablesReturnKeyAutomatically = true
            }, nameof(ViewModel.Name), firstResponder: ViewModel.State == AddClassViewModel.OperationState.Adding);

            AddDivider();

            var colorPicker = new BareUIInlineColorPickerView(this, left: 16, right: 16)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                AvailableColors = ColorItem.DefaultColors.Select(i => new BareUIInlineColorPickerView.ColorItem(i.Text, BareUIHelper.ToCGColor(i.Color)))
            };
            StackView.AddArrangedSubview(colorPicker);
            colorPicker.StretchWidth(StackView);
            colorPicker.SetHeight(44);
            BindingHost.SetSelectedColorBinding(colorPicker, nameof(ViewModel.Color));

            AddSectionDivider();

            var partialSemesterSwitch = new BareUISwitch()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Header = "Partial semester class?"
            };
            BindingHost.SetSwitchBinding(partialSemesterSwitch, nameof(ViewModel.IsPartialSemesterClass));
            StackView.AddArrangedSubview(partialSemesterSwitch);
            partialSemesterSwitch.StretchWidth(StackView);
            partialSemesterSwitch.SetHeight(44);

            var stackViewStartEndDates = new UIStackView() { Axis = UILayoutConstraintAxis.Vertical };
            {
                AddDivider(stackViewStartEndDates);

                var startDatePicker = new BareUIInlineDatePicker(this, left: 16, right: 16)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    HeaderText = "Start Date"
                };
                BindingHost.SetDateBinding(startDatePicker, nameof(ViewModel.StartDate));
                stackViewStartEndDates.AddArrangedSubview(startDatePicker);
                startDatePicker.StretchWidth(stackViewStartEndDates);
                startDatePicker.SetHeight(44);

                AddDivider(stackViewStartEndDates);

                var endDatePicker = new BareUIInlineDatePicker(this, left: 16, right: 16)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    HeaderText = "End Date"
                };
                BindingHost.SetDateBinding(endDatePicker, nameof(ViewModel.EndDate));
                stackViewStartEndDates.AddArrangedSubview(endDatePicker);
                endDatePicker.StretchWidth(stackViewStartEndDates);
                endDatePicker.SetHeight(44);
            }
            AddUnderVisiblity(stackViewStartEndDates, nameof(ViewModel.IsPartialSemesterClass));

            AddSectionDivider();

            if (ViewModel.IncludesEditingDetails)
            {
                var detailsView = new BareUITextView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Placeholder = "Details"
                };
                BindingHost.SetTextViewTextBinding(detailsView.TextView, nameof(ViewModel.Details));
                StackView.AddArrangedSubview(detailsView);
                detailsView.StretchWidth(StackView);
                detailsView.SetHeight(130);

                AddSectionDivider();
            }

            AddDeleteButtonWithConfirmation("Delete Class", ViewModel.Delete, "Delete class?", "Are you sure you want to delete this class and all of its items?");

            AddBottomSectionDivider();

            BackButtonText = "Cancel";
            PositiveNavBarButton = new PopupRightNavBarButtonItem("Save", delegate { Save(); });

            base.OnViewModelLoadedOverride();
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