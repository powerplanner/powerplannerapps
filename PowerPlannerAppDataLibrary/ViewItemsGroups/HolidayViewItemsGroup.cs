using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewItemsGroups
{
    /// <summary>
    /// Currently does NOT support tracking changes
    /// </summary>
    public class HolidayViewItemsGroup : BaseAccountViewItemsGroup
    {
        public ViewItemSemester Semester { get; private set; }

        /// <summary>
        /// List is NOT sorted
        /// </summary>
        public MyObservableList<ViewItemHoliday> Holidays { get; private set; }

        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        private HolidayViewItemsGroup(Guid localAccountId, ViewItemSemester semester, DateTime start, DateTime end) : base(localAccountId, false)
        {
            Semester = semester;
            StartDate = start;
            EndDate = end;
        }

        public static Task<HolidayViewItemsGroup> LoadAsync(Guid localAccountId, ViewItemSemester semester, DateTime start, DateTime end, bool trackChanges = true)
        {
            if (semester == null)
                throw new ArgumentNullException("semester");

            if (trackChanges)
                throw new NotImplementedException("Tracking changes currently isn't implemented");

            start = start.Date;
            end = end.Date;

            return CreateLoadTask(localAccountId, semester, start, end, trackChanges);
        }

        private static async Task<HolidayViewItemsGroup> CreateLoadTask(Guid localAccountId, ViewItemSemester semester, DateTime start, DateTime end, bool trackChanges)
        {
            var answer = new HolidayViewItemsGroup(localAccountId, semester, start, end);

            await Task.Run(answer.LoadBlocking);
            return answer;
        }

        private async Task LoadBlocking()
        {
            var dataStore = await GetDataStore();

            Guid semesterId = Semester.Identifier;
            DateTime startAsUtc = DateTime.SpecifyKind(StartDate, DateTimeKind.Utc);
            DateTime endAsUtc = DateTime.SpecifyKind(EndDate, DateTimeKind.Utc);
            DataItemMegaItem[] dataItemHolidays;

            using (await Locks.LockDataForReadAsync("HolidayViewItemsGroup.LoadBlocking"))
            {
                dataItemHolidays = dataStore.TableMegaItems.Where(i =>
                    i.MegaItemType == PowerPlannerSending.MegaItemType.Holiday
                    && i.UpperIdentifier == semesterId
                    && ((i.Date <= startAsUtc && i.EndTime >= startAsUtc)
                        || (i.Date >= startAsUtc && i.Date <= endAsUtc))).ToArray();
            }

            Holidays = new MyObservableList<ViewItemHoliday>(dataItemHolidays.Select(i => new ViewItemHoliday(i)));
        }
    }
}
