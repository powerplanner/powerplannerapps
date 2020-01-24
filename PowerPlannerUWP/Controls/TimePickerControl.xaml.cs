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
    public sealed partial class TimePickerControl : UserControl
    {
        public static DependencyProperty HeaderProperty = ComboBox.HeaderProperty;
        public static string GetHeader(DependencyObject obj) => ((TimePickerControl)obj).Header;
        public static string SetHeader(DependencyObject obj, string val) => ((TimePickerControl)obj).Header = val;

        public static DependencyProperty IsEndProperty = DependencyProperty.Register("IsEnd", typeof(bool), typeof(TimePickerControl), new PropertyMetadata(false, TimeChanged));
        public static bool GetIsEnd(DependencyObject obj) => (bool)obj.GetValue(IsEndProperty);
        public static void SetIsEnd(DependencyObject obj, bool val) => obj.SetValue(IsEndProperty, val);

        // If this is an end control, we can optionally use this to work out an offset against the StartTime.

        public static DependencyProperty StartTimeProperty = DependencyProperty.Register("StartTime", typeof(TimeSpan), typeof(TimePickerControl), new PropertyMetadata(new TimeSpan(9, 0, 0), TimeChanged));
        public static TimeSpan GetStartTime(DependencyObject obj) => (TimeSpan)obj.GetValue(StartTimeProperty);
        public static void SetStartTime(DependencyObject obj, TimeSpan val) => obj.SetValue(StartTimeProperty, val);

        // The time this control shows.

        public static DependencyProperty MainTimeProperty = DependencyProperty.Register("MainTime", typeof(TimeSpan), typeof(TimePickerControl), new PropertyMetadata(new TimeSpan(9, 0, 0), TimeChanged));
        public static TimeSpan GetMainTime(DependencyObject obj) => (TimeSpan)obj.GetValue(MainTimeProperty);
        public static void SetMainTime(DependencyObject obj, TimeSpan val) => obj.SetValue(MainTimeProperty, val);

        bool _timeChanged;

        public string Header
        {
            get => (string)TimePickerComboBox.GetValue(ComboBox.HeaderProperty);
            set => TimePickerComboBox.SetValue(ComboBox.HeaderProperty, value);
        }

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

        private static void TimeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var timePicker = ((TimePickerControl)sender);
            timePicker.UpdateTime();
        }

        private void UpdateTime()
        {
            var mainTimeTicks = GetMainTime(this).Ticks;
            _timeChanged = true;
            UpdateItems();

            if (GetIsEnd(this))
                Selected = DateTimeFormatterExtension.Current.FormatAsShortTime(new DateTime(mainTimeTicks)) + CustomTimePickerHelpers.GenerateTimeOffsetText(new TimeSpan(mainTimeTicks - GetStartTime(this).Ticks));
            else
                Selected = DateTimeFormatterExtension.Current.FormatAsShortTime(new DateTime(mainTimeTicks));
        }

        private void TimePickerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If we've changed this through the "TimeChanged" method then don't do anything.
            if (_timeChanged)
            {
                _timeChanged = false;
                return;
            }

            var isEnd = GetIsEnd(this);

            // Attempt to parse the value that was just set into the ComboBox.
            if (!CustomTimePickerHelpers.ParseComboBoxItem(isEnd ? new DateTime(GetStartTime(this).Ticks) : DateTime.MinValue, (string)TimePickerComboBox.SelectedItem, out var t))
            {
                var correctString = "EditingClassScheduleItemView_Invalid" + (isEnd ? "End" : "Start") + "Time";
                var correctTitle = PowerPlannerResources.GetString(correctString + ".Title");
                var correctContent = PowerPlannerResources.GetString(correctString + ".Content");
                new PortableMessageDialog(correctContent, correctTitle).Show();
                return;
            }

            SetMainTime(this, t);
        }

        private void UpdateItems()
        {
            ObservableCollection<string> items;

            // Generate the new items.
            if (GetIsEnd(this))
                items = CustomTimePickerHelpers.GenerateTimesWithOffset(new DateTime(GetStartTime(this).Ticks), new DateTime(GetMainTime(this).Ticks), CustomTimePickerHelpers.CUSTOM_TIME_PICKER_DEFAULT_INTERVAL);
            else
                items = CustomTimePickerHelpers.GenerateTimes(DateTime.MinValue, new DateTime(GetMainTime(this).Ticks), CustomTimePickerHelpers.CUSTOM_TIME_PICKER_DEFAULT_INTERVAL);

            // Put them into the ComboBox.
            TimePickerComboBox.Items.Clear();
            for (int i = 0; i < items.Count; i++)
                TimePickerComboBox.Items.Add(items[i]);
        }
    }
}