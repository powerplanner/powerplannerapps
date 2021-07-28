using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization.DateTimeFormatting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.ScheduleViews
{
    public sealed partial class EditingScheduleClassTimeListViewItem : UserControl
    {
        public EditingScheduleClassTimeListViewItem()
        {
            this.InitializeComponent();
        }

        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            IEnumerable<ViewItemSchedule> schedules = DataContext as IEnumerable<ViewItemSchedule>;
            if (schedules == null || !schedules.Any())
                return;

            ViewItemSchedule first = schedules.First();

            var timeFormatter = new DateTimeFormatter("shorttime");

            TextBlockDayOfWeeks.Text = string.Join(", ", schedules.Select(i => i.DayOfWeek).Distinct().OrderBy(i => i).Select(i => DateTools.ToLocalizedString(i)));
            TextBlockName.Text = LocalizedResources.Common.GetStringTimeToTime(timeFormatter.Format(first.StartTime), timeFormatter.Format(first.EndTime));
            if (string.IsNullOrWhiteSpace(first.Room))
            {
                TextBlockRoom.Visibility = Visibility.Collapsed;
            }
            else
            {
                TextBlockRoom.Text = first.Room;
                TextBlockRoom.Visibility = Visibility.Visible;
            }

            if (first.ScheduleWeek == PowerPlannerSending.Schedule.Week.BothWeeks)
            {
                TextBlockWeek.Visibility = Visibility.Collapsed;
            }

            else
            {
                TextBlockWeek.Text = LocalizedResources.Common.GetLocalizedWeek(first.ScheduleWeek);
                TextBlockWeek.Visibility = Visibility.Visible;
            }
        }
    }
}
