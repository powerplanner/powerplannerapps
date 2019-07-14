using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using InterfacesiOS.Views;
using ToolsPortable;
using PowerPlannerAppDataLibrary;

namespace PowerPlanneriOS.Controllers
{
    public class AddClassTimeViewController : PopupViewControllerWithScrolling<AddClassTimeViewModel>
    {
        public AddClassTimeViewController()
        {
            ConfigureForInputsStyle();
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            if (ViewModel.State == AddClassTimeViewModel.OperationState.Adding)
            {
                Title = "Add Time";
            }
            else
            {
                Title = "Edit Times";
            }

            BackButtonText = "Cancel";
            PositiveNavBarButton = new PopupRightNavBarButtonItem("Save", delegate { ViewModel.Save(); });

            AddTopSectionDivider();

            var timePickerFrom = new BareUIInlineTimePicker(this, left: 16, right: 16)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HeaderText = "From"
            };
            BindingHost.SetTimeBinding(timePickerFrom, nameof(ViewModel.StartTime));
            StackView.AddArrangedSubview(timePickerFrom);
            timePickerFrom.StretchWidth(StackView);
            timePickerFrom.SetHeight(44);

            AddDivider();

            var timePickerTo = new BareUIInlineTimePicker(this, left: 16, right: 16)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HeaderText = "To"
            };
            BindingHost.SetTimeBinding(timePickerTo, nameof(ViewModel.EndTime));
            StackView.AddArrangedSubview(timePickerTo);
            timePickerTo.StretchWidth(StackView);
            timePickerTo.SetHeight(44);

            AddSectionDivider();

            AddTextField(new UITextField()
            {
                Placeholder = "Room",
                ReturnKeyType = UIReturnKeyType.Default
            }, nameof(ViewModel.Room));

            AddSectionDivider();

            AddSwitch(DateTools.ToLocalizedString(DayOfWeek.Monday), nameof(ViewModel.IsMondayChecked));
            AddDivider();
            AddSwitch(DateTools.ToLocalizedString(DayOfWeek.Tuesday), nameof(ViewModel.IsTuesdayChecked));
            AddDivider();
            AddSwitch(DateTools.ToLocalizedString(DayOfWeek.Wednesday), nameof(ViewModel.IsWednesdayChecked));
            AddDivider();
            AddSwitch(DateTools.ToLocalizedString(DayOfWeek.Thursday), nameof(ViewModel.IsThursdayChecked));
            AddDivider();
            AddSwitch(DateTools.ToLocalizedString(DayOfWeek.Friday), nameof(ViewModel.IsFridayChecked));
            AddDivider();
            AddSwitch(DateTools.ToLocalizedString(DayOfWeek.Saturday), nameof(ViewModel.IsSaturdayChecked));
            AddDivider();
            AddSwitch(DateTools.ToLocalizedString(DayOfWeek.Sunday), nameof(ViewModel.IsSundayChecked));

            AddSectionDivider();

            var pickerWeek = new BareUIInlinePickerView(this, left: 16, right: 16)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HeaderText = "Week",
                ItemsSource = ViewModel.AvailableScheduleWeekStrings
            };
            BindingHost.SetSelectedItemBinding(pickerWeek, nameof(ViewModel.ScheduleWeekString));
            StackView.AddArrangedSubview(pickerWeek);
            pickerWeek.StretchWidth(StackView);
            pickerWeek.SetHeight(44);

            AddDivider();

            AddSpacing(6);

            var weekExplanation = new UILabel()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = PowerPlannerResources.GetString("EditingClassScheduleItemView_TextBlockWeekDescription.Text"),
                Lines = 0,
                Font = UIFont.PreferredCaption1,
                TextColor = UIColor.LightGray
            };
            StackView.AddArrangedSubview(weekExplanation);
            weekExplanation.StretchWidth(StackView, 16, 16);

            AddSpacing(12);

            AddSectionDivider();

            AddDeleteButtonWithConfirmation("Delete Class Times", ViewModel.Delete, "Delete class times?", "Are you sure you want to delete these class times?");

            AddBottomSectionDivider();
        }

        private void AddSwitch(string header, string bindingName)
        {
            var container = new BareUISwitch()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Header = header
            };
            BindingHost.SetSwitchBinding(container, bindingName);
            StackView.AddArrangedSubview(container);
            container.StretchWidth(StackView);
            container.SetHeight(44);
        }
    }
}