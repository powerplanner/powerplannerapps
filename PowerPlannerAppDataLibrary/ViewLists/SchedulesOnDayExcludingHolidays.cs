using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewLists
{
    /// <summary>
    /// Helper for getting schedules on a day while excluding days that have holidays.
    /// This is intended for use by notification/reminder code that should not fire on holidays.
    /// The UI uses <see cref="SchedulesOnDay"/> directly (and greys out classes on holidays).
    /// </summary>
    public class SchedulesOnDayExcludingHolidays
    {
        private readonly IReadOnlyList<ViewItemHoliday> _holidays;

        private SchedulesOnDayExcludingHolidays(IReadOnlyList<ViewItemHoliday> holidays)
        {
            _holidays = holidays;
        }

        /// <summary>
        /// Loads holiday data for the given date range so that <see cref="Get"/> can filter them out.
        /// </summary>
        public static async Task<SchedulesOnDayExcludingHolidays> LoadAsync(Guid localAccountId, ViewItemSemester semester, DateTime startDate, DateTime endDate)
        {
            var holidayGroup = await HolidayViewItemsGroup.LoadAsync(localAccountId, semester, startDate, endDate, trackChanges: false);
            return new SchedulesOnDayExcludingHolidays(holidayGroup.Holidays);
        }

        /// <summary>
        /// Returns true if the given date falls on a holiday.
        /// </summary>
        public bool HasHolidayOnDay(DateTime date)
        {
            var d = date.Date;
            return _holidays.Any(h => h.IsOnDay(d));
        }

        /// <summary>
        /// Gets the schedules on the given day, or an empty list if there is a holiday on that day.
        /// Drop-in replacement for <see cref="SchedulesOnDay.Get"/> in reminder/notification code.
        /// </summary>
        public SchedulesOnDay Get(AccountDataItem account, IEnumerable<ViewItemClass> classes, DateTime date, PowerPlannerSending.Schedule.Week week)
        {
            if (HasHolidayOnDay(date))
            {
                return SchedulesOnDay.CreateEmpty();
            }

            return SchedulesOnDay.Get(account, classes, date, week, trackChanges: false);
        }
    }
}
