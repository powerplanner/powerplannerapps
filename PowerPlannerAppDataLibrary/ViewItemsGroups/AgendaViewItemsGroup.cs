using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
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
    public class AgendaViewItemsGroup : BaseAccountViewItemsGroup
    {
        private class AgendaItemsList : MyObservableList<ViewItemTaskOrEvent>
        {
            public AgendaItemsList(DateTime today, IReadOnlyList<BaseViewItemMegaItem> list) : base()
            {
                base.Filter = new FilterUsingFunction(i => !i.IsComplete);

                base.InsertSorted(list);
            }
        }

        public Guid SemesterId
        {
            get { return _semester.Identifier; }
        }

        private ViewItemSemester _semester;

        public DateTime Today { get; private set; }

        public MyObservableList<ViewItemClass> Classes
        {
            get { return _semester == null ? null : _semester.Classes; }
        }

        /// <summary>
        /// List is NOT sorted
        /// </summary>
        public MyObservableList<ViewItemTaskOrEvent> Items { get; private set; }

        private AgendaViewItemsGroup(Guid localAccountId, ViewItemSemester semester, DateTime today, bool trackChanges = true) : base(localAccountId, trackChanges)
        {
            if (semester == null)
                throw new ArgumentNullException("semester");

            _semester = semester;
            Today = today;
        }

        private class CacheItem
        {
            public ViewItemSemester Semester { get; private set; }
            public DateTime Today { get; private set; }
            public Task<AgendaViewItemsGroup> Task { get; private set; }

            public CacheItem(ViewItemSemester semester, DateTime today, Task<AgendaViewItemsGroup> task)
            {
                Semester = semester;
                Today = today;
                Task = task;
            }
        }

        private static WeakReferenceList<CacheItem> _cache = new WeakReferenceList<CacheItem>();

        public static void ClearCache()
        {
            _cache.Clear();
        }

        public static Task<AgendaViewItemsGroup> LoadAsync(Guid localAccountId, ViewItemSemester semester, DateTime today, bool trackChanges = true)
        {
            if (semester == null)
                throw new ArgumentNullException("semester");

            if (!trackChanges)
            {
                return CreateLoadStandalone(localAccountId, semester, today);
            }

            Task<AgendaViewItemsGroup> answer;
            lock (_cache)
            {
                // We compare exactly on semester reference, since otherwise the children could end up with different class objects than parents.
                // We had a bug here where a semester from the widget was being used, which didn't load the weight categories, and previously we were
                // comparing by identifier, so it used the cached version which led to a crash.
                var matchingCache = _cache.FirstOrDefault(i =>
                    i.Semester == semester
                    && i.Today == today
                    && !i.Task.IsCanceled
                    && !i.Task.IsFaulted);
                if (matchingCache != null)
                {
                    // Note that ideally we would perform a re-filter here to eliminate events that expired today.
                    // However, if we re-filtered from a background thread and a UI thread was listening to this collection,
                    // it would throw an exception. Hence we won't do that.
                    return matchingCache.Task;
                }

                answer = CreateLoadTask(localAccountId, semester, today);
                _cache.Add(new CacheItem(semester, today, answer));
                return answer;
            }
        }

        ///// <summary>
        ///// Eliminates any items that have become expired (like events that have expired)
        ///// </summary>
        //private void Refilter()
        //{
        //    // Beware - can't do this from a background thread or else it would crash things
        //    // Hence we actually need to only do this from a UI thread and also ensure to put it in the
        //    // lock so that background threads using cached item don't get messed up
        //    Guid[] classIdentifiers = Classes.Select(i => i.Identifier).ToArray();
        //    DateTime todayAsUtc = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

        //    this.Items.RemoveWhere(i => !ShouldIncludeItem(i.DataItem as DataItemMegaItem, classIdentifiers, todayAsUtc);
        //}

        private static async Task<AgendaViewItemsGroup> CreateLoadTask(Guid localAccountId, ViewItemSemester semester, DateTime today)
        {
            var answer = new AgendaViewItemsGroup(localAccountId, semester, today, trackChanges: true);

            SemesterItemsViewGroup cached  = SemesterItemsViewGroup.GetCached(semester.Identifier);
            if (cached != null)
            {
                // Can perform this without locks since we already know we're on UI thread right now,
                // and any modifications to the SemesterItems list would need to be on UI thread to occur
                await cached.LoadingTask;
                answer.PrepareFromCached(cached);
            }
            else
            {
                await Task.Run(answer.LoadBlocking);
            }
            return answer;
        }

        private static async Task<AgendaViewItemsGroup> CreateLoadStandalone(Guid localAccountId, ViewItemSemester semester, DateTime today)
        {
            var answer = new AgendaViewItemsGroup(localAccountId, semester, today, trackChanges: false);
            await Task.Run(answer.LoadBlocking);
            return answer;
        }

        private SemesterItemsViewGroup _cachedSemesterItems;
        private void PrepareFromCached(SemesterItemsViewGroup semesterItems)
        {
            // Hold onto a strong reference of the cached items so they continue updating
            _cachedSemesterItems = semesterItems;
            this.Items = new AgendaItemsList(Today, semesterItems.Items);
        }

        private async Task LoadBlocking()
        {
            var dataStore = await GetDataStore();

            DataItemMegaItem[] dataItems;

            using (await Locks.LockDataForReadAsync("AgendaViewItemsGroup.LoadBlocking"))
            {
                Guid[] classIdentifiers = _semester.Classes.Select(i => i.Identifier).ToArray();

                DateTime todayAsUtc = DateTime.SpecifyKind(Today, DateTimeKind.Utc);

                dataItems = dataStore.TableMegaItems.Where(ShouldIncludeItemFunction(classIdentifiers, todayAsUtc)).ToArray();

                this.Items = new MyObservableList<ViewItemTaskOrEvent>();

                foreach (var i in dataItems)
                {
                    Add(i);
                }
            }
        }

        protected override void OnDataChangedEvent(DataChangedEvent e)
        {
            if (Items != null && _semester != null && _cachedSemesterItems == null)
            {
                Guid[] classIdentifiers = _semester.Classes.Select(i => i.Identifier).ToArray();
                DateTime todayAsUtc = DateTime.SpecifyKind(Today, DateTimeKind.Utc);

                // Remove items that were deleted
                Items.RemoveWhere(i => e.DeletedItems.Contains(i.Identifier));

                // Look through edited items
                foreach (var edited in e.EditedItems.OfType<DataItemMegaItem>())
                {
                    var matched = Items.FirstOrDefault(i => i.Identifier == edited.Identifier);

                    // If the edited item should be in our collection
                    if (ShouldIncludeItem(edited, classIdentifiers, todayAsUtc))
                    {
                        // If there was a matching, we need to update it
                        if (matched != null)
                        {
                            matched.PopulateFromDataItem(edited);

                            AssignClass(edited, matched);

                            // And then add/remove (a.k.a. resort)
                            Items.Remove(matched);
                            Add(matched);
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
                Items.RemoveWhere(i => !i.Class.IsNoClassClass && !_semester.Classes.Contains(i.Class));

                // Add new items
                foreach (var newItem in e.NewItems.OfType<DataItemMegaItem>().Where(ShouldIncludeItemFunction(classIdentifiers, todayAsUtc)))
                    Add(newItem);
            }
        }

        private void AssignClass(DataItemMegaItem data, ViewItemTaskOrEvent view)
        {
            if (data.MegaItemType == PowerPlannerSending.MegaItemType.Task || data.MegaItemType == PowerPlannerSending.MegaItemType.Event)
            {
                view.Class = _semester.NoClassClass;
            }
            else
            {
                view.Class = _semester.Classes.First(i => i.Identifier == data.UpperIdentifier);
            }
        }

        /// <summary>
        /// Assumes item has already been filtered (date and class)
        /// </summary>
        /// <param name="h"></param>
        private void Add(DataItemMegaItem i)
        {
            ViewItemTaskOrEvent viewItem = i.CreateViewItemTaskOrEvent(_semester);

            if (viewItem != null && !viewItem.IsComplete)
            {
                Add(viewItem);
            }
        }

        private void Add(ViewItemTaskOrEvent viewItem)
        {
            if (viewItem == null || viewItem.IsComplete)
            {
                // Complete items aren't added
                return;
            }

            Items.Add(viewItem);
        }

        private static Func<DataItemMegaItem, bool> ShouldIncludeItemFunction(Guid[] classIdentifiers, DateTime todayAsUtc)
        {
            return i =>
                (((i.MegaItemType == PowerPlannerSending.MegaItemType.Homework && i.PercentComplete < 1)
                    || (i.MegaItemType == PowerPlannerSending.MegaItemType.Exam && i.Date >= todayAsUtc))
                && classIdentifiers.Contains(i.UpperIdentifier))
                || (i.MegaItemType == PowerPlannerSending.MegaItemType.Task && i.PercentComplete < 1)
                || (i.MegaItemType == PowerPlannerSending.MegaItemType.Event && i.Date >= todayAsUtc);
        }

        private bool ShouldIncludeItem(DataItemMegaItem i, Guid[] classIdentifiers, DateTime todayAsUtc)
        {
            return ShouldIncludeItemFunction(classIdentifiers, todayAsUtc).Invoke(i);
        }
    }
}
