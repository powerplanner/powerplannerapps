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
        public class DayFilter : IFilter<BaseViewItemMegaItem>
        {
            public DateTime Date { get; private set; }
            public DateTime Today { get; private set; }
            public bool ActiveOnly { get; private set; }

            public DayFilter(DateTime date, DateTime today, bool activeOnly)
            {
                Date = date.Date;
                Today = today.Date;
                ActiveOnly = activeOnly;
            }

            public bool ShouldInsert(BaseViewItemMegaItem itemToBeInserted)
            {
                if ((itemToBeInserted is ViewItemTaskOrEvent)
                    && itemToBeInserted.Date.Date == Date)
                {
                    if (ActiveOnly)
                    {
                        if ((itemToBeInserted as ViewItemTaskOrEvent).IsActive(Today))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }

            public bool Equals(DateTime otherDate, DateTime otherToday, bool otherActiveOnly)
            {
                if (Date != otherDate)
                {
                    return false;
                }

                if (ActiveOnly)
                {
                    if (!otherActiveOnly)
                    {
                        return false;
                    }

                    if (Today != otherToday)
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    if (otherActiveOnly)
                    {
                        return false;
                    }

                    return true;
                }
            }
        }

        public MyObservableList<BaseViewItemMegaItem> MainList { get; private set; }

        public new DayFilter Filter
        {
            get => base.Filter as DayFilter;
            set => base.Filter = value;
        }

        private HomeworksOnDay(MyObservableList<BaseViewItemMegaItem> mainList, DateTime date, DateTime today, bool activeOnly)
        {
            MainList = mainList;

            base.Filter = new DayFilter(date, today, activeOnly);

            base.InsertSorted(mainList);
        }

        private static readonly List<WeakReference<HomeworksOnDay>> _cached = new List<WeakReference<HomeworksOnDay>>();

        public static HomeworksOnDay Get(MyObservableList<BaseViewItemMegaItem> mainList, DateTime date, DateTime? today = null, bool activeOnly = false)
        {
            if (today == null)
            {
                today = DateTime.Today;
            }

            HomeworksOnDay answer;
            for (int i = 0; i < _cached.Count; i++)
            {
                if (_cached[i].TryGetTarget(out answer))
                {
                    if (answer.MainList == mainList
                        && answer.Filter.Equals(date, today.Value, activeOnly))
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

            answer = new HomeworksOnDay(mainList, date, today.Value, activeOnly);
            _cached.Add(new WeakReference<HomeworksOnDay>(answer));
            return answer;
        }
    }
}
