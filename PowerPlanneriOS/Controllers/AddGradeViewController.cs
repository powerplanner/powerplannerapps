using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.Converters;
using PowerPlannerSending;

namespace PowerPlanneriOS.Controllers
{
    public class AddGradeViewController : PopupViewControllerWithScrolling<AddGradeViewModel>
    {
        public AddGradeViewController()
        {
            ConfigureForInputsStyle();
        }

        public override void OnViewModelSetOverride()
        {
            // Enable setting IsDropped
            ViewModel.UsesIsDropped = true;

            base.OnViewModelSetOverride();
        }

        public override void OnViewModelLoadedOverride()
        {
            Title = ViewModel.State == AddGradeViewModel.OperationState.Adding ? "Add Grade" : "Edit Grade";

            AddTopSectionDivider();

            AddTextField(new UITextField()
            {
                Placeholder = "Name",
                ReturnKeyType = UIReturnKeyType.Done,
                EnablesReturnKeyAutomatically = true
            }, nameof(ViewModel.Name), firstResponder: ViewModel.State == AddGradeViewModel.OperationState.Adding);

            AddDivider();

            base.OnViewModelLoadedOverride();

            // Grade received, grade total, and percent
            var gradesView = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            {
                var textFieldReceived = new UITextField()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    ReturnKeyType = UIReturnKeyType.Next,
                    KeyboardType = UIKeyboardType.DecimalPad,
                    AdjustsFontSizeToFitWidth = true
                    // TODO: Wire up Next button on keyboard by using ShouldReturn
                };
                BindingHost.SetTextFieldTextBinding<double>(textFieldReceived, nameof(ViewModel.GradeReceived), converter: GradeToTextBoxTextConverter.Convert, backConverter: GradeToTextBoxTextConverter.ConvertBack);
                if (ViewModel.State == AddGradeViewModel.OperationState.Editing && ViewModel.GradeReceived == Grade.UNGRADED)
                {
                    // When assigning grades to tasks, the empty grade becomes the first responder
                    textFieldReceived.BecomeFirstResponder();
                }
                gradesView.Add(textFieldReceived);
                textFieldReceived.StretchHeight(gradesView);

                var labelOutOf = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = "out of",
                    Font = UIFont.PreferredCaption1
                };
                gradesView.Add(labelOutOf);
                labelOutOf.StretchHeight(gradesView);

                var textFieldTotal = new UITextField()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    ReturnKeyType = UIReturnKeyType.Next,
                    KeyboardType = UIKeyboardType.DecimalPad,
                    AdjustsFontSizeToFitWidth = true
                    // TODO: Wire up Next button on keyboard
                };
                BindingHost.SetTextFieldTextBinding<double>(textFieldTotal, nameof(ViewModel.GradeTotal), converter: GradeToTextBoxTextConverter.Convert, backConverter: GradeToTextBoxTextConverter.ConvertBack);
                gradesView.Add(textFieldTotal);
                textFieldTotal.StretchHeight(gradesView);

                var labelPercent = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.PreferredBody
                };
                BindingHost.SetLabelTextBinding(labelPercent, nameof(ViewModel.GradePercent));
                gradesView.Add(labelPercent);
                labelPercent.StretchHeight(gradesView);

                gradesView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[received(46)]-4-[outOf]-4-[total(46)]->=0-[percent]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                    "received", textFieldReceived,
                    "outOf", labelOutOf,
                    "total", textFieldTotal,
                    "percent", labelPercent));
            }
            StackView.AddArrangedSubview(gradesView);
            gradesView.StretchWidth(StackView, left: 16, right: 16);
            gradesView.SetHeight(44);

            AddDivider();

            var datePicker = new BareUIInlineDatePicker(this, left: 16, right: 16)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            BindingHost.SetDateBinding(datePicker, nameof(ViewModel.Date));
            StackView.AddArrangedSubview(datePicker);
            datePicker.StretchWidth(StackView);
            datePicker.SetHeight(44);

            AddDivider();

            var pickerGradeWeight = new BareUIInlinePickerView(this, left: 16, right: 16)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HeaderText = "Weight Category"
            };
            BindingHost.SetItemsSourceBinding(pickerGradeWeight, nameof(ViewModel.WeightCategories));
            BindingHost.SetSelectedItemBinding(pickerGradeWeight, nameof(ViewModel.SelectedWeightCategory));
            StackView.AddArrangedSubview(pickerGradeWeight);
            pickerGradeWeight.StretchWidth(StackView);
            pickerGradeWeight.SetHeight(44);

            AddDivider();

            var container = new BareUISwitch()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Header = "Is Dropped"
            };
            BindingHost.SetSwitchBinding(container, nameof(ViewModel.IsDropped));
            StackView.AddArrangedSubview(container);
            container.StretchWidth(StackView);
            container.SetHeight(44);

            AddDivider();

            var detailsView = new BareUITextView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Placeholder = "Details"
            };
            BindingHost.SetTextViewTextBinding(detailsView.TextView, nameof(ViewModel.Details));
            StackView.AddArrangedSubview(detailsView);
            detailsView.StretchWidth(StackView);
            detailsView.SetHeight(130);

            AddBottomSectionDivider();

            BackButtonText = "Cancel";
            PositiveNavBarButton = new PopupRightNavBarButtonItem("Save", delegate { Save(); });
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