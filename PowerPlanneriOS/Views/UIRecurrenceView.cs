using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Foundation;
using InterfacesiOS.Binding;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.Controls;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS.Views
{
    public class UIRecurrenceView : UIStackView
    {
        private UIViewController _viewController;

        public UIRecurrenceView(UIViewController viewController)
        {
            _viewController = viewController;
            Axis = UILayoutConstraintAxis.Vertical;
        }

        private BindingHost _bindingHost = new BindingHost();

        private RecurrenceControlViewModel _viewModel;
        public RecurrenceControlViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                if (_viewModel != null)
                {
                    throw new InvalidOperationException("We don't allow changing the view model once assigned.");
                }

                if (value != null)
                {
                    _viewModel = value;
                    _bindingHost.DataContext = value;
                    InitializeViews();
                }
            }
        }

        private UIInlineMultiDayPickerView _pickerRepeatOn;

        private void InitializeViews()
        {
            // [pickerRepeatsEvery]

            // 1 - 364
            uint[] intervalNumberRange = GenerateNumberSequence(1, 364);
            var pickerIntervalAndType = new BareUIInlinePickerView(_viewController, left: 16, right: 16)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HeaderText = PowerPlannerResources.GetString("RepeatingEntry_TextBlockRepeatEvery.Text"),
                Components = new IEnumerable[]
                {
                    intervalNumberRange,
                    ViewModel.RepeatOptionsAsStrings
                },
                SelectedItem = new object[]
                {
                    ViewModel.GetRepeatIntervalAsNumber(),
                    ViewModel.SelectedRepeatOptionAsString
                }
            };
            pickerIntervalAndType.SelectionChanged += PickerIntervalAndType_SelectionChanged;
            this.AddArrangedSubview(pickerIntervalAndType);
            pickerIntervalAndType.StretchWidth(this);
            pickerIntervalAndType.SetHeight(44);

            var repeatOnContainer = new UIStackView() { Axis = UILayoutConstraintAxis.Vertical };
            {
                repeatOnContainer.AddDivider();

                _pickerRepeatOn = new UIInlineMultiDayPickerView(_viewController, left: 16, right: 16)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    HeaderText = PowerPlannerResources.GetString("RepeatingEntry_TextBlockRepeatOn.Text"),
                    SelectedDays = ViewModel.DayCheckBoxes.Where(i => i.IsChecked).Select(i => i.DayOfWeek).ToArray()
                };
                _pickerRepeatOn.SelectionsChanged += PickerRepeatOn_SelectionsChanged;
                repeatOnContainer.AddArrangedSubview(_pickerRepeatOn);
                _pickerRepeatOn.StretchWidth(repeatOnContainer);
                _pickerRepeatOn.SetHeight(44);
            }
            this.AddUnderVisiblity(repeatOnContainer, _bindingHost, nameof(ViewModel.AreDayCheckBoxesVisible));

            this.AddDivider();

            var pickerEndType = new BareUIInlinePickerView(_viewController, left: 16, right: 16)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HeaderText = PowerPlannerResources.GetString("RepeatingEntry_TextBlockEnds.Text"),
                ItemsSource = new string[]
                {
                    PowerPlannerResources.GetString("RepeatingEntry_RadioButtonEndsOn.Content"),
                    PowerPlannerResources.GetString("RepeatingEntry_RadioButtonEndsAfter.Content")
                }
            };
            if (ViewModel.SelectedEndOption == RecurrenceControlViewModel.EndOptions.Date)
            {
                pickerEndType.SelectedItem = pickerEndType.ItemsSource.OfType<object>().First();
            }
            else
            {
                pickerEndType.SelectedItem = pickerEndType.ItemsSource.OfType<object>().Last();
            }
            pickerEndType.SelectionChanged += PickerEndType_SelectionChanged;
            this.AddArrangedSubview(pickerEndType);
            pickerEndType.StretchWidth(this);
            pickerEndType.SetHeight(44);

            var endDateContainer = new UIStackView() { Axis = UILayoutConstraintAxis.Vertical };
            {
                endDateContainer.AddDivider();

                var endDatePicker = new BareUIInlineDatePicker(_viewController, left: 16, right: 16)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    HeaderText = PowerPlannerResources.GetString("EditTaskOrEventPage_DatePickerDate.Header"), // "Date"
                    Date = ViewModel.EndDate
                };
                endDatePicker.DateChanged += EndDatePicker_DateChanged;
                endDateContainer.AddArrangedSubview(endDatePicker);
                endDatePicker.StretchWidth(endDateContainer);
                endDatePicker.SetHeight(44);
            }
            this.AddUnderVisiblity(endDateContainer, _bindingHost, nameof(ViewModel.IsEndDateChecked));

            var endOccurrencesContainer = new UIStackView() { Axis = UILayoutConstraintAxis.Vertical };
            {
                endOccurrencesContainer.AddDivider();

                var endOccurrencesPicker = new BareUIInlinePickerView(_viewController, left: 16, right: 16)
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    HeaderText = PowerPlannerResources.GetCapitalizedString("RepeatingEntry_TextBlockOccurrences.Text"),
                    ItemsSource = GenerateNumberSequence(2, 50),
                    SelectedItem = ViewModel.GetEndOccurrencesAsNumber()
                };
                endOccurrencesPicker.SelectionChanged += EndOccurrencesPicker_SelectionChanged;
                endOccurrencesContainer.AddArrangedSubview(endOccurrencesPicker);
                endOccurrencesPicker.StretchWidth(endOccurrencesContainer);
                endOccurrencesPicker.SetHeight(44);
            }
            this.AddUnderVisiblity(endOccurrencesContainer, _bindingHost, nameof(ViewModel.IsEndOccurrencesChecked));
        }

        private void EndOccurrencesPicker_SelectionChanged(object sender, object e)
        {
            if (e != null)
            {
                ViewModel.EndOccurrencesAsString = e.ToString();
            }
        }

        private static uint[] GenerateNumberSequence(uint start, uint end)
        {
            // Inclusive
            uint[] sequence = new uint[(end - start) + 1];
            for (uint i = 0; i < sequence.Length; i++)
            {
                sequence[i] = i + start;
            }

            return sequence;
        }

        private void EndDatePicker_DateChanged(object sender, DateTime? e)
        {
            if (e != null)
            {
                ViewModel.EndDate = e.Value;
            }
        }

        private void PickerEndType_SelectionChanged(object sender, object e)
        {
            var itemsSource = (sender as BareUIInlinePickerView).ItemsSource;
            int index = itemsSource.OfType<object>().ToList().IndexOf(e);
            ViewModel.SelectedEndOption = (RecurrenceControlViewModel.EndOptions)index;
        }

        private void PickerIntervalAndType_SelectionChanged(object sender, object e)
        {
            var selectedComponents = e as object[];
            if (selectedComponents != null)
            {
                uint interval = (uint)selectedComponents[0];
                string repeatOptionAsString = (string)selectedComponents[1];

                ViewModel.RepeatIntervalAsString = interval.ToString();
                ViewModel.SelectedRepeatOptionAsString = repeatOptionAsString;
            }
        }

        private void PickerRepeatOn_SelectionsChanged(object sender, DayOfWeek[] e)
        {
            foreach (var day in ViewModel.DayCheckBoxes)
            {
                day.IsChecked = _pickerRepeatOn.SelectedDays.Contains(day.DayOfWeek);
            }
        }
    }
}