using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.Controls
{
    /// <summary>
    /// A ViewModel that allows easy management of the time picker controls.
    /// Everything is setup in the constructor.
    /// Then, bind your ComboBox's items and selected item to the correct properties.
    /// </summary>
    public class TimePickerControlViewModel : BaseViewModel
    {
        // Variables to keep track of everything going on outside this ViewModel.
        public string StartTimePropertyName;
        public string EndTimePropertyName;
        public Func<TimeSpan> GetStartTime;
        public Func<TimeSpan> GetEndTime;

        bool _hasEndTime;
        Action<TimeSpan> _setStartTime;
        Action<TimeSpan> _setEndTime;

        private void StartTimeChanged(TimeSpan startTime)
        {
            _startTimeSelected = DateTimeFormatterExtension.Current.FormatAsShortTime(new DateTime(startTime.Ticks));
            OnPropertyChanged(nameof(StartTimeItems));
        }

        private void EndTimeChanged(TimeSpan endTime)
        {
            _endTimeSelected = DateTimeFormatterExtension.Current.FormatAsShortTime(new DateTime(endTime.Ticks)) + CustomTimePickerHelpers.GenerateTimeOffsetText(GetEndTime() - GetStartTime());
            OnPropertyChanged(nameof(EndTimeItems), nameof(EndTimeSelected));
        }

        public TimePickerControlViewModel(bool hasEndTime, string startTimePropertyName, Func<TimeSpan> getStartTime, Action<TimeSpan> setStartTime, string endTimePropertyName, Func<TimeSpan> getEndTime, Action<TimeSpan> setEndTime, BaseViewModel parent) : base(parent)
        {
            _hasEndTime = hasEndTime;
            GetStartTime = getStartTime;
            GetEndTime = getEndTime;
            _setStartTime = setStartTime;
            _setEndTime = setEndTime;
            StartTimePropertyName = startTimePropertyName;
            EndTimePropertyName = endTimePropertyName;

            Parent.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == StartTimePropertyName) 
                    StartTimeChanged(GetStartTime());
                else if (e.PropertyName == EndTimePropertyName) 
                    EndTimeChanged(GetEndTime());
            };

            StartTimeChanged(GetStartTime());
            EndTimeChanged(GetEndTime());
        }
        public ObservableCollection<string> StartTimeItems
        {
            get => CachedComputation(() => CustomTimePickerHelpers.GenerateTimes(DateTime.MinValue, new DateTime(GetStartTime().Ticks), CustomTimePickerHelpers.CUSTOM_TIME_PICKER_DEFAULT_INTERVAL), StartTimeItemsDependencies);
        }
        static readonly string[] StartTimeItemsDependencies = new string[] { nameof(StartTimeSelected) };

        private string _startTimeSelected;
        public string StartTimeSelected
        {
            get => _startTimeSelected;
            set
            {
                if (value == null)
                    return;

                // NOTE: This setter should only be called from the ComboBox.
                // So, the ComboBox has just been set - now, we need to attempt to parse that value as a TimeSpan, and if we can't, then show a message.
                if (!CustomTimePickerHelpers.ParseComboBoxItem(DateTime.MinValue, value, out var t))
                {
                    new PortableMessageDialog(PowerPlannerResources.GetString("EditingClassScheduleItemView_InvalidStartTime.Content"), PowerPlannerResources.GetString("EditingClassScheduleItemView_InvalidStartTime.Title")).Show();
                    return;
                }

                // Since this has now changed, we will change the "StartTime", which will in return set "_startTimeSelected" to the correct formatted value.
                _setStartTime(t);
            }
        }

        public ObservableCollection<string> EndTimeItems
        {
            get => CachedComputation(() => CustomTimePickerHelpers.GenerateTimesWithOffset(new DateTime(GetStartTime().Ticks), new DateTime(GetEndTime().Ticks), CustomTimePickerHelpers.CUSTOM_TIME_PICKER_DEFAULT_INTERVAL), EndTimeItemsDependencies);
        }
        static readonly string[] EndTimeItemsDependencies = new string[] { nameof(EndTimeSelected) };

        private string _endTimeSelected;
        public string EndTimeSelected
        {
            get => _endTimeSelected;
            set
            {
                if (value == null)
                    return;

                // NOTE: This setter should only be called from the ComboBox.
                // So, the ComboBox has just been set - now, we need to attempt to parse that value as a TimeSpan, and if we can't, then show a message.
                if (!CustomTimePickerHelpers.ParseComboBoxItem(new DateTime(GetStartTime().Ticks), value, out var t))
                {
                    new PortableMessageDialog(PowerPlannerResources.GetString("EditingClassScheduleItemView_InvalidEndTime.Content"), PowerPlannerResources.GetString("EditingClassScheduleItemView_InvalidEndTime.Title")).Show();
                    return;
                }

                // Since this has now changed, we will change the "EndTime", which will in return set "_endTimeSelected" to the correct formatted value.
                _setEndTime(t);
            }
        }
    }
}
