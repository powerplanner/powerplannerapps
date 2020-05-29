using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Extensions.Telemetry;
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
    public class ClassViewItemsGroup : BaseAccountViewItemsGroup
    {
        public class ClassNotFoundExcetion : Exception
        {

        }

        private Guid _classId;

        public DateTime Today { get; private set; }
        public DateTime TodayAsUtc { get; private set; }

        private ClassViewItemsGroup(Guid localAccountId, Guid classId, DateTime today) : base(localAccountId)
        {
            _classId = classId;
            Today = today;
            TodayAsUtc = DateTime.SpecifyKind(today, DateTimeKind.Utc);
        }

        private ViewItemSemester _semester;

        public ViewItemClass Class { get; private set; }

        public IMyObservableReadOnlyList<ViewItemTaskOrEvent> Tasks { get; private set; }

        public IMyObservableReadOnlyList<ViewItemTaskOrEvent> Events { get; private set; }

        public MyObservableList<ViewItemTaskOrEvent> PastCompletedTasksAndEvents;

        private MyObservableList<ViewItemTaskOrEvent> _pastCompletedTasks;
        public MyObservableList<ViewItemTaskOrEvent> PastCompletedTasks
        {
            get => _pastCompletedTasks;
            set => SetProperty(ref _pastCompletedTasks, value, nameof(PastCompletedTasks));
        }

        private MyObservableList<ViewItemTaskOrEvent> _pastCompletedEvents;
        public MyObservableList<ViewItemTaskOrEvent> PastCompletedEvents
        {
            get => _pastCompletedEvents;
            set => SetProperty(ref _pastCompletedEvents, value, nameof(PastCompletedEvents));
        }

        private MyObservableList<ViewItemTaskOrEvent> _unassignedItems;
        public MyObservableList<ViewItemTaskOrEvent> UnassignedItems
        {
            get { return _unassignedItems; }
            set { SetProperty(ref _unassignedItems, value, nameof(UnassignedItems)); }
        }

        private bool _hasUnassignedItems;
        public bool HasUnassignedItems
        {
            get { return _hasUnassignedItems; }
            set { SetProperty(ref _hasUnassignedItems, value, nameof(HasUnassignedItems)); }
        }

        /// <summary>
        /// Throws ArgumentException if class wasn't found
        /// </summary>
        /// <param name="localAccountId"></param>
        /// <param name="classId"></param>
        /// <returns></returns>
        public static async Task<ClassViewItemsGroup> LoadAsync(Guid localAccountId, Guid classId, DateTime today, ViewItemSemester viewItemSemester, bool includeWeights = true)
        {
            ClassViewItemsGroup answer = new ClassViewItemsGroup(localAccountId, classId, today);
            await answer.LoadAsync(viewItemSemester, includeWeights);
            return answer;
        }

        private async Task LoadAsync(ViewItemSemester viewItemSemester, bool includeWeights)
        {
            await Task.Run(async delegate { await LoadBlocking(viewItemSemester, includeWeights); });
        }

        private async Task LoadBlocking(ViewItemSemester viewItemSemester, bool includeWeights)
        {
            var dataStore = await GetDataStore();

            DataItemClass dataClass;
            DataItemClass[] dataClasses;
            DataItemSchedule[] dataSchedules;
            DataItemWeightCategory[] dataWeights;

            using (await Locks.LockDataForReadAsync("ClassViewItemsGroup.LoadBlocking"))
            {
                dataClasses = viewItemSemester.Classes.Select(i => i.DataItem).OfType<DataItemClass>().ToArray();

                var viewClassRef = viewItemSemester.Classes.FirstOrDefault(i => i.Identifier == _classId);
                dataClass = viewClassRef?.DataItem as DataItemClass;

                if (dataClass == null)
                    throw new ClassNotFoundExcetion();

                dataSchedules = viewClassRef.Schedules.Select(i => i.DataItem).OfType<DataItemSchedule>().ToArray();

                if (includeWeights)
                {
                    // Get weights for ALL classes, since we need them for editing purposes when editing item to different class
                    dataWeights = viewItemSemester.Classes.SelectMany(i => i.WeightCategories).Select(i => i.DataItem).OfType<DataItemWeightCategory>().ToArray();
                }
                else
                {
                    dataWeights = null;
                }

                Func<DataItemWeightCategory, ViewItemWeightCategory> createWeight = null;
                if (includeWeights)
                {
                    createWeight = CreateWeight;
                }

                var classItem = new ViewItemClass(
                    dataClass,
                    createScheduleMethod: CreateSchedule,
                    createWeightMethod: createWeight);

                classItem.FilterAndAddChildren(dataSchedules);

                if (includeWeights)
                {
                    classItem.FilterAndAddChildren(dataWeights);
                }

                this.Class = classItem;

                _semester = new ViewItemSemester(dataClass.UpperIdentifier, createClassMethod: CreateClass);

                dataClasses = dataStore.TableClasses.Where(i => i.UpperIdentifier == _semester.Identifier).ToArray();

                _semester.FilterAndAddChildren(dataClasses);

                // Add the weights for the other classes
                if (includeWeights)
                {
                    foreach (var c in _semester.Classes.Where(i => i.Identifier != this.Class.Identifier))
                    {
                        c.FilterAndAddChildren(dataWeights);
                    }
                }
            }

            // If there were no weights in the class, we need to create and add a weight
            if (this.Class.WeightCategories != null && this.Class.WeightCategories.Count == 0)
            {
                TelemetryExtension.Current?.TrackEvent("Error_ClassMissingWeightCategoryAddingDefault");

                DataChanges changes = new DataLayer.DataChanges();
                changes.Add(AccountDataStore.CreateDefaultWeightCategory(this.Class.Identifier));

                await PowerPlannerApp.Current.SaveChanges(changes);
            }
        }

        private TaskCompletionSource<bool> _loadGradesTaskSource = new TaskCompletionSource<bool>();
        public Task LoadGradesTask
        {
            get { return _loadGradesTaskSource.Task; }
        }

        private bool _hasGradesBeenRequested;
        private bool _isGradesLoaded;
        public bool IsGradesLoaded
        {
            get { return _isGradesLoaded; }
            set { SetProperty(ref _isGradesLoaded, value, nameof(IsGradesLoaded)); }
        }

        public async void LoadGrades()
        {
            try
            {
                if (_hasGradesBeenRequested)
                {
                    return;
                }

                _hasGradesBeenRequested = true;

                await Task.Run(async delegate
                {
                    try
                    {
                        var dataStore = await GetDataStore();
                        
                        DataItemGrade[] dataGrades;
                        DataItemMegaItem[] dataItems;

                        using (await Locks.LockDataForReadAsync())
                        {
                            Guid[] weightIds = this.Class.WeightCategories.Select(i => i.Identifier).ToArray();

                            dataGrades = dataStore.TableGrades.Where(i => weightIds.Contains(i.UpperIdentifier)).ToArray();

                            dataItems = dataStore.TableMegaItems.Where(i =>
                                (i.MegaItemType == PowerPlannerSending.MegaItemType.Exam || i.MegaItemType == PowerPlannerSending.MegaItemType.Homework)
                                && i.UpperIdentifier == _classId
                                && i.WeightCategoryIdentifier != PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_EXCLUDED)
                                .ToArray();

                            var unassignedItems = new MyObservableList<ViewItemTaskOrEvent>();
                            unassignedItems.InsertSorted(dataItems
                                .Where(i => IsUnassignedChild(i))
                                .Select(i => new ViewItemTaskOrEvent(i) { Class = this.Class, WeightCategory = ViewItemWeightCategory.UNASSIGNED }));

                            PortableDispatcher.GetCurrentDispatcher().Run(delegate
                            {
                                try
                                {
                                    foreach (var weight in this.Class.WeightCategories)
                                    {
                                        weight.AddGradesHelper(ViewItemWeightCategory.CreateGradeHelper);

                                        weight.FilterAndAddChildren<BaseDataItemHomeworkExamGrade>(dataGrades);
                                        weight.FilterAndAddChildren<BaseDataItemHomeworkExamGrade>(dataItems);
                                    }

                                    Class.CalculateEverything();

                                    UnassignedItems = unassignedItems;
                                    HasUnassignedItems = unassignedItems.Count > 0;

                                    _loadGradesTaskSource.SetResult(true);
                                    IsGradesLoaded = true;
                                }
                                catch (Exception ex)
                                {
                                    TelemetryExtension.Current?.TrackException(ex);
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private TaskCompletionSource<bool> _loadTasksAndEventsCompletionSource = new TaskCompletionSource<bool>();
        public Task LoadTasksAndEventsTask
        {
            get { return _loadTasksAndEventsCompletionSource.Task; }
        }

        private bool _hasTasksAndEventsBeenRequested;
        public async void LoadTasksAndEvents()
        {
            try
            {
                if (_hasTasksAndEventsBeenRequested)
                {
                    return;
                }

                _hasTasksAndEventsBeenRequested = true;

                bool hasPastCompletedTasks = false;
                bool hasPastCompletedEvents = false;

                SemesterItemsViewGroup cached = null;
                if (this.Class.Semester != null)
                {
                    cached = SemesterItemsViewGroup.GetCached(this.Class.Semester.Identifier);
                }

                if (cached != null)
                {
                    await cached.LoadingTask;
                    DataItemMegaItem[] dataMegaItems = cached.Items
                        .OfType<ViewItemTaskOrEvent>()
                        .Select(i => i.DataItem)
                        .OfType<DataItemMegaItem>()
                        .ToArray();

                    this.Class.AddTasksAndEventsChildrenHelper(CreateTaskOrEvent, ShouldIncludeTaskOrEventFunction(_classId, TodayAsUtc));
                    this.Class.FilterAndAddChildren(dataMegaItems);

                    hasPastCompletedTasks = dataMegaItems.Any(IsPastCompletedTaskFunction(_classId, TodayAsUtc));
                    hasPastCompletedEvents = dataMegaItems.Any(IsPastCompletedEventFunction(_classId, TodayAsUtc));
                }
                else
                {
                    await Task.Run(async delegate
                    {
                        var dataStore = await GetDataStore();

                        DataItemMegaItem[] dataTasksOrEvents;

                        using (await Locks.LockDataForReadAsync())
                        {
                            dataTasksOrEvents = dataStore.TableMegaItems.Where(ShouldIncludeTaskOrEventFunction(_classId, TodayAsUtc)).ToArray();

                            this.Class.AddTasksAndEventsChildrenHelper(CreateTaskOrEvent, ShouldIncludeTaskOrEventFunction(_classId, TodayAsUtc));
                            this.Class.FilterAndAddChildren(dataTasksOrEvents);

                            hasPastCompletedTasks = dataStore.TableMegaItems.Any(IsPastCompletedTaskFunction(_classId, TodayAsUtc));
                            hasPastCompletedEvents = dataStore.TableMegaItems.Any(IsPastCompletedEventFunction(_classId, TodayAsUtc));
                        }
                    });
                }

                HasPastCompletedTasks = hasPastCompletedTasks;
                HasPastCompletedEvents = hasPastCompletedEvents;
                Tasks = this.Class.TasksAndEvents.Sublist(ShouldIncludeInNormalTasksFunction(TodayAsUtc)).Cast<ViewItemTaskOrEvent>();
                Events = new MyObservableList<ViewItemTaskOrEvent>();
                (Events as MyObservableList<ViewItemTaskOrEvent>).InsertSorted(
                    this.Class.TasksAndEvents.Sublist(ShouldIncludeInNormalExamsFunction()).Cast<ViewItemTaskOrEvent>());
                OnPropertyChanged(nameof(Tasks));
                OnPropertyChanged(nameof(Events));

                _loadTasksAndEventsCompletionSource.SetResult(true);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private ViewItemClass CreateClass(DataItemClass dataClass)
        {
            if (dataClass.Identifier == this.Class.Identifier)
                return this.Class;

            return new ViewItemClass(dataClass, createWeightMethod: CreateWeightForOtherClasses);
        }

        private ViewItemTaskOrEvent CreateTaskOrEvent(DataItemMegaItem dataTaskOrEvent)
        {
            return new ViewItemTaskOrEvent(dataTaskOrEvent);
        }

        private static ViewItemSchedule CreateSchedule(DataItemSchedule dataSchedule)
        {
            return new ViewItemSchedule(dataSchedule);
        }

        private static Func<DataItemMegaItem, bool> ShouldIncludeTaskOrEventFunction(Guid classId, DateTime todayAsUtc)
        {
            return i =>
                (i.MegaItemType == PowerPlannerSending.MegaItemType.Homework
                && i.UpperIdentifier == classId
                && (i.PercentComplete < 1 || i.Date >= todayAsUtc))
                ||
                (i.MegaItemType == PowerPlannerSending.MegaItemType.Exam
                && i.UpperIdentifier == classId
                && i.Date >= todayAsUtc);
        }

        private static Func<ViewItemTaskOrEvent, bool> ShouldIncludeInNormalTasksFunction(DateTime todayAsUtc)
        {
            return i => i.Type == TaskOrEventType.Task && (i.PercentComplete < 1 || i.Date >= todayAsUtc);
        }

        private static Func<DataItemMegaItem, bool> IsPastCompletedTaskOrEventFunction(Guid classId, DateTime todayAsUtc)
        {
            DateTime nextDay = todayAsUtc.Date.AddDays(1);

            return i =>
                (i.MegaItemType == PowerPlannerSending.MegaItemType.Homework
                && i.UpperIdentifier == classId
                && (i.PercentComplete >= 1 && !(i.Date >= todayAsUtc))) // Negate that last date operation since we need to ignore the seconds on the date
                ||
                (i.MegaItemType == PowerPlannerSending.MegaItemType.Exam
                && i.UpperIdentifier == classId
                && i.Date < nextDay);
        }

        private static Func<DataItemMegaItem, bool> IsPastCompletedTaskFunction(Guid classId, DateTime todayAsUtc)
        {
            return i =>
                (i.MegaItemType == PowerPlannerSending.MegaItemType.Homework
                && i.UpperIdentifier == classId
                && (i.PercentComplete >= 1 && !(i.Date >= todayAsUtc))); // Negate that last date operation since we need to ignore the seconds on the date
        }

        private static Func<DataItemMegaItem, bool> IsPastCompletedEventFunction(Guid classId, DateTime todayAsUtc)
        {
            DateTime nextDay = todayAsUtc.Date.AddDays(1);

            // Technically this is incorrect since it'll include events that are on today and haven't finished yet,
            // but there's no easy way in the database to calculate this (since the class schedule is considered), so
            // this is close enough and only would be incorrect on the very first event

            return i =>
                (i.MegaItemType == PowerPlannerSending.MegaItemType.Exam
                && i.UpperIdentifier == classId
                && i.Date < nextDay);
        }

        private static Func<ViewItemTaskOrEvent, bool> ShouldIncludeInNormalExamsFunction()
        {
            return i => i.Type == TaskOrEventType.Event && !i.IsComplete;
        }

        private ViewItemWeightCategory CreateWeight(DataItemWeightCategory dataWeight)
        {
            return new ViewItemWeightCategory(
                dataWeight,
                createGradeMethod: _isGradesLoaded ? new Func<BaseDataItemHomeworkExamGrade, BaseViewItemMegaItem>(ViewItemWeightCategory.CreateGradeHelper) : null);
        }

        private ViewItemWeightCategory CreateWeightForOtherClasses(DataItemWeightCategory dataWeight)
        {
            return new ViewItemWeightCategory(dataWeight);
        }

        protected override void OnDataChangedEvent(DataChangedEvent e)
        {
            base.OnDataChangedEvent(e);

            if (_semester != null)
            {
                // Look through edited items
                if (Class.TasksAndEvents != null)
                {
                    foreach (var edited in e.EditedItems.OfType<DataItemMegaItem>())
                    {
                        var matched = Class.TasksAndEvents.FirstOrDefault(i => i.Identifier == edited.Identifier);

                        // If found matching
                        if (matched != null)
                        {
                            // If no longer under this class, we need to re-assign the class
                            if (matched.Class.Identifier != edited.UpperIdentifier)
                            {
                                if (edited.UpperIdentifier == _semester.NoClassClass.Identifier)
                                {
                                    matched.Class = _semester.NoClassClass;
                                }
                                else
                                {
                                    matched.Class = _semester.Classes.FirstOrDefault(i => i.Identifier == edited.UpperIdentifier);
                                }
                            }
                        }
                    }
                }

                // Re-assigning classes needs to be done before, otherwise we'll no longer have a reference to the object
                _semester.HandleDataChangedEvent(e);

                // Only calculates if needed
                if (IsGradesLoaded)
                {
                    Class.CalculateEverything();
                }

                // If we previously didn't have old items, see whether we do now
                if (!HasPastCompletedTasks)
                {
                    bool hasPastCompleted = e.EditedItems.Concat(e.NewItems).OfType<DataItemMegaItem>().Any(
                        h => IsPastCompletedTaskFunction(_classId, TodayAsUtc).Invoke(h));

                    if (hasPastCompleted)
                    {
                        HasPastCompletedTasks = true;
                    }
                }

                if (!HasPastCompletedEvents)
                {
                    bool hasPastCompleted = e.EditedItems.Concat(e.NewItems).OfType<DataItemMegaItem>().Any(
                        exam => IsPastCompletedEventFunction(_classId, TodayAsUtc).Invoke(exam));

                    if (hasPastCompleted)
                    {
                        HasPastCompletedEvents = true;
                    }
                }

                if (UnassignedItems != null)
                {
                    // Remove any deleted
                    if (e.DeletedItems.Any())
                    {
                        UnassignedItems.RemoveWhere(i => e.DeletedItems.Contains(i.Identifier));
                    }

                    List<DataItemMegaItem> potentialNew = new List<DataItemMegaItem>(e.NewItems.OfType<DataItemMegaItem>());

                    // Look through edited
                    foreach (var edited in e.EditedItems.OfType<DataItemMegaItem>())
                    {
                        var matching = UnassignedItems.FirstOrDefault(i => i.Identifier == edited.Identifier);
                        if (matching != null)
                        {
                            // If it should be removed
                            if (!IsUnassignedChild(edited))
                            {
                                UnassignedItems.Remove(matching);
                            }

                            // Otherwise it needs to be updated and then re-sorted
                            else
                            {
                                matching.PopulateFromDataItem(edited);
                                UnassignedItems.Remove(matching);
                                UnassignedItems.InsertSorted(matching);
                            }
                        }

                        // New
                        else
                        {
                            potentialNew.Add(edited);
                        }
                    }

                    foreach (var newItem in potentialNew)
                    {
                        if (IsUnassignedChild(newItem))
                        {
                            ViewItemTaskOrEvent newViewItem;
                            if (newItem.MegaItemType == PowerPlannerSending.MegaItemType.Homework || newItem.MegaItemType == PowerPlannerSending.MegaItemType.Exam)
                            {
                                newViewItem = new ViewItemTaskOrEvent(newItem)
                                {
                                    Class = this.Class,
                                    WeightCategory = ViewItemWeightCategory.UNASSIGNED
                                };
                            }
                            else
                            {
                                continue;
                            }

                            UnassignedItems.InsertSorted(newViewItem);
                        }
                    }

                    HasUnassignedItems = UnassignedItems.Count > 0;
                }
            }
        }

        private bool IsUnassignedChild(DataItemMegaItem item)
        {
            // It's a child if it's a child of this class and weight category is unassiged OR weight category not found
            return item.UpperIdentifier == _classId
                && (item.MegaItemType == PowerPlannerSending.MegaItemType.Exam || item.MegaItemType == PowerPlannerSending.MegaItemType.Homework)
                && item.WeightCategoryIdentifier != PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_EXCLUDED
                && (item.WeightCategoryIdentifier == PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_UNASSIGNED
                    || !this.Class.WeightCategories.Any(i => i.Identifier == item.WeightCategoryIdentifier));
        }

        private TaskCompletionSource<bool> _loadPastCompleteTasksAndEventsCompletionSource = new TaskCompletionSource<bool>();
        public Task LoadPastCompleteTasksAndEventsTask
        {
            get { return _loadPastCompleteTasksAndEventsCompletionSource.Task; }
        }

        private bool _hasLoadedPastCompletedTasksAndEvents;
        private async void LoadPastCompletedTasksAndEvents()
        {
            if (_hasLoadedPastCompletedTasksAndEvents)
            {
                return;
            }

            _hasLoadedPastCompletedTasksAndEvents = true;

            try
            {
                var dataStore = await GetDataStore();

                DataItemMegaItem[] additionalTasksAndEvents;

                using (await Locks.LockDataForReadAsync())
                {
                    // Get the data items that we haven't loaded yet
                    additionalTasksAndEvents = dataStore.TableMegaItems.Where(IsPastCompletedTaskOrEventFunction(_classId, TodayAsUtc)).ToArray();

                    // Exclude any that are already loaded (due to the events being complicated to calculate whether they're incomplete, we end up double loading items that are on today)
                    additionalTasksAndEvents = additionalTasksAndEvents.Where(a => !this.Class.TasksAndEvents.Any(i => i.Identifier == a.Identifier)).ToArray();

                    // And then update the child function so that we include all tasks/events for the class, and inject the new items
                    this.Class.UpdateIsChildMethod<DataItemMegaItem, ViewItemTaskOrEvent>(i => i.UpperIdentifier == _classId && i.MegaItemType == PowerPlannerSending.MegaItemType.Homework || i.MegaItemType == PowerPlannerSending.MegaItemType.Exam, additionalTasksAndEvents);
                }

                // Include the opposite of the other function
                PastCompletedTasks = new PastCompletedTasksList(this.Class.TasksAndEvents.Sublist(i => i.Type == TaskOrEventType.Task), TodayAsUtc);
                PastCompletedEvents = new PastCompletedEventsList(this.Class.TasksAndEvents.Sublist(i => i.Type == TaskOrEventType.Event), TodayAsUtc);

                _loadPastCompleteTasksAndEventsCompletionSource.SetResult(true);
            }

            catch (Exception ex)
            {
                _loadPastCompleteTasksAndEventsCompletionSource.SetException(ex);
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private bool _isPastCompletedTasksDisplayed;
        public bool IsPastCompletedTasksDisplayed
        {
            get => _isPastCompletedTasksDisplayed;
            private set => SetProperty(ref _isPastCompletedTasksDisplayed, value, nameof(IsPastCompletedTasksDisplayed));
        }

        public void ShowPastCompletedTasks()
        {
            IsPastCompletedTasksDisplayed = true;
            LoadPastCompletedTasksAndEvents();
        }

        public void HidePastCompletedTasks()
        {
            IsPastCompletedTasksDisplayed = false;
        }

        private bool _isPastCompletedEventsDisplayed;
        public bool IsPastCompletedEventsDisplayed
        {
            get => _isPastCompletedEventsDisplayed;
            private set => SetProperty(ref _isPastCompletedEventsDisplayed, value, nameof(IsPastCompletedEventsDisplayed));
        }

        public void ShowPastCompletedEvents()
        {
            IsPastCompletedEventsDisplayed = true;
            LoadPastCompletedTasksAndEvents();
        }

        public void HidePastCompletedEvents()
        {
            IsPastCompletedEventsDisplayed = false;
        }

        private bool _hasPastCompletedTasks;
        public bool HasPastCompletedTasks
        {
            get => _hasPastCompletedTasks;
            set => SetProperty(ref _hasPastCompletedTasks, value, nameof(HasPastCompletedTasks));
        }

        private bool _hasPastCompletedEvents;
        public bool HasPastCompletedEvents
        {
            get => _hasPastCompletedEvents;
            set => SetProperty(ref _hasPastCompletedEvents, value, nameof(HasPastCompletedEvents));
        }

        private class PastCompletedEventsList : MyObservableList<ViewItemTaskOrEvent>
        {
            public PastCompletedEventsList(IMyObservableReadOnlyList<ViewItemTaskOrEvent> sourceList, DateTime todayAsUtc)
            {
                base.Filter = new FilterUsingFunction(i => !ShouldIncludeInNormalExamsFunction().Invoke(i));
                base.Comparer = new PastCompletedEventsComparer();
                base.InsertSorted(sourceList);
            }

            private class PastCompletedEventsComparer : IComparer<ViewItemTaskOrEvent>
            {
                public int Compare(ViewItemTaskOrEvent x, ViewItemTaskOrEvent y)
                {
                    // Show in reverse order
                    return x.CompareTo(y) * -1;
                }
            }
        }

        private class PastCompletedTasksList : MyObservableList<ViewItemTaskOrEvent>
        {
            public PastCompletedTasksList(IMyObservableReadOnlyList<ViewItemTaskOrEvent> sourceList, DateTime todayAsUtc)
            {
                base.Filter = new FilterUsingFunction(i => !ShouldIncludeInNormalTasksFunction(todayAsUtc).Invoke(i));
                base.Comparer = new PastCompletedTasksComparer();
                base.InsertSorted(sourceList);
            }

            private class PastCompletedTasksComparer : IComparer<ViewItemTaskOrEvent>
            {
                public int Compare(ViewItemTaskOrEvent x, ViewItemTaskOrEvent y)
                {
                    // Show in reverse order
                    return x.CompareTo(y) * -1;
                }
            }
        }
    }
}