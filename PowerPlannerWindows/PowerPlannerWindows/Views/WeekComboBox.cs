using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PowerPlannerUWP.Views
{
    public class WeekComboBox : ComboBox
    {
        public WeekComboBox()
        {
            base.ItemsSource = new string[]
            {
                LocalizedResources.GetString("String_BothWeeks"),
                LocalizedResources.Common.GetStringWeekA(),
                LocalizedResources.Common.GetStringWeekB()
            };

            base.SelectedIndex = 0;
            base.SelectionChanged += WeekComboBox_SelectionChanged;
        }

        private void WeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (base.SelectedIndex)
            {
                case 0:
                    SelectedWeek = Schedule.Week.BothWeeks;
                    break;

                case 1:
                    SelectedWeek = Schedule.Week.WeekOne;
                    break;

                default:
                    SelectedWeek = Schedule.Week.WeekTwo;
                    break;
            }
        }

        public Schedule.Week SelectedWeek
        {
            get { return (Schedule.Week)GetValue(SelectedWeekProperty); }
            set { SetValue(SelectedWeekProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedWeek.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedWeekProperty =
            DependencyProperty.Register("SelectedWeek", typeof(Schedule.Week), typeof(WeekComboBox), new PropertyMetadata(Schedule.Week.BothWeeks, OnSelectedWeekChanged));

        private static void OnSelectedWeekChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as WeekComboBox).OnSelectedWeekChanged(e);
        }

        private void OnSelectedWeekChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (SelectedWeek)
            {
                case Schedule.Week.BothWeeks:
                    base.SelectedIndex = 0;
                    break;

                case Schedule.Week.WeekOne:
                    base.SelectedIndex = 1;
                    break;

                default:
                    base.SelectedIndex = 2;
                    break;
            }
        }
    }
}
