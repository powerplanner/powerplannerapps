using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using Vx.Extensions;
using Vx.Helpers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Vx.Uwp.Controls.TimePickers
{
    public partial class TextBasedTimePicker : UserControl
    {
        /// <summary>
        /// Gets whether the OS supports this control (supported in 17763 and up), which 7% of my users are still lower than that as of 1/25/2020.
        /// </summary>
        public static bool IsSupported => ApiInformation.IsEventPresent(typeof(ComboBox).FullName, "TextSubmitted");

        public const int CUSTOM_TIME_PICKER_DEFAULT_INTERVAL = 30;

        public TextBasedTimePicker()
        {
            InitializeComponent();

            TimePickerComboBox.ItemsSource = _timeEntries;

            // Update selected time
            OnSelectedTimeChanged();
        }

        protected ObservableCollection<TimeEntry> _timeEntries = new ObservableCollection<TimeEntry>();

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(TextBasedTimePicker), new PropertyMetadata(""));

        /// <summary>
        /// The selected time.
        /// </summary>
        public TimeSpan SelectedTime
        {
            get { return (TimeSpan)GetValue(SelectedTimeProperty); }
            set { SetValue(SelectedTimeProperty, value); }
        }

        public static readonly DependencyProperty SelectedTimeProperty =
            DependencyProperty.Register(nameof(SelectedTime), typeof(TimeSpan), typeof(TextBasedTimePicker), new PropertyMetadata(new TimeSpan(9, 0, 0), OnSelectedTimeChanged));

        private static void OnSelectedTimeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as TextBasedTimePicker).OnSelectedTimeChanged();
        }

        private void OnSelectedTimeChanged()
        {
            if (SelectedTime.TotalDays >= 1)
            {
                throw new InvalidOperationException("SelectedTime must be less than a day.");
            }

            // Update items and remove any non-30 minute interval times that aren't selected
            UpdateItems();

            var matching = _timeEntries.FirstOrDefault(i => i.Time == SelectedTime);
            if (matching != null)
            {
                // Select it
                TimePickerComboBox.SelectedItem = matching;
            }
            else
            {
                // Wasn't in the list, add it and select it
                TimePickerComboBox.SelectedItem = AddTime(SelectedTime);
            }
        }

        private void TimePickerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimePickerComboBox.SelectedItem is TimeEntry entry)
            {
                SelectedTime = entry.Time;
            }
        }

        protected void UpdateItems()
        {
            var desired = GenerateEntries().ToList();
            if (desired.Count == 0)
            {
                desired.Add(CreateTimeEntry(new TimeSpan(23, 59, 0)));
            }
            _timeEntries.MakeListLike(desired);
        }

        private IEnumerable<TimeEntry> GenerateEntries()
        {
            var start = GetStartTime();
            var interval = TimeSpan.FromMinutes(CUSTOM_TIME_PICKER_DEFAULT_INTERVAL);
            var current = start;
            var addedExtraItem = false;

            while (current.Days == 0)
            {
                // Don't add the extra item if it exactly matches something already in the list.
                if (SelectedTime == current)
                    addedExtraItem = true;

                if (!addedExtraItem && current > SelectedTime)
                {
                    yield return CreateTimeEntry(SelectedTime);
                    addedExtraItem = true;
                }

                yield return CreateTimeEntry(current);
                current = current.Add(interval);
            }
        }

        protected virtual TimeSpan GetStartTime()
        {
            return new TimeSpan();
        }

        protected virtual bool ParseText(string text, out TimeSpan time)
        {
            return CustomTimePickerHelpers.ParseComboBoxItem(DateTime.MinValue, text, out time);
        }

        private void TimePickerComboBox_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            // https://docs.microsoft.com/en-us/windows/uwp/design/controls-and-patterns/combo-box#text-submitted
            // This is only invoked for unknown (unmatched) values

            // However there seems to be a bug where it submits even if user selects from list, after they then leave the text control
            if (_timeEntries.Any(i => i.DisplayText == args.Text))
            {
                // Don't do anything, framework should correctly update (keep) the selected item
                return;
            }

            if (ParseText(args.Text, out TimeSpan time))
            {
                var matching = _timeEntries.FirstOrDefault(i => i.Time == time);
                if (matching != null)
                {
                    // Select the item
                    args.Handled = true;
                    TimePickerComboBox.SelectedItem = matching;
                }
                else
                {
                    // Add the item and select it
                    args.Handled = true;
                    TimePickerComboBox.SelectedItem = AddTime(time);
                }
            }

            else
            {
                // Mark the event as handled so the framework doesn't update the selected item.
                args.Handled = true;

                var correctString = "EditingClassScheduleItemView_Invalid" + (this is TextBasedEndTimePicker ? "End" : "Start") + "Time";
                var correctTitle = PortableLocalizedResources.GetString(correctString + ".Title");
                var correctContent = PortableLocalizedResources.GetString(correctString + ".Content");
                new PortableMessageDialog(correctContent, correctTitle).Show();
            }
        }

        private TimeEntry AddTime(TimeSpan time)
        {
            var newEntry = CreateTimeEntry(time);

            for (int i = 0; i < _timeEntries.Count; i++)
            {
                if (time < _timeEntries[i].Time)
                {
                    _timeEntries.Insert(i, newEntry);
                    return newEntry;
                }
            }

            _timeEntries.Add(newEntry);
            return newEntry;
        }

        protected virtual TimeEntry CreateTimeEntry(TimeSpan time)
        {
            return new TimeEntry(time);
        }

        protected class TimeEntry : BindableBase
        {
            public TimeSpan Time { get; private set; }

            public TimeEntry(TimeSpan time)
            {
                Time = time;
            }

            public virtual string DisplayText
            {
                get => DateTimeFormatterExtension.Current.FormatAsShortTime(DateTime.Today.Add(Time));
            }

            public override bool Equals(object obj)
            {
                if (obj is TimeEntry other)
                {
                    return Time == other.Time;
                }

                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return Time.GetHashCode();
            }

            public override string ToString()
            {
                return DisplayText;
            }
        }
    }

    public class TextBasedEndTimePicker : TextBasedTimePicker
    {
        /// <summary>
        /// If this is an end time control, you can use this property so it'll dynamically eliminate values based on start time.
        /// </summary>
        public TimeSpan StartTime
        {
            get { return (TimeSpan)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register(nameof(StartTime), typeof(TimeSpan), typeof(TextBasedTimePicker), new PropertyMetadata(new TimeSpan(9, 0, 0), OnStartTimeChanged));

        private static void OnStartTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as TextBasedEndTimePicker).OnStartTimeChanged(e);
        }

        private void OnStartTimeChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateItems();

            // Update their end times
            foreach (var entry in _timeEntries.OfType<EndTimeEntry>())
            {
                entry.NotifyDisplayTextChanged();
            }
        }

        protected override TimeSpan GetStartTime()
        {
            return StartTime.Add(TimeSpan.FromMinutes(30));
        }

        protected override TimeEntry CreateTimeEntry(TimeSpan time)
        {
            return new EndTimeEntry(this, time);
        }

        protected override bool ParseText(string text, out TimeSpan time)
        {
            if (CustomTimePickerHelpers.ParseComboBoxItem(new DateTime(StartTime.Ticks), text, out time))
            {
                // Time must be greater than start time
                return time > StartTime;
            }

            time = default;
            return false;
        }

        private class EndTimeEntry : TimeEntry
        {
            private TextBasedEndTimePicker _endTimePickerControl;

            public EndTimeEntry(TextBasedEndTimePicker endTimePickerControl, TimeSpan time) : base(time)
            {
                _endTimePickerControl = endTimePickerControl;
            }

            public override string DisplayText => base.DisplayText + CustomTimePickerHelpers.GenerateTimeOffsetText(Time - _endTimePickerControl.StartTime);

            public void NotifyDisplayTextChanged()
            {
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }
}