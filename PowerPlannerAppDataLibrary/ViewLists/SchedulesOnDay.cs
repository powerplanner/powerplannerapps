using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewLists
{
    public class SchedulesOnDay : MyObservableList<ViewItemSchedule>
    {
        private class ScheduleFilter : IFilter<ViewItemSchedule>
        {
            private DateTime _date;
            private Schedule.Week _week;

            public ScheduleFilter(DateTime date, Schedule.Week week)
            {
                _date = date.Date;
                _week = week;
            }

            public bool ShouldInsert(ViewItemSchedule itemToBeInserted)
            {
                return itemToBeInserted.Class.IsActiveOnDate(_date) && itemToBeInserted.DayOfWeek == _date.DayOfWeek && (itemToBeInserted.ScheduleWeek == _week || itemToBeInserted.ScheduleWeek == Schedule.Week.BothWeeks);
            }
        }

        public bool TrackChanges { get; private set; }

        private PropertyChangedEventHandler _classPropertyChangedEventHandler;

        private Schedule.Week Week { get; set; }
        private DateTime Date { get; set; }
        private IEnumerable<ViewItemClass> Classes { get; set; }

        private SchedulesOnDay(IEnumerable<ViewItemClass> classes, DateTime date, Schedule.Week week, bool trackChanges = false)
        {
            TrackChanges = trackChanges;
            Week = week;
            Date = date;
            _classPropertyChangedEventHandler = new WeakEventHandler<PropertyChangedEventArgs>(Class_PropertyChanged).Handler;
            base.Filter = new ScheduleFilter(date, week);

            if (trackChanges && classes is INotifyCollectionChanged)
            {
                (classes as INotifyCollectionChanged).CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(Classes_CollectionChanged).Handler;
                Classes = classes;
            }

            foreach (var c in classes)
            {
                AddClass(c);
            }
        }

        private SchedulesOnDay(SchedulesOnDay other)
        {
            base.AddRange(other);
        }

        private static List<WeakReference<SchedulesOnDay>> _cached = new List<WeakReference<SchedulesOnDay>>();

        public static void ClearCached()
        {
            _cached.Clear();
        }

        public static SchedulesOnDay Get(IEnumerable<ViewItemClass> classes, DateTime date, Schedule.Week week, bool trackChanges = false)
        {
            SchedulesOnDay answer;
            for (int i = 0; i < _cached.Count; i++)
            {
                if (_cached[i].TryGetTarget(out answer))
                {
                    if (answer.Date == date.Date && answer.Classes == classes)
                    {
                        if (trackChanges)
                        {
                            return answer;
                        }
                        else
                        {
                            return new SchedulesOnDay(answer);
                        }
                    }
                }
                else
                {
                    _cached.RemoveAt(i);
                    i--;
                }
            }

            answer = new SchedulesOnDay(classes, date, week, trackChanges);
            if (trackChanges)
            {
                _cached.Add(new WeakReference<SchedulesOnDay>(answer));
            }
            return answer;
        }

        private void Classes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var c in e.NewItems.OfType<ViewItemClass>())
                    {
                        AddClass(c);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var c in e.OldItems.OfType<ViewItemClass>())
                    {
                        RemoveClass(c);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (var c in e.OldItems.OfType<ViewItemClass>())
                    {
                        RemoveClass(c);
                    }
                    foreach (var c in e.NewItems.OfType<ViewItemClass>())
                    {
                        AddClass(c);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    // Nothing
                    break;

                case NotifyCollectionChangedAction.Reset:
                    if (TrackChanges)
                    {
                        foreach (var c in _addedClasses)
                        {
                            StopObservingCollection(c.Schedules);
                        }
                        _addedClasses.Clear();
                    }
                    this.Clear();
                    foreach (var c in (sender as IEnumerable<ViewItemClass>))
                    {
                        AddClass(c);
                    }
                    break;
            }
        }

        private List<ViewItemClass> _addedClasses = new List<ViewItemClass>();
        private void AddClass(ViewItemClass c)
        {
            if (TrackChanges)
            {
                _addedClasses.Add(c);
                c.PropertyChanged += _classPropertyChangedEventHandler;
            }
            base.InsertSorted(c.Schedules, trackChanges: TrackChanges);
        }

        private void Class_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewItemClass.StartDate):
                case nameof(ViewItemClass.EndDate):
                    RemoveClass((sender as ViewItemClass));
                    AddClass((sender as ViewItemClass));
                    break;
            }
        }

        private void RemoveClass(ViewItemClass c)
        {
            if (TrackChanges)
            {
                _addedClasses.RemoveWhere(i => i == c);
                base.StopObservingCollection(c.Schedules);
                c.PropertyChanged -= _classPropertyChangedEventHandler;
            }
            IListExtensions.RemoveWhere(this, i => i.Class == c);
        }
    }
}
