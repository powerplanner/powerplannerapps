using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;
using InterfacesiOS.Views;
using PowerPlanneriOS.Views;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary;
using System.Collections;
using PowerPlanneriOS.Helpers;
using ToolsPortable;

namespace PowerPlanneriOS.Controllers
{
    public class AddHomeworkViewController : PopupViewControllerWithScrolling<AddHomeworkViewModel>
    {
        public AddHomeworkViewController()
        {
            ConfigureForInputsStyle();
        }

        public override void OnViewModelLoadedOverride()
        {
            Title = ViewModel.State == AddHomeworkViewModel.OperationState.Adding ?
                ViewModel.Type == AddHomeworkViewModel.ItemType.Homework ? "Add Task" : "Add Event"
                : ViewModel.Type == AddHomeworkViewModel.ItemType.Homework ? "Edit Task" : "Edit Event";

            AddTopSectionDivider();

            AddTextField(new UITextField()
            {
                Placeholder = "Name",
                ReturnKeyType = UIReturnKeyType.Done,
                EnablesReturnKeyAutomatically = true
            }, nameof(ViewModel.Name), firstResponder: ViewModel.State == AddHomeworkViewModel.OperationState.Adding);

            AddDivider();

            var dateContainer = new UIStackView() { Axis = UILayoutConstraintAxis.Vertical };
            {
                var datePicker = new BareUIInlineDatePicker(this, left: 16, right: 16)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                BindingHost.SetDateBinding(datePicker, nameof(ViewModel.Date));
                dateContainer.AddArrangedSubview(datePicker);
                datePicker.StretchWidth(dateContainer);
                datePicker.SetHeight(44);

                AddDivider(dateContainer);
            }
            AddUnderVisiblity(dateContainer, nameof(ViewModel.IsDatePickerVisible));

            var classContainer = new UIStackView() { Axis = UILayoutConstraintAxis.Vertical };
            {
                var pickerClass = new BareUIInlinePickerView(this, left: 16, right: 16)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    HeaderText = "Class",
                    ItemToViewConverter = ConvertClassToInlineView
                };
                BindingHost.SetItemsSourceBinding(pickerClass, nameof(ViewModel.Classes));
                BindingHost.SetSelectedItemBinding(pickerClass, nameof(ViewModel.Class));
                classContainer.AddArrangedSubview(pickerClass);
                pickerClass.StretchWidth(classContainer);
                pickerClass.SetHeight(44);

                AddDivider(classContainer);
            }
            AddUnderVisiblity(classContainer, nameof(ViewModel.IsClassPickerVisible));

            var gradeWeightContainer = new UIStackView() { Axis = UILayoutConstraintAxis.Vertical };
            {
                var pickerGradeWeight = new BareUIInlinePickerView(this, left: 16, right: 16)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    HeaderText = "Grade Category"
                };
                BindingHost.SetItemsSourceBinding(pickerGradeWeight, nameof(ViewModel.WeightCategories));
                BindingHost.SetSelectedItemBinding(pickerGradeWeight, nameof(ViewModel.SelectedWeightCategory));
                gradeWeightContainer.AddArrangedSubview(pickerGradeWeight);
                pickerGradeWeight.StretchWidth(gradeWeightContainer);
                pickerGradeWeight.SetHeight(44);

                AddDivider(gradeWeightContainer);
            }
            AddUnderVisiblity(gradeWeightContainer, nameof(ViewModel.IsWeightCategoryPickerVisible));

