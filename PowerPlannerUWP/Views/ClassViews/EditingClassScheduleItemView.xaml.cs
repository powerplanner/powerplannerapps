using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.ClassViews
{
    public sealed partial class EditingClassScheduleItemView : UserControl
    {
        public class ScheduleData
        {
            public DateTime StartTime, EndTime;

            public string Room;

            public DayOfWeek Day;

            public Schedule.Week Week;
        }

        public event EventHandler OnDelete;

        public EditingClassScheduleItemView()
        {
            this.InitializeComponent();

            checkBoxMonday.Content = DateTools.ToLocalizedString(DayOfWeek.Monday);
            checkBoxTuesday.Content = DateTools.ToLocalizedString(DayOfWeek.Tuesday);
            checkBoxWednesday.Content = DateTools.ToLocalizedString(DayOfWeek.Wednesday);
            checkBoxThursday.Content = DateTools.ToLocalizedString(DayOfWeek.Thursday);
            checkBoxFriday.Content = DateTools.ToLocalizedString(DayOfWeek.Friday);
            checkBoxSaturday.Content = DateTools.ToLocalizedString(DayOfWeek.Saturday);
            checkBoxSunday.Content = DateTools.ToLocalizedString(DayOfWeek.Sunday);
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (OnDelete != null)
                OnDelete(this, null);
        }

        public ScheduleCreator ScheduleCreator
        {
            set
            {
                timePickerStart.Time = value.StartTime;
                timePickerEnd.Time = value.EndTime;

                tbRoom.Text = value.Room;

                selectedDays = value.DayOfWeeks;

                weekComboBox.SelectedWeek = value.Week;
            }
        }

        private IEnumerable<DayOfWeek> selectedDays
        {
            get
            {
                List<DayOfWeek> answer = new List<DayOfWeek>();

                if (checkBoxMonday.IsChecked.Value)
                    answer.Add(DayOfWeek.Monday);

                if (checkBoxTuesday.IsChecked.Value)
                    answer.Add(DayOfWeek.Tuesday);

                if (checkBoxWednesday.IsChecked.Value)
                    answer.Add(DayOfWeek.Wednesday);

                if (checkBoxThursday.IsChecked.Value)
                    answer.Add(DayOfWeek.Thursday);

                if (checkBoxFriday.IsChecked.Value)
                    answer.Add(DayOfWeek.Friday);

                if (checkBoxSaturday.IsChecked.Value)
                    answer.Add(DayOfWeek.Saturday);

                if (checkBoxSunday.IsChecked.Value)
                    answer.Add(DayOfWeek.Sunday);

                return answer;
            }

            set
            {
                checkBoxMonday.IsChecked = value.Contains(DayOfWeek.Monday);
                checkBoxTuesday.IsChecked = value.Contains(DayOfWeek.Tuesday);
                checkBoxWednesday.IsChecked = value.Contains(DayOfWeek.Wednesday);
                checkBoxThursday.IsChecked = value.Contains(DayOfWeek.Thursday);
                checkBoxFriday.IsChecked = value.Contains(DayOfWeek.Friday);
                checkBoxSaturday.IsChecked = value.Contains(DayOfWeek.Saturday);
                checkBoxSunday.IsChecked = value.Contains(DayOfWeek.Sunday);
            }
        }

        public List<ScheduleData> GetSchedules()
        {
            var days = selectedDays;

            List<ScheduleData> answer = new List<ScheduleData>();

            foreach (DayOfWeek d in days)
                answer.Add(new ScheduleData()
                {
                    StartTime = asUtc(timePickerStart.Time),
                    EndTime = asUtc(timePickerEnd.Time),
                    Room = tbRoom.Text.Trim(),
                    Day = d,
                    Week = weekComboBox.SelectedWeek
                });

            return answer;
        }

        private DateTime asUtc(TimeSpan timeSpan)
        {
            return DateTime.SpecifyKind(DateTime.Today.Add(timeSpan), DateTimeKind.Utc);
        }
    }
}
