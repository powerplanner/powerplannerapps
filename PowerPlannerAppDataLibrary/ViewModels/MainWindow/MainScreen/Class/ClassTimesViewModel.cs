using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx;
using Vx.Extensions;
using Vx.Views;
using static PowerPlannerAppDataLibrary.ViewLists.DayScheduleItemsArranger;

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

        protected override View Render()
        {
            var linLayout = new LinearLayout
            {
                Margin = new Thickness(VxPlatform.Current == Platform.Uwp ? 12 : Theme.Current.PageMargin)
            };

            foreach (var group in TimesGroupedByDay)
            {
                SubscribeToCollection(group.Times);

                if (group.IsVisible)
                {
                    linLayout.Children.Add(new TextBlock
                    {
                        Text = DateTools.ToLocalizedString(group.DayOfWeek)
                    }.TitleStyle());

                    foreach (var time in group.Times)
                    {
                        bool hasRoom = !string.IsNullOrWhiteSpace(time.Room);

                        linLayout.Children.Add(new TextBlock
                        {
                            Text = PowerPlannerResources.GetStringTimeToTime(DateTimeFormatterExtension.Current.FormatAsShortTime(time.StartTimeInSchoolTime), DateTimeFormatterExtension.Current.FormatAsShortTime(time.EndTimeInSchoolTime)),
                            Margin = new Thickness(0, 0, 0, hasRoom ? 0 : 12)
                        });

                        if (hasRoom)
                        {
                            linLayout.Children.Add(new HyperlinkTextBlock
                            {
                                Text = time.Room,
                                Margin = new Thickness(0, 0, 0, 12)
                            });
                        }
                    }
                }
            }

            if (linLayout.Children.Count == 0)
            {
                return new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin),
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = PowerPlannerResources.GetString("ClassPage_SchedulesNoTimesHeader.Text"),
                            TextAlignment = HorizontalAlignment.Center
                        }.TitleStyle(),

                        new TextBlock
                        {
                            Text = PowerPlannerResources.GetString("ClassPage_SchedulesNoTimesDetails.Text"),
                            TextAlignment = HorizontalAlignment.Center
                        }
                    }
                };
            }

            return new ScrollView(linLayout);
        }

        public class GroupedDay : BindableBase
        {
            public GroupedDay(MyObservableList<ViewItemSchedule> allSchedules, DayOfWeek dayOfWeek)
            {
                DayOfWeek = dayOfWeek;

                var date = DateTools.Next(dayOfWeek, DateTime.Today);

                Times = allSchedules.Sublist(i => i.OccursOnDate(date));
            }

            public MyObservableList<ViewItemSchedule> Times { get; private set; }

            public bool IsVisible => Times.Count > 0 ? true : false;

            public DayOfWeek DayOfWeek { get; private set; }
        }
    }
}
