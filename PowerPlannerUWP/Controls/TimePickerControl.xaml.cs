using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewModels.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Controls
{
    public partial class TimePickerControl : UserControl
    {
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(TimePickerControl), new PropertyMetadata(""));

        /// <summary>
        /// The selected time.
        /// </summary>
        public TimeSpan SelectedTime
        {
            get { return (TimeSpan)GetValue(SelectedTimeProperty); }
            set { SetValue(SelectedTimeProperty, value); }
        }

        public static readonly DependencyProperty SelectedTimeProperty =
            DependencyProperty.Register("SelectedTime", typeof(TimeSpan), typeof(TimePickerControl), new PropertyMetadata(new TimeSpan(9, 0, 0), OnValuesChanged));

        bool _timeChanged;

        public string Selected
        {
            get => (string)TimePickerComboBox.SelectedItem;
            set => TimePickerComboBox.SelectedItem = value;
        }

        public TimePickerControl()
        {
            InitializeComponent();
            UpdateTime();
        }

        protected static void OnValuesChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var timePicker = ((TimePickerControl)sender);
            timePicker.UpdateTime();
        }

        private void UpdateTime()
        {
            var selectedTime = SelectedTime;
            _timeChanged = true;
            UpdateItems();

            Selected = FormatTime(selectedTime);
        }

        protected virtual string FormatTime(TimeSpan time)
        {
            return DateTimeFormatterExtension.Current.FormatAsShortTime(new DateTime(time.Ticks));
        }

        private void TimePickerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If we've changed this through the "TimeChanged" method then don't do anything.
            if (_timeChanged)
            {
                _timeChanged = false;
                return;
            }

            // Attempt to parse the value that was just set into the ComboBox.
            if (!ParseText((string)TimePickerComboBox.SelectedItem, out var t))
            {
                var correctString = "EditingClassScheduleItemView_Invalid" + (this is EndTimePickerControl ? "End" : "Start") + "Time";
                var correctTitle = PowerPlannerResources.GetString(correctString + ".Title");
                var correctContent = PowerPlannerResources.GetString(correctString + ".Content");
                new PortableMessageDialog(correctContent, correctTitle).Show();
                return;
            }

            SelectedTime = t;
        }

        private void UpdateItems()
        {
            List<string> items = GenerateItems();

            TimePickerComboBox.ItemsSource = items;
        }

        protected virtual List<string> GenerateItems()
        {
            return CustomTimePickerHelpers.GenerateTimes(DateTime.MinValue, new DateTime(SelectedTime.Ticks), CustomTimePickerHelpers.CUSTOM_TIME_PICKER_DEFAULT_INTERVAL);
        }

        protected virtual bool ParseText(string text, out TimeSpan time)
        {
            return CustomTimePickerHelpers.ParseComboBoxItem(DateTime.MinValue, text, out time);
        }
    }

    public class EndTimePickerControl : TimePickerControl
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
            DependencyProperty.Register("StartTime", typeof(TimeSpan), typeof(TimePickerControl), new PropertyMetadata(null, OnValuesChanged));

        protected override List<string> GenerateItems()
        {
            return CustomTimePickerHelpers.GenerateTimesWithOffset(new DateTime(StartTime.Ticks), new DateTime(SelectedTime.Ticks), CustomTimePickerHelpers.CUSTOM_TIME_PICKER_DEFAULT_INTERVAL);
        }

        protected override bool ParseText(string text, out TimeSpan time)
        {
            return CustomTimePickerHelpers.ParseComboBoxItem(new DateTime(StartTime.Ticks), text, out time);
        }

        protected override string FormatTime(TimeSpan time)
        {
            return DateTimeFormatterExtension.Current.FormatAsShortTime(new DateTime(time.Ticks)) + CustomTimePickerHelpers.GenerateTimeOffsetText(time - StartTime);
        }
    }
}