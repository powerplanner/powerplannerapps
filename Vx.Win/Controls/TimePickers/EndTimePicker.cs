using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace Vx.Uwp.Controls.TimePickers
{
    public class EndTimePicker : UserControl
    {
        private TimePicker _timePicker;

        public EndTimePicker()
        {
            _timePicker = new TimePicker()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            _timePicker.TimeChanged += _timePicker_TimeChanged;

            Content = _timePicker;
        }

        private void _timePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (SelectedTime != _timePicker.Time)
            {
                SelectedTime = _timePicker.Time;
            }
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(EndTimePicker), new PropertyMetadata("", OnHeaderChanged));

        private static void OnHeaderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // For some reason binding isn't working, so using the changed events
            (sender as EndTimePicker)._timePicker.Header = e.NewValue as string;
        }

        public TimeSpan StartTime
        {
            get { return (TimeSpan)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register(nameof(StartTime), typeof(TimeSpan), typeof(EndTimePicker), new PropertyMetadata(new TimeSpan(9, 0, 0), OnStartTimeChanged));

        /// <summary>
        /// The selected time.
        /// </summary>
        public TimeSpan SelectedTime
        {
            get { return (TimeSpan)GetValue(SelectedTimeProperty); }
            set { SetValue(SelectedTimeProperty, value); }
        }

        public static readonly DependencyProperty SelectedTimeProperty =
            DependencyProperty.Register(nameof(SelectedTime), typeof(TimeSpan), typeof(EndTimePicker), new PropertyMetadata(new TimeSpan(9, 50, 0), OnSelectedTimeChanged));

        private static void OnSelectedTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as EndTimePicker).OnSelectedTimeChanged();
        }

        private void OnSelectedTimeChanged()
        {
            if (_timePicker.Time != SelectedTime)
            {
                _timePicker.Time = SelectedTime;
            }
        }

        private static void OnStartTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as EndTimePicker).OnStartTimeChanged(e);
        }

        private void OnStartTimeChanged(DependencyPropertyChangedEventArgs e)
        {
            TimeSpan changeAmount = (TimeSpan)e.NewValue - (TimeSpan)e.OldValue;

            TimeSpan newSelectedTime = SelectedTime + changeAmount;
            if (newSelectedTime.Days > 0)
            {
                newSelectedTime = new TimeSpan(23, 59, 0);
            }
            else if (newSelectedTime.Ticks <= 0)
            {
                newSelectedTime = new TimeSpan(0, 1, 0);
            }

            SelectedTime = newSelectedTime;
        }
    }
}
