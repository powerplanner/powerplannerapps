using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public class ScheduleTileDataHelper
    {
        public AccountDataItem Account { get; private set; }

        public int DaysIncludingToday { get; private set; }

        public DateTime Today { get; private set; }

        public bool HasSemester { get; private set; }

        public ScheduleViewItemsGroup ScheduleViewItemsGroup { get; private set; }

        public HolidayViewItemsGroup HolidayViewItemsGroup { get; private set; }

        private ScheduleTileDataHelper() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="today">As local time</param>
        /// <param name="daysIncludingToday"></param>
        /// <returns></returns>
        public static async Task<ScheduleTileDataHelper> LoadAsync(AccountDataItem account, DateTime today, int daysIncludingToday)
        {
            var answer = new ScheduleTileDataHelper()
            {
                Account = account,
                DaysIncludingToday = daysIncludingToday,
                Today = today.Date
            };

            var currSemesterId = account.CurrentSemesterId;
            if (currSemesterId != Guid.Empty)
            {
                try
                {
                    answer.ScheduleViewItemsGroup = await ScheduleViewItemsGroup.LoadAsync(account.LocalAccountId, currSemesterId, trackChanges: true, includeWeightCategories: false);
                    answer.HasSemester = true;
                }
                catch
                {
                    // If semester not found
                    return answer;
                }

                try
                {
                    answer.HolidayViewItemsGroup = await HolidayViewItemsGroup.LoadAsync(account.LocalAccountId, answer.ScheduleViewItemsGroup.Semester, today, today.AddDays(daysIncludingToday - 1), trackChanges: false);
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            // TODO: Genericize the logic between UWP and Android

            return answer;
        }

        public IEnumerable<ScheduleTileDayData> GetDataForAllDays()
        {
            DateTime[] dates = new DateTime[DaysIncludingToday];
            for (int i = 0; i < dates.Length; i++)
            {
                dates[i] = Today.AddDays(i);
            }

            return dates.Select(i => CreateDayData(i));
        }

        private ScheduleTileDayData CreateDayData(DateTime date)
        {
            var answer = new ScheduleTileDayData()
            {
                Date = date,
                IsToday = date == Today
            };

            if (!ScheduleViewItemsGroup.Semester.IsDateDuringThisSemester(date))
            {
                return answer;
            }

            if (HolidayViewItemsGroup != null && HolidayViewItemsGroup.Holidays.Any(h => h.IsOnDay(date)))
            {
                answer.Holidays = HolidayViewItemsGroup.Holidays.Where(h => h.IsOnDay(date)).OrderBy(h => h).ToArray();
            }

            else
            {
                answer.Schedules = SchedulesOnDay.Get(Account, ScheduleViewItemsGroup.Classes, date, Account.CurrentWeek, trackChanges: false).ToArray();
            }

            return answer;
        }

        public class ScheduleTileDayData
        {
            public DateTime Date { get; set; }

            public bool IsToday { get; set; }

            public ViewItemHoliday[] Holidays { get; set; } = new ViewItemHoliday[0];

            public ViewItemSchedule[] Schedules { get; set; } = new ViewItemSchedule[0];
        }
    }
}
