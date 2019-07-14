using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerUWPLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsUniversal;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Converters
{
    public class SchedulesToGroupedDaysConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                MyObservableList<ViewItemSchedule> allSchedules = value as MyObservableList<ViewItemSchedule>;

                if (allSchedules == null)
                    return value;

                return new GroupedDay[]
                {
                    new GroupedDay(allSchedules, DayOfWeek.Monday),
                    new GroupedDay(allSchedules, DayOfWeek.Tuesday),
                    new GroupedDay(allSchedules, DayOfWeek.Wednesday),
                    new GroupedDay(allSchedules, DayOfWeek.Thursday),
                    new GroupedDay(allSchedules, DayOfWeek.Friday),
                    new GroupedDay(allSchedules, DayOfWeek.Saturday),
                    new GroupedDay(allSchedules, DayOfWeek.Sunday)
                };
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private class GroupedDay : BindableBase
        {
            public GroupedDay(MyObservableList<ViewItemSchedule> allSchedules, DayOfWeek dayOfWeek)
            {
                DayOfWeek = dayOfWeek;

                Times = allSchedules.Sublist(i => i.DayOfWeek == DayOfWeek);
                Times.CollectionChanged += Schedules_CollectionChanged;

                ResetVisibility();
            }

            private void Schedules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                try
                {
                    ResetVisibility();
                }

                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            public MyObservableList<ViewItemSchedule> Times { get; private set; }

            private Visibility _visibility;
            public Visibility Visibility
            {
                get { return _visibility; }
                private set { SetProperty(ref _visibility, value, "Visibility"); }
            }

            public DayOfWeek DayOfWeek { get; private set; }

            private void ResetVisibility()
            {
                Visibility = Times.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
