﻿using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerUWP.Controls.TimePickers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Controls
{
    public class EnhancedTimePicker : UserControl
    {
        public EnhancedTimePicker()
        {
            if (TextBasedTimePicker.IsSupported && AbTestHelper.Tests.NewTimePicker)
            {
                var picker = CreateTextBasedTimePicker();

                picker.SetBinding(TextBasedTimePicker.HeaderProperty, new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(nameof(Header))
                });

                picker.SetBinding(TextBasedTimePicker.SelectedTimeProperty, new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(nameof(SelectedTime)),
                    Mode = BindingMode.TwoWay
                });

                Content = picker;
            }

            else
            {
                var picker = CreateTimePicker();
                picker.HorizontalAlignment = HorizontalAlignment.Stretch;

                Content = picker;
            }
        }

        protected virtual TextBasedTimePicker CreateTextBasedTimePicker()
        {
            return new TextBasedTimePicker();
        }

        protected virtual FrameworkElement CreateTimePicker()
        {
            var picker = new TimePicker();

            picker.SetBinding(TimePicker.HeaderProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(nameof(Header))
            });

            picker.SetBinding(TimePicker.TimeProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(nameof(SelectedTime)),
                Mode = BindingMode.TwoWay
            });

            return picker;
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(EnhancedTimePicker), new PropertyMetadata(""));

        /// <summary>
        /// The selected time.
        /// </summary>
        public TimeSpan SelectedTime
        {
            get { return (TimeSpan)GetValue(SelectedTimeProperty); }
            set { SetValue(SelectedTimeProperty, value); }
        }

        public static readonly DependencyProperty SelectedTimeProperty =
            DependencyProperty.Register(nameof(SelectedTime), typeof(TimeSpan), typeof(EnhancedTimePicker), new PropertyMetadata(new TimeSpan(9, 0, 0)));
    }

    public class EnhancedEndTimePicker : EnhancedTimePicker
    {
        protected override TextBasedTimePicker CreateTextBasedTimePicker()
        {
            var picker = new TextBasedEndTimePicker();

            picker.SetBinding(TextBasedEndTimePicker.StartTimeProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(nameof(StartTime))
            });

            return picker;
        }

        protected override FrameworkElement CreateTimePicker()
        {
            var picker = new EndTimePicker();

            picker.SetBinding(EndTimePicker.HeaderProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(nameof(Header))
            });

            picker.SetBinding(EndTimePicker.StartTimeProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(nameof(StartTime))
            });

            picker.SetBinding(EndTimePicker.SelectedTimeProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath(nameof(SelectedTime)),
                Mode = BindingMode.TwoWay
            });

            return picker;
        }

        public TimeSpan StartTime
        {
            get { return (TimeSpan)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register(nameof(StartTime), typeof(TimeSpan), typeof(EnhancedEndTimePicker), new PropertyMetadata(new TimeSpan(9, 0, 0)));
    }
}
