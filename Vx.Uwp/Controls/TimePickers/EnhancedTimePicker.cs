using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Vx.Uwp.Controls.TimePickers
{
    public class EnhancedTimePicker : UserControl
    {
        public event EventHandler<TimeSpan> SelectedTimeChanged;

        public EnhancedTimePicker()
        {
            if (TextBasedTimePicker.IsSupported)
            {
                var picker = CreateTextBasedTimePicker();

                picker.Header = Header;
                picker.SelectedTime = SelectedTime;

                picker.SelectedTimeChanged += Picker_SelectedTimeChanged;

                Content = picker;
            }

            else
            {
                var picker = CreateTimePicker();
                picker.HorizontalAlignment = HorizontalAlignment.Stretch;

                Content = picker;
            }
        }

        private void Picker_SelectedTimeChanged(object sender, TimeSpan e)
        {
            SelectedTime = e;
        }

        protected virtual TextBasedTimePicker CreateTextBasedTimePicker()
        {
            return new TextBasedTimePicker();
        }

        protected virtual FrameworkElement CreateTimePicker()
        {
            var picker = new TimePicker()
            {
                Header = Header,
                Time = SelectedTime
            };

            picker.TimeChanged += Picker_TimeChanged;

            return picker;
        }

        private void Picker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            SelectedTime = e.NewTime;
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(EnhancedTimePicker), new PropertyMetadata("", OnHeaderChanged));

        private static void OnHeaderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as EnhancedTimePicker).OnHeaderChanged(e);
        }

        private void OnHeaderChanged(DependencyPropertyChangedEventArgs e)
        {
            if (Content is TextBasedTimePicker textPicker)
            {
                textPicker.Header = Header;
            }
            else if (Content is TimePicker timePicker)
            {
                timePicker.Header = Header;
            }
            else if (Content is EndTimePicker endTimePicker)
            {
                endTimePicker.Header = Header;
            }
        }

        /// <summary>
        /// The selected time.
        /// </summary>
        public TimeSpan SelectedTime
        {
            get { return (TimeSpan)GetValue(SelectedTimeProperty); }
            set { SetValue(SelectedTimeProperty, value); }
        }

        public static readonly DependencyProperty SelectedTimeProperty =
            DependencyProperty.Register(nameof(SelectedTime), typeof(TimeSpan), typeof(EnhancedTimePicker), new PropertyMetadata(new TimeSpan(9, 0, 0), OnSelectedTimeChanged));

        private static void OnSelectedTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as EnhancedTimePicker).OnSelectedTimeChanged(e);
        }

        private void OnSelectedTimeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (Content is TextBasedTimePicker textPicker)
            {
                textPicker.SelectedTime = SelectedTime;
            }
            else if (Content is TimePicker timePicker)
            {
                timePicker.Time = SelectedTime;
            }
            else if (Content is EndTimePicker endTimePicker)
            {
                endTimePicker.SelectedTime = SelectedTime;
            }

            SelectedTimeChanged?.Invoke(this, (TimeSpan)e.NewValue);
        }
    }

    public class EnhancedEndTimePicker : EnhancedTimePicker
    {
        protected override TextBasedTimePicker CreateTextBasedTimePicker()
        {
            var picker = new TextBasedEndTimePicker();

            picker.StartTime = StartTime;

            return picker;
        }

        protected override FrameworkElement CreateTimePicker()
        {
            var picker = new EndTimePicker()
            {
                Header = Header,
                StartTime = StartTime,
                SelectedTime = SelectedTime
            };

            picker.SelectedTimeChanged += Picker_SelectedTimeChanged;

            return picker;
        }

        private void Picker_SelectedTimeChanged(object sender, TimeSpan e)
        {
            SelectedTime = e;
        }

        public TimeSpan StartTime
        {
            get { return (TimeSpan)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register(nameof(StartTime), typeof(TimeSpan), typeof(EnhancedEndTimePicker), new PropertyMetadata(new TimeSpan(9, 0, 0), OnStartTimeChanged));

        private static void OnStartTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as EnhancedEndTimePicker).OnStartTimeChanged(e);
        }

        private void OnStartTimeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (Content is TextBasedEndTimePicker textPicker)
            {
                textPicker.StartTime = StartTime;
            }
            else if (Content is EndTimePicker endTimePicker)
            {
                endTimePicker.StartTime = StartTime;
            }
        }
    }
}
