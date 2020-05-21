using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
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
    public class SemesterItemsViewGroup : BaseAccountViewItemsGroup
    {
        public event EventHandler OnItemsChanged;

        public ViewItemSemester Semester { get; private set; }

        public MyObservableList<ViewItemClass> Classes
        {
            get { return Semester.Classes; }
        }

        public MyObservableList<BaseViewItemMegaItem> Items { get; private set; } = new MyObservableList<BaseViewItemMegaItem>();

        public Task LoadingTask { get; private set; }

        private static WeakReferenceCache<Guid, SemesterItemsViewGroup> _cachedSemesterItems = new WeakReferenceCache<Guid, SemesterItemsViewGroup>();

        public static SemesterItemsViewGroup Load(Guid localAccountId, ViewItemSemester semester, bool trackChanges = true)
        {
            if (trackChanges)
            {
                SemesterItemsViewGroup answer;
                lock (_cachedSemesterItems)
                {
                    if (_cachedSemesterItems.TryGetValue(semester.Identifier, out answer))
                    {
                        return answer;
                    }

                    answer = CreateInstance(localAccountId, semester, true);
                    _cachedSemesterItems[semester.Identifier] = answer;
                    return answer;
                }
            }

            // If not tracking changes, we'll load a fresh instance
            return CreateInstance(localAccountId, semester, false);
        }

        public static SemesterItemsViewGroup GetCached(Guid semesterId)
        {
            SemesterItemsViewGroup answer;
            lock (_cachedSemesterItems)
            {
                if (_cachedSemesterItems.TryGetValue(semesterId, out answer))
                {
                    return answer;
                }
            }
            return null;
        }

        private static SemesterItemsViewGroup CreateInstance(Guid localAccountId, ViewItemSemester semester, bool trackChanges = true)
        {
            var answer = new SemesterItemsViewGroup(localAccountId, semester, trackChanges);
            answer.LoadingTask = Task.Run(answer.LoadBlocking);
            return answer;
        }

        private SemesterItemsViewGroup(Guid localAccountId, ViewItemSemester semester, bool trackChanges = true) : base(localAccountId, trackChanges)
        {
            if (semester == null)
                throw new ArgumentNullException("semester");

            Semester = semester;
        }

        private async Task LoadBlocking()
        {
            var dataStore = await GetDataStore();

            DataItemMegaItem[] dataItems;

            using (await Locks.LockDataForReadAsync("SemesterItemsViewGroup.LoadBlocking"))
            {
                Guid semesterIdentifier = Semester.Identifier;
                Guid[] classIdentifiers = Semester.Classes.Select(i => i.Identifier).ToArray();

                dataItems = dataStore.TableMegaItems.Where(ShouldIncludeItemFunction(semesterIdentifier, classIdentifiers)).ToArray();
            }

            await Dispatcher.RunAsync(delegate
            {
                foreach (var i in dataItems)
                {
                    Add(i);
                }

                if (dataItems.Length > 0)
                {
                    OnItemsChanged?.Invoke(this, new EventArgs());
                }
            });
        }

        /// <summary>
        /// Assumes item has already been filtered
        /// </summary>
        /// <param name="h"></param>
        private void Add(DataItemMegaItem i)
        {
            ViewItemTaskOrEvent taskOrExam = i.CreateViewItemTaskOrEvent(Semester);
            if (taskOrExam != null)
            {
                Items.Add(taskOrExam);
            }
            else if (i.MegaItemType == PowerPlannerSending.MegaItemType.Holiday)
            {
                var viewItem = CreateHoliday(i);
                Items.Add(viewItem);
            }
        }

        private ViewItemHoliday CreateHoliday(DataItemMegaItem i)
        {
            return new ViewItemHoliday(i);
        }

        private static Func<DataItemMegaItem, bool> ShouldIncludeItemFunction(Guid semesterIdentifier, Guid[] classIdentifiers)
        {
            return i =>

                // Things that are children of semester
                ((i.MegaItemType == PowerPlannerSending.MegaItemType.Holiday || i.MegaItemType == PowerPlannerSending.MegaItemType.Task || i.MegaItemType == PowerPlannerSending.MegaItemType.Event)
                    && i.UpperIdentifier == semesterIdentifier)

                // Things that are children of class
                || ((i.MegaItemType == PowerPlannerSending.MegaItemType.Homework || i.MegaItemType == PowerPlannerSending.MegaItemType.Exam)
                    && classIdentifiers.Contains(i.UpperIdentifier));
        }

        private bool ShouldIncludeItem(DataItemMegaItem i, Guid semesterIdentifier, Guid[] classIdentifiers)
        {
            return ShouldIncludeItemFunction(semesterIdentifier, classIdentifiers).Invoke(i);
        }

        protected override void OnDataChangedEvent(DataChangedEvent e)
        {
            if (Items != null && Semester != null)
            {
                bool changed = false;

                Guid[] classIdentifiers = Semester.Classes.Select(i => i.Identifier).ToArray();

                // Remove items that were deleted
                changed = Items.RemoveWhere(i => e.DeletedItems.Contains(i.Identifier));

                // Look through edited items
                foreach (var edited in e.EditedItems.OfType<DataItemMegaItem>())
                {
                    var matchedIndex = Items.FindIndex(i => i.Identifier == edited.Identifier);

                    // If the edited item should be in our collection
                    if (ShouldIncludeItem(edited, Semester.Identifier, classIdentifiers))
                    {
                        // If there was a matching, we need to update it
                        if (matchedIndex != -1)
                        {
                            var matched = Items[matchedIndex];

                            matched.PopulateFromDataItem(edited);

                            if (matched is ViewItemTaskOrEvent taskOrEvent)
                                AssignClass(edited, taskOrEvent);

                            Items[matchedIndex] = matched;
                            changed = true;
                        }

                        // Otherwise we need to create a view item
                        else
                        {
                            Add(edited);
                            changed = true;
                        }
                    }

                    // Otherwise it shouldn't be in our collection
                    else
                    {
                        // If there was a matching, we need to remove it since it's no longer needed
                        if (matchedIndex != -1)
                        {
                            Items.RemoveAt(matchedIndex);
                            changed = true;
                        }
                    }
                }

                // Any items that are no longer have a class need to be removed
                changed = changed || Items.RemoveWhere(i =>
                    i is ViewItemTaskOrEvent taskOrEvent && !Semester.Classes.Contains(taskOrEvent.Class) && !taskOrEvent.Class.IsNoClassClass);

                // Add new items
                foreach (var newItem in e.NewItems.OfType<DataItemMegaItem>().Where(ShouldIncludeItemFunction(Semester.Identifier, classIdentifiers)))
                {
                    Add(newItem);
                    changed = true;
                }

                if (changed)
                {
                    OnItemsChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        private void AssignClass(DataItemMegaItem data, ViewItemTaskOrEvent view)
        {
            if (data.MegaItemType == PowerPlannerSending.MegaItemType.Task || data.MegaItemType == PowerPlannerSending.MegaItemType.Event)
            {
                view.Class = Semester.NoClassClass;
            }
            else
            {
                view.Class = Semester.Classes.First(i => i.Identifier == data.UpperIdentifier);
            }
        }
    }
}
