using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewLists
{
    public class HolidaysOnDay : MyObservableList<ViewItemHoliday>
    {
        private class DayFilter : IFilter<ViewItemHoliday>
        {
            public DateTime Date { get; private set; }

            public DayFilter(DateTime date)
            {
                Date = date.Date;
            }

            public bool ShouldInsert(ViewItemHoliday itemToBeInserted)
            {
                return Date <= itemToBeInserted.EndTime.Date && Date >= itemToBeInserted.Date.Date;
            }
        }

        private HolidaysOnDay(IReadOnlyList<ViewItemHoliday> holidaysList, DateTime date)
        {
            base.Filter = new DayFilter(date);

            base.InsertSorted(holidaysList);
        }

        public static HolidaysOnDay Create(MyObservableList<BaseViewItemMegaItem> mainList, DateTime date)
        {
            return new HolidaysOnDay(mainList.OfTypeObservable<ViewItemHoliday>(), date);
        }
    }
}
