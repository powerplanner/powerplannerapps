using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewItemsGroups
{
    public class CalendarViewItemsGroup : BaseAccountViewItemsGroup
    {
        public Guid SemesterId
        {
            get { return Semester.Identifier; }
        }

        public ViewItemSemester Semester { get; private set; }

        public MyObservableList<ViewItemClass> Classes
        {
            get { return Semester.Classes; }
        }

        public MyObservableList<BaseViewItemHomeworkExam> Items { get; private set; } = new MyObservableList<BaseViewItemHomeworkExam>();

        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        private CalendarViewItemsGroup(Guid localAccountId, ViewItemSemester semester) : base(localAccountId)
        {
            Semester = semester;
        }

        public static CalendarViewItemsGroup Load(Guid localAccountId, ViewItemSemester semester)
        {
            var answer = new CalendarViewItemsGroup(localAccountId, semester);
            // There's actually nothing to load async here, since classes/schedules are already loaded
            return answer;
        }

        private class DateRange
        {
            public DateTime Start { get; private set; }
            public DateTime End { get; private set; }

            public DateRange(DateTime start, DateTime end)
            {
                Start = start;
                End = end;
            }
        }

        private DateRange _pendingFilterRequest;
        private async Task ApplyFilterAsync(DateTime start, DateTime end)
        {
            // If the requested filter is already pending, do nothing
            if (_pendingFilterRequest != null && _pendingFilterRequest.Start == start && _pendingFilterRequest.End == end)
                return;

            // Otherwise create new pending request
            var newRequest = new DateRange(start, end);

            // And assign it
            _pendingFilterRequest = newRequest;

            DateTime prevStart = Start;
            DateTime prevEnd = End;

            await Task.Run(async delegate
            {
                await ApplyFilterBlocking(prevStart, prevEnd, newRequest);
            });
        }

        private async Task EnsureLoadedAsync(DateTime neededStart, DateTime neededEnd, DateTime loadUpToStart, DateTime loadUpToEnd)
        {
            if (Start > neededStart || End < neededEnd)
            {
                await ApplyFilterAsync(loadUpToStart, loadUpToEnd);
            }
        }

        public Task EnsureLoadedForCalendarAsync(DateTime displayMonth)
        {
            displayMonth = DateTools.GetMonth(displayMonth);

            DateTime neededStart = displayMonth.AddMonths(-1);
            DateTime neededEnd = displayMonth.AddMonths(2).AddDays(-1); // Need the full next month

            return EnsureLoadedAsync(neededStart, neededEnd, neededStart.AddMonths(-3), displayMonth.AddMonths(4).AddDays(-1));
        }

        public Task EnsureLoadedForDayAsync(DateTime currDay, int bufferDaysNeeded, int bufferDaysToLoad)
        {
            DateTime neededStart = currDay.AddDays(bufferDaysNeeded * -1);
            DateTime neededEnd = currDay.AddDays(bufferDaysNeeded);

            return EnsureLoadedAsync(neededStart, neededEnd, currDay.AddDays(bufferDaysToLoad * -1), currDay.AddDays(bufferDaysToLoad));
        }

        private async Task ApplyFilterBlocking(DateTime prevStart, DateTime prevEnd, DateRange filterRequest)
        {
            DateTime newStart = filterRequest.Start;
            DateTime newEnd = filterRequest.End;

            DateTime notLoadedStart;
            DateTime notLoadedEnd;

            // We're only supporting where the date range shifts in one direction.
            // We're not supporting where the date range expands in both directions.

            if (newEnd > prevEnd)
            {
                notLoadedEnd = newEnd;

                if (newStart < prevEnd)
                    notLoadedStart = prevEnd.AddTicks(1);
                else
                    notLoadedStart = newStart;
            }

            else if (newStart < prevStart)
            {
                notLoadedStart = newStart;

                if (newEnd > prevStart)
                    notLoadedEnd = prevStart.AddTicks(-1);
                else
                    notLoadedEnd = newEnd;
            }

            else
                return;


            var dataStore = await GetDataStore();

            using (await Locks.LockDataForReadAsync())
            {
                // If the pending filter request has changed, do nothing
                if (_pendingFilterRequest != filterRequest)
                    return;

                Guid[] classIdentifiers = Semester.Classes.Select(i => i.Identifier).ToArray();

                DataItemMegaItem[] dataItems = dataStore.TableMegaItems.Where(ShouldIncludeItemFunction(classIdentifiers, notLoadedStart, notLoadedEnd)).ToArray();

                // We need to dispatch to UI thread to actually change the collection
                try
                {
                    Dispatcher.Run(delegate
                    {
                        // If the pending filter request has changed, do nothing
                        if (_pendingFilterRequest != filterRequest)
                                return;


                        // Remove all that are no longer in the date range (don't worry about class changing since data didn't change here)
                        Items.RemoveWhere(i => !ShouldIncludeItem(i, newStart, newEnd));

                        // Then add the new items
                        foreach (var h in dataItems)
                        {
                            Add(h);
                        }

                        Start = newStart;
                        End = newEnd;
                    });
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }
        }

        private static Func<DataItemMegaItem, bool> ShouldIncludeItemFunction(Guid[] classIdentifiers, DateTime start, DateTime end)
        {
            return i =>
                (i.MegaItemType == PowerPlannerSending.MegaItemType.Homework || i.MegaItemType == PowerPlannerSending.MegaItemType.Exam || i.MegaItemType == PowerPlannerSending.MegaItemType.Task)
                && classIdentifiers.Contains(i.UpperIdentifier) && i.Date >= start && i.Date <= end;
        }

        private bool ShouldIncludeItem(DataItemMegaItem i, Guid[] classIdentifiers, DateTime start, DateTime end)
        {
            return ShouldIncludeItemFunction(classIdentifiers, start, end).Invoke(i);
        }

        private bool ShouldIncludeItem(BaseViewItemHomeworkExam i, DateTime start, DateTime end)
        {
            return i.Date >= start && i.Date <= end;
        }

        /// <summary>
        /// Assumes item has already been filtered (date and class)
        /// </summary>
        /// <param name="i"></param>
        private void Add(DataItemMegaItem i)
        {
            if (i.MegaItemType == PowerPlannerSending.MegaItemType.Homework)
            {
                var viewItem = CreateHomework(i);
                AssignClass(i, viewItem);
                if (viewItem.Class == null)
                {
                    TelemetryExtension.Current?.TrackException(new NullReferenceException("CalendarViewItemsGroup: Class for homework couldn't be found"));
                    return;
                }
                Items.InsertSorted(viewItem);
            }
            else if (i.MegaItemType == PowerPlannerSending.MegaItemType.Exam)
            {
                var viewItem = CreateExam(i);
                AssignClass(i, viewItem);
                if (viewItem.Class == null)
                {
                    TelemetryExtension.Current?.TrackException(new NullReferenceException("CalendarViewItemsGroup: Class for exam couldn't be found"));
                    return;
                }
                Items.InsertSorted(viewItem);
            }
            else if (i.MegaItemType == PowerPlannerSending.MegaItemType.Task)
            {
                var viewItem = CreateTask(i);
                AssignClass(i, viewItem);
                Items.InsertSorted(viewItem);
            }
        }

        private void AssignClass(DataItemMegaItem data, ViewItemExam view)
        {
            view.Class = Semester.Classes.FirstOrDefault(i => i.Identifier == data.UpperIdentifier);
        }

        private void AssignClass(DataItemMegaItem data, ViewItemHomework view)
        {
            if (data.MegaItemType == PowerPlannerSending.MegaItemType.Task)
            {
                view.Class = Semester.NoClassClass;
            }
            else
            {
                view.Class = Semester.Classes.First(i => i.Identifier == data.UpperIdentifier);
            }
        }

        private ViewItemHomework CreateHomework(DataItemMegaItem h)
        {
            return new ViewItemHomework(h);
        }

        private ViewItemExam CreateExam(DataItemMegaItem e)
        {
            return new ViewItemExam(e);
        }

        private ViewItemHomework CreateTask(DataItemMegaItem i)
        {
            return new ViewItemHomework(i);
        }

        private BaseViewItemHomeworkExam CreateItem(DataItemMegaItem dataItem)
        {
            if (dataItem.MegaItemType == PowerPlannerSending.MegaItemType.Homework)
                return CreateHomework(dataItem);
            else if (dataItem.MegaItemType == PowerPlannerSending.MegaItemType.Exam)
                return CreateExam(dataItem);
            else if (dataItem.MegaItemType == PowerPlannerSending.MegaItemType.Task)
                return CreateTask(dataItem);
            else
                throw new NotImplementedException("Wasn't any of expected types");
        }

        protected override void OnDataChangedEvent(DataChangedEvent e)
        {
            if (Items != null && Semester != null)
            {
                Guid[] classIdentifiers = Semester.Classes.Select(i => i.Identifier).ToArray();

                // Remove items that were deleted
                Items.RemoveWhere(i => e.DeletedItems.Contains(i.Identifier));

                // Look through edited items
                foreach (var edited in e.EditedItems.OfType<DataItemMegaItem>())
                {
                    var matched = Items.FirstOrDefault(i => i.Identifier == edited.Identifier);

                    // If the edited item should be in our collection
                    if (ShouldIncludeItem(edited, classIdentifiers, Start, End))
                    {
                        // If there was a matching, we need to update it
                        if (matched != null)
                        {
                            matched.PopulateFromDataItem(edited);

                            if (matched is ViewItemHomework)
                                AssignClass(edited, matched as ViewItemHomework);
                            else
                                AssignClass(edited, (ViewItemExam)matched);

                            // And then add/remove (a.k.a. resort)
                            Items.Remove(matched);
                            Items.Add(matched);
                        }

                        // Otherwise we need to create a view item
                        else
                        {
                            Add(edited);
                        }
                    }

                    // Otherwise it shouldn't be in our collection
                    else
                    {
                        // If there was a matching, we need to remove it since it's no longer needed
                        if (matched != null)
                            Items.Remove(matched);
                    }
                }

                // Any items that are no longer have a class need to be removed
                Items.RemoveWhere(i => (i is ViewItemHomework && !Semester.Classes.Contains((i as ViewItemHomework).Class)) || (i is ViewItemExam && !Semester.Classes.Contains((i as ViewItemExam).Class)));

                // Add new items
                foreach (var newItem in e.NewItems.OfType<DataItemMegaItem>().Where(i => ShouldIncludeItem(i, classIdentifiers, Start, End)))
                    Add(newItem);
            }
        }
    }
}
