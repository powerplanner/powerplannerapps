using PowerPlannerAppDataLibrary.DataLayer;
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
    public class TasksOrEventsOnDay : MyObservableList<BaseViewItemMegaItem>
    {
        public class DayFilter : IFilter<BaseViewItemMegaItem>
        {
            public DateTime Date { get; private set; }
            public DateTime Today { get; private set; }
            public bool ActiveOnly { get; private set; }
            public bool UseEffectiveDateEvenWhenItemsHaveTimes { get; private set; }

            public DayFilter(DateTime date, DateTime today, bool activeOnly, bool useEffectiveDateEvenWhenItemsHaveTimes)
            {
                Date = date.Date;
                Today = today.Date;
                ActiveOnly = activeOnly;
                UseEffectiveDateEvenWhenItemsHaveTimes = useEffectiveDateEvenWhenItemsHaveTimes;
            }

            public bool ShouldInsert(BaseViewItemMegaItem itemToBeInserted)
            {
                if (itemToBeInserted is ViewItemTaskOrEvent taskOrEvent)
                {
                    bool dateMatches;
                    if (UseEffectiveDateEvenWhenItemsHaveTimes || taskOrEvent.TimeOption == DataLayer.DataItems.DataItemMegaItem.TimeOptions.AllDay)
                    {
                        dateMatches = itemToBeInserted.EffectiveDateForDisplayInDateBasedGroups.Date == Date;
                    }
                    else
                    {
                        dateMatches = itemToBeInserted.Date.Date == Date;
                    }

                    if (dateMatches)
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
                else
                {
                    return false;
                }
            }

            public bool Equals(DateTime otherDate, DateTime otherToday, bool otherActiveOnly, bool otherUseEffectiveDateEvenWhenItemsHaveTimes)
            {
                if (Date != otherDate)
                {
                    return false;
                }

                if (UseEffectiveDateEvenWhenItemsHaveTimes != otherUseEffectiveDateEvenWhenItemsHaveTimes)
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

        public TimeZoneInfo SchoolTimeZone { get; private set; }

        public new DayFilter Filter
        {
            get => base.Filter as DayFilter;
            set => base.Filter = value;
        }

        private TasksOrEventsOnDay(MyObservableList<BaseViewItemMegaItem> mainList, DateTime date, DateTime today, bool activeOnly, bool useEffectiveDateEvenWhenItemsHaveTimes, TimeZoneInfo schoolTimeZone)
        {
            MainList = mainList;
            SchoolTimeZone = schoolTimeZone;

            base.Filter = new DayFilter(date, today, activeOnly, useEffectiveDateEvenWhenItemsHaveTimes);

            base.InsertSorted(mainList);
        }

        private static readonly List<WeakReference<TasksOrEventsOnDay>> _cached = new List<WeakReference<TasksOrEventsOnDay>>();

        public static TasksOrEventsOnDay Get(AccountDataItem account, MyObservableList<BaseViewItemMegaItem> mainList, DateTime date, DateTime? today = null, bool activeOnly = false, bool useEffectiveDateEvenWhenItemsHaveTimes = true)
        {
            if (today == null)
            {
                today = DateTime.Today;
            }

            TasksOrEventsOnDay answer;
            for (int i = 0; i < _cached.Count; i++)
            {
                if (_cached[i].TryGetTarget(out answer))
                {
                    if (answer.MainList == mainList
                        && object.Equals(answer.SchoolTimeZone, account.SchoolTimeZone)
                        && answer.Filter.Equals(date, today.Value, activeOnly, useEffectiveDateEvenWhenItemsHaveTimes))
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

            answer = new TasksOrEventsOnDay(mainList, date, today.Value, activeOnly, useEffectiveDateEvenWhenItemsHaveTimes, account.SchoolTimeZone);
            _cached.Add(new WeakReference<TasksOrEventsOnDay>(answer));
            return answer;
        }
    }
}
