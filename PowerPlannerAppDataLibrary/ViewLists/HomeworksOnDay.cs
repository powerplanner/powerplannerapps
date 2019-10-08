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
    public class HomeworksOnDay : MyObservableList<BaseViewItemMegaItem>
    {
        private class DayFilter : IFilter<BaseViewItemMegaItem>
        {
            public DateTime Date { get; private set; }

            public DayFilter(DateTime date)
            {
                Date = date.Date;
            }

            public bool ShouldInsert(BaseViewItemMegaItem itemToBeInserted)
            {
                return itemToBeInserted is ViewItemTaskOrEvent
                    && itemToBeInserted.Date.Date == Date;
            }
        }

        public DateTime Date { get; private set; }
        public MyObservableList<BaseViewItemMegaItem> MainList { get; private set; }

        private HomeworksOnDay(MyObservableList<BaseViewItemMegaItem> mainList, DateTime date)
        {
            Date = date.Date;
            MainList = mainList;

            base.Filter = new DayFilter(date);

            base.InsertSorted(mainList);
        }

        private static readonly List<WeakReference<HomeworksOnDay>> _cached = new List<WeakReference<HomeworksOnDay>>();

        public static HomeworksOnDay Get(MyObservableList<BaseViewItemMegaItem> mainList, DateTime date)
        {
            HomeworksOnDay answer;
            for (int i = 0; i < _cached.Count; i++)
            {
                if (_cached[i].TryGetTarget(out answer))
                {
                    if (answer.Date == date.Date && answer.MainList == mainList)
                    {
                        return answer;
                    }
                }
                else
                {
                    _cached.RemoveAt(i);
                    i--;
                }
            }

            answer = new HomeworksOnDay(mainList, date);
            _cached.Add(new WeakReference<HomeworksOnDay>(answer));
            return answer;
        }
    }
}
