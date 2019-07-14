using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerUWP.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization.DateTimeFormatting;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.ScheduleViews
{
    public sealed partial class MyScheduleItem : UserControl
    {
        public MyScheduleItem()
        {
            this.InitializeComponent();
        }

        public ViewItemSchedule Schedule
        {
            get { return (ViewItemSchedule)GetValue(ScheduleProperty); }
            set { SetValue(ScheduleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Schedule.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScheduleProperty =
            DependencyProperty.Register("Schedule", typeof(ViewItemSchedule), typeof(MyScheduleItem), new PropertyMetadata(null, OnScheduleChanged));

        private static void OnScheduleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as MyScheduleItem).OnScheduleChanged(e);
        }

        private void OnScheduleChanged(DependencyPropertyChangedEventArgs e)
        {
            if (Schedule == null)
                return;

            var timeFormatter = new DateTimeFormatter("shorttime");
            TextBlockTime.Text = string.Format(LocalizedResources.GetString("String_TimeToTime"), timeFormatter.Format(Schedule.StartTime), timeFormatter.Format(Schedule.EndTime));
        }

        public bool IsHighlighted
        {
            get { return (bool)GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHighlighted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register("IsHighlighted", typeof(bool), typeof(MyScheduleItem), new PropertyMetadata(false, OnIsHighlightedChanged));

        private static void OnIsHighlightedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as MyScheduleItem).OnIsHighlightedChanged(e);
        }

        private void OnIsHighlightedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (IsHighlighted)
                VisualStateManager.GoToState(this, "HighlightedState", true);
            else
                VisualStateManager.GoToState(this, "DefaultState", true);
        }
    }
}
