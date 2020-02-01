using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class
{
    public class ClassTimesViewModel : BaseClassContentViewModel
    {
        public ClassTimesViewModel(ClassViewModel parent) : base(parent)
        {
            TimesGroupedByDay = GenerateGroups(parent.ViewItemsGroupClass.Class.Schedules);
        }

        public GroupedDay[] TimesGroupedByDay { get; private set; }

        public GroupedDay[] GenerateGroups(MyObservableList<ViewItemSchedule> schedules)
        {
            try
            {
                if (schedules == null)
                    return null;

                return new GroupedDay[]
                {
                    new GroupedDay(schedules, DayOfWeek.Monday),
                    new GroupedDay(schedules, DayOfWeek.Tuesday),
                    new GroupedDay(schedules, DayOfWeek.Wednesday),
                    new GroupedDay(schedules, DayOfWeek.Thursday),
                    new GroupedDay(schedules, DayOfWeek.Friday),
                    new GroupedDay(schedules, DayOfWeek.Saturday),
                    new GroupedDay(schedules, DayOfWeek.Sunday)
                };
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return null;
            }
        }

        public class GroupedDay : BindableBase
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

            private bool _isVisible;
            public bool IsVisible
            {
                get { return _isVisible; }
                private set { SetProperty(ref _isVisible, value, "Visibility"); }
            }

            public DayOfWeek DayOfWeek { get; private set; }

            private void ResetVisibility()
            {
                IsVisible = Times.Count > 0 ? true : false;
            }
        }
    }
}