            var pickerTime = new BareUIInlinePickerView(this, left: 16, right: 16)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HeaderText = "Time"
            };
            BindingHost.SetItemsSourceBinding(pickerTime, nameof(ViewModel.TimeOptions));
            BindingHost.SetSelectedItemBinding(pickerTime, nameof(ViewModel.SelectedTimeOption));
            StackView.AddArrangedSubview(pickerTime);
            pickerTime.StretchWidth(StackView);
            pickerTime.SetHeight(44);

            var stackViewPickerCustomTime = new UIStackView() { Axis = UILayoutConstraintAxis.Vertical };
            AddDivider(stackViewPickerCustomTime);
            if (ViewModel.Type == AddHomeworkViewModel.ItemType.Homework)
            {
                var pickerDueTime = new BareUIInlineTimePicker(this, left: 16, right: 16)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    HeaderText = "Due Time"
                };
                BindingHost.SetTimeBinding(pickerDueTime, nameof(ViewModel.StartTime));
                stackViewPickerCustomTime.AddArrangedSubview(pickerDueTime);
                pickerDueTime.StretchWidth(stackViewPickerCustomTime);
                pickerDueTime.SetHeight(44);
            }
            else
            {
                var pickerStartTime = new BareUIInlineTimePicker(this, left: 16, right: 16)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    HeaderText = "Start Time"
                };
                BindingHost.SetTimeBinding(pickerStartTime, nameof(ViewModel.StartTime));
                stackViewPickerCustomTime.AddArrangedSubview(pickerStartTime);
                pickerStartTime.StretchWidth(stackViewPickerCustomTime);
                pickerStartTime.SetHeight(44);

                AddDivider(stackViewPickerCustomTime);

                var pickerEndTime = new BareUIInlineTimePicker(this, left: 16, right: 16)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    HeaderText = "End Time"
                };
                BindingHost.SetTimeBinding(pickerEndTime, nameof(ViewModel.EndTime));
                stackViewPickerCustomTime.AddArrangedSubview(pickerEndTime);
                pickerEndTime.StretchWidth(stackViewPickerCustomTime);
                pickerEndTime.SetHeight(44);
            }
            var pickerCustomTimeContainer = new BareUIVisibilityContainer()
            {
                Child = stackViewPickerCustomTime
            };
            BindingHost.SetVisibilityBinding(pickerCustomTimeContainer, nameof(ViewModel.IsStartTimePickerVisible));
            StackView.AddArrangedSubview(pickerCustomTimeContainer);
            pickerCustomTimeContainer.StretchWidth(StackView);

            if (ViewModel.IsInDifferentTimeZone)
            {
                AddSectionDivider();

                AddSpacing(12);

                var timeZoneWarning = new UILabel()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Text = PowerPlannerResources.GetString("DifferentTimeZoneWarning.Text"),
                    TextColor = UIColor.Red,
                    Font = UIFont.PreferredCaption1,
                    Lines = 0
                };
                StackView.AddArrangedSubview(timeZoneWarning);
                timeZoneWarning.StretchWidth(StackView, left: 16, right: 16);

                AddSpacing(12);
            }

            AddSectionDivider();

            var detailsView = new BareUITextView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Placeholder = "Details"
            };
            BindingHost.SetTextViewTextBinding(detailsView.TextView, nameof(ViewModel.Details));
            StackView.AddArrangedSubview(detailsView);
            detailsView.StretchWidth(StackView);
            detailsView.SetHeight(130);

            if (ViewModel.IsRepeatsVisible)
            {
                AddSectionDivider();

                var switchRepeats = new BareUISwitch()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Header = PowerPlannerResources.GetString("RepeatingEntry_CheckBoxRepeats.Content")
                };
                BindingHost.SetSwitchBinding(switchRepeats, nameof(ViewModel.Repeats));
                StackView.AddArrangedSubview(switchRepeats);
                switchRepeats.StretchWidth(StackView);
                switchRepeats.SetHeight(44);

                StackView.AddUnderLazyVisibility(BindingHost, nameof(ViewModel.Repeats), delegate
                {
                    var recurrenceContainer = new UIStackView() { Axis = UILayoutConstraintAxis.Vertical };
                    {
                        recurrenceContainer.AddDivider();

                        var tryForFreeContainer = new UIStackView() { Axis = UILayoutConstraintAxis.Vertical };
                        {
                            tryForFreeContainer.AddSpacing(16);
                            var tryForFreeLabel = new UILabel()
                            {
                                TranslatesAutoresizingMaskIntoConstraints = false,
                                Text = PowerPlannerResources.GetString("RepeatingEntry_TextBlockTryForFree.Text"),
                                Font = UIFont.PreferredCaption1,
                                TextColor = UIColor.Red,
                                Lines = 0
                            };
                            tryForFreeContainer.AddArrangedSubview(tryForFreeLabel);
                            tryForFreeLabel.StretchWidth(tryForFreeContainer, left: 16, right: 16);
                            tryForFreeContainer.AddSpacing(16);

                            tryForFreeContainer.AddDivider();
                        }
                        recurrenceContainer.AddUnderVisiblity(tryForFreeContainer, BindingHost, nameof(ViewModel.ShowRepeatingPremiumTrial));

                        var mustUpgradeContainer = new UIStackView() { Axis = UILayoutConstraintAxis.Vertical };
                        {
                            mustUpgradeContainer.AddSpacing(16);
                            var mustUpgradeLabel = new UILabel()
                            {
                                TranslatesAutoresizingMaskIntoConstraints = false,
                                Text = PowerPlannerResources.GetString("RepeatingEntry_TextBlockMustUpgrade.Text"),
                                Font = UIFont.PreferredCaption1,
                                TextColor = UIColor.Red,
                                Lines = 0
                            };
                            mustUpgradeContainer.AddArrangedSubview(mustUpgradeLabel);
                            mustUpgradeLabel.StretchWidth(mustUpgradeContainer, left: 16, right: 16);
                            mustUpgradeContainer.AddSpacing(8);

                            var buttonUpgrade = PowerPlannerUIHelper.CreatePowerPlannerBlueButton(PowerPlannerResources.GetString("Settings_UpgradeToPremium_ButtonUpgrade.Content"));
                            buttonUpgrade.TranslatesAutoresizingMaskIntoConstraints = false;
                            buttonUpgrade.TouchUpInside += new WeakEventHandler<EventArgs>(delegate { ViewModel.UpgradeToPremiumForRepeating(); }).Handler;
                            mustUpgradeContainer.AddArrangedSubview(buttonUpgrade);
                            buttonUpgrade.StretchWidth(mustUpgradeContainer, left: 16, right: 16);
                            mustUpgradeContainer.AddSpacing(16);

                            mustUpgradeContainer.AddDivider();
                        }
                        recurrenceContainer.AddUnderVisiblity(mustUpgradeContainer, BindingHost, nameof(ViewModel.ShowRepeatingMustUpgradeToPremium));

                        var recurrenceView = new UIRecurrenceView(this)
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false,
                            ViewModel = ViewModel.RecurrenceControlViewModel
                        };
                        BindingHost.SetIsEnabledBinding(recurrenceView, nameof(ViewModel.IsRepeatingEntryEnabled));
                        recurrenceContainer.AddArrangedSubview(recurrenceView);
                        recurrenceView.StretchWidth(recurrenceContainer);

                        recurrenceContainer.AddDivider();

                        recurrenceContainer.AddSpacing(16);
                        var labelNoteCannotBulkEdit = new UILabel()
                        {
                            TranslatesAutoresizingMaskIntoConstraints = false,
                            Text = PowerPlannerResources.GetString("RepeatingEntry_TextBlockNoteCannotBulkEdit.Text"),
                            Font = UIFont.PreferredCaption1,
                            TextColor = UIColor.LightGray,
                            Lines = 0
                        };
                        recurrenceContainer.AddArrangedSubview(labelNoteCannotBulkEdit);
                        labelNoteCannotBulkEdit.StretchWidth(recurrenceContainer, left: 16, right: 16);
                        recurrenceContainer.AddSpacing(16);
                    }
                    return recurrenceContainer;
                });
            }

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

        private static UIView ConvertClassToInlineView(object item, UIView recycledView)
        {
            var c = item as ViewItemClass;

            var view = recycledView as BareUIPickerViewItemTextWithColorCircle;
            if (view == null)
            {
                view = new BareUIPickerViewItemTextWithColorCircle();
            }
            view.Text = c.Name;
            view.Color = BareUIHelper.ToCGColor(c.Color);

            return view;
        }
    }
}