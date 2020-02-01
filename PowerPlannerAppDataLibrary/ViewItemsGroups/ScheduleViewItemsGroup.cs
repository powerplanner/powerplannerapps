using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Exceptions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewItemsGroups
{
    public class ScheduleViewItemsGroup : BaseAccountViewItemsGroup
    {
        public Guid SemesterId { get; private set; }

        public event EventHandler<DataChangedEvent> OnChangesOccurred;

        public ViewItemSemester Semester { get; private set; }

        private bool _includeWeightCategories;
        private Task _loadingTask;

        public MyObservableList<ViewItemClass> Classes
        {
            get { return Semester.Classes; }
        }

        private ScheduleViewItemsGroup(Guid localAccountId, Guid semesterId, bool trackChanges, bool includeWeightCategories) : base(localAccountId, trackChanges)
        {
            SemesterId = semesterId;
            _includeWeightCategories = includeWeightCategories;
        }

        private static WeakReferenceList<ScheduleViewItemsGroup> _cachedItems = new WeakReferenceList<ScheduleViewItemsGroup>();

        public static async Task<ScheduleViewItemsGroup> LoadAsync(Guid localAccountId, Guid semesterId, bool trackChanges = true, bool includeWeightCategories = true)
        {
            ScheduleViewItemsGroup answer;
            bool usedCached = false;

            // Find a cached match
            // Note that if the requester doesn't require weight categories, we'll ignore checking that includeWeightCategories matches,
            // since returning a cache that includes weight categories doesn't harm anything.
            lock (_cachedItems)
            {
                var cached = _cachedItems.FirstOrDefault(i =>
                    i.LocalAccountId == localAccountId
                    && i.SemesterId == semesterId
                    && (!includeWeightCategories || i._includeWeightCategories == includeWeightCategories));

                // If we'd like the changes tracked and we already have a matching cached
                if (trackChanges && cached != null)
                {
                    usedCached = true;
                    answer = cached;
                }

                else
                {
                    // Otherwise, no cached or no change tracking requested, so load new
                    answer = new ScheduleViewItemsGroup(localAccountId, semesterId, trackChanges, includeWeightCategories);
                    answer._loadingTask = Task.Run(answer.LoadBlocking);

                    // If we're tracking changes, we'll add it to the cached
                    if (trackChanges)
                    {
                        _cachedItems.Add(answer);
                    }
                }
            }

            try
            {
                await answer._loadingTask;
                return answer;
            }
            catch
            {
                if (usedCached)
                {
                    // If we were using cached and it previously failed to load,
                    // we remove it and try again
                    lock (_cachedItems)
                    {
                        _cachedItems.Remove(answer);
                    }
                }
                else
                {
                    throw;
                }
            }

            // We try to load again since we used cached and it failed previously
            return await LoadAsync(localAccountId, semesterId, trackChanges: trackChanges, includeWeightCategories: includeWeightCategories);
        }

        private async Task LoadBlocking()
        {
            var dataStore = await GetDataStore();

            DataItemClass[] dataClasses;
            DataItemSchedule[] dataSchedules;
            DataItemWeightCategory[] dataWeights = null; // Weights are now needed for adding homework/exams
            DataItemSemester dataSemester;

            using (await Locks.LockDataForReadAsync("ScheduleViewItemsGroup.LoadBlocking"))
            {
                var timeTracker = TimeTracker.Start();
                dataSemester = dataStore.TableSemesters.FirstOrDefault(i => i.Identifier == SemesterId);

                if (dataSemester == null)
                    throw new SemesterNotFoundException();

                dataClasses = dataStore.TableClasses.Where(i => i.UpperIdentifier == SemesterId).ToArray();

                Guid[] classIdentifiers = dataClasses.Select(i => i.Identifier).ToArray();

                dataSchedules = dataStore.TableSchedules.Where(i => classIdentifiers.Contains(i.UpperIdentifier)).ToArray();

                if (_includeWeightCategories)
                {
                    dataWeights = dataStore.TableWeightCategories.Where(i => classIdentifiers.Contains(i.UpperIdentifier)).ToArray();
                }
                timeTracker.End(3, "ScheduleViewItemsGroup.LoadBlocking loading items from database");

                timeTracker = TimeTracker.Start();
                var semester = new ViewItemSemester(dataSemester, CreateClass);

                semester.FilterAndAddChildren(dataClasses);

                foreach (var c in semester.Classes)
                {
                    c.FilterAndAddChildren(dataSchedules);

                    if (_includeWeightCategories)
                    {
                        c.FilterAndAddChildren(dataWeights);
                    }
                }

                this.Semester = semester;
                timeTracker.End(3, "ScheduleViewItemsGroup.LoadBlocking constructing view items");
            }
        }

        private ViewItemClass CreateClass(DataItemClass c)
        {
            return new ViewItemClass(c,
                createScheduleMethod: CreateSchedule,
                createWeightMethod: _includeWeightCategories ? new Func<DataItemWeightCategory, ViewItemWeightCategory>(CreateWeightCategory) : null)
            {
                Semester = Semester
            };
        }

        private ViewItemWeightCategory CreateWeightCategory(DataItemWeightCategory w)
        {
            return new ViewItemWeightCategory(w);
        }

        private ViewItemSchedule CreateSchedule(DataItemSchedule s)
        {
            return new ViewItemSchedule(s);
        }

        protected override void OnDataChangedEvent(DataChangedEvent e)
        {
            if (Semester != null)
            {
                if (Semester.HandleDataChangedEvent(e))
                {
                    if (OnChangesOccurred != null)
                        OnChangesOccurred(this, e);
                }
            }
        }
    }
}
