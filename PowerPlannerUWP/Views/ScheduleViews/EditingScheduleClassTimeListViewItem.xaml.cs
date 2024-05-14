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
            // Editing view, so we use School Time
            TextBlockName.Text = LocalizedResources.Common.GetStringTimeToTime(timeFormatter.Format(first.StartTimeInSchoolTime), timeFormatter.Format(first.EndTimeInSchoolTime));
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
