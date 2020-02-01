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

        public IMyObservableReadOnlyList<ViewItemHomework> Homework { get; private set; }

        public IMyObservableReadOnlyList<ViewItemExam> Exams { get; private set; }

        public MyObservableList<BaseViewItemHomeworkExam> PastCompletedHomeworkAndExams;

        public MyObservableList<ViewItemHomework> PastCompletedHomework { get; set; }

        public MyObservableList<ViewItemExam> PastCompletedExams { get; set; }

        public MyObservableList<BaseViewItemHomeworkExam> UnassignedItems { get; set; }

        public bool HasUnassignedItems { get; set; }

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
        public bool IsGradesLoaded { get; set; }

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

                            var unassignedItems = new MyObservableList<BaseViewItemHomeworkExam>();
                            unassignedItems.InsertSorted(dataItems
                                .Where(i => IsUnassignedChild(i))
                                .Select(i =>
                                    i.MegaItemType == PowerPlannerSending.MegaItemType.Homework ?
                                        new ViewItemHomework(i) { Class = this.Class, WeightCategory = ViewItemWeightCategory.UNASSIGNED } as BaseViewItemHomeworkExam
                                        : new ViewItemExam(i) { Class = this.Class, WeightCategory = ViewItemWeightCategory.UNASSIGNED }));

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

        private TaskCompletionSource<bool> _loadHomeworkAndExamsCompletionSource = new TaskCompletionSource<bool>();
        public Task LoadHomeworkAndExamsTask
        {
            get { return _loadHomeworkAndExamsCompletionSource.Task; }
        }

        private bool _hasHomeworkAndExamsBeenRequested;
        public async void LoadHomeworkAndExams()
        {
            try
            {
                if (_hasHomeworkAndExamsBeenRequested)
                {
                    return;
                }

                _hasHomeworkAndExamsBeenRequested = true;

                bool hasPastCompletedHomework = false;
                bool hasPastCompletedExams = false;

                SemesterItemsViewGroup cached = null;
                if (this.Class.Semester != null)
                {
                    cached = SemesterItemsViewGroup.GetCached(this.Class.Semester.Identifier);
                }

                if (cached != null)
                {
                    await cached.LoadingTask;
                    DataItemMegaItem[] dataMegaItems = cached.Items
                        .OfType<BaseViewItemHomeworkExam>()
                        .Select(i => i.DataItem)
                        .OfType<DataItemMegaItem>()
                        .ToArray();

                    this.Class.AddHomeworkAndExamChildrenHelper(CreateHomeworkOrExam, ShouldIncludeHomeworkOrExamFunction(_classId, TodayAsUtc));
                    this.Class.FilterAndAddChildren(dataMegaItems);

                    hasPastCompletedHomework = dataMegaItems.Any(IsPastCompletedHomeworkFunction(_classId, TodayAsUtc));
                    hasPastCompletedExams = dataMegaItems.Any(IsPastCompletedExamFunction(_classId, TodayAsUtc));
                }
                else
                {
                    await Task.Run(async delegate
                    {
                        var dataStore = await GetDataStore();

                        DataItemMegaItem[] dataHomeworks;

                        using (await Locks.LockDataForReadAsync())
                        {
                            dataHomeworks = dataStore.TableMegaItems.Where(ShouldIncludeHomeworkOrExamFunction(_classId, TodayAsUtc)).ToArray();

                            this.Class.AddHomeworkAndExamChildrenHelper(CreateHomeworkOrExam, ShouldIncludeHomeworkOrExamFunction(_classId, TodayAsUtc));
                            this.Class.FilterAndAddChildren(dataHomeworks);

                            hasPastCompletedHomework = dataStore.TableMegaItems.Any(IsPastCompletedHomeworkFunction(_classId, TodayAsUtc));
                            hasPastCompletedExams = dataStore.TableMegaItems.Any(IsPastCompletedExamFunction(_classId, TodayAsUtc));
                        }
                    });
                }

                HasPastCompletedHomework = hasPastCompletedHomework;
                HasPastCompletedExams = hasPastCompletedExams;
                Homework = this.Class.HomeworkAndExams.Sublist(ShouldIncludeInNormalHomeworkFunction(TodayAsUtc)).Cast<ViewItemHomework>();
                Exams = new MyObservableList<ViewItemExam>();
                (Exams as MyObservableList<ViewItemExam>).InsertSorted(
                    this.Class.HomeworkAndExams.Sublist(ShouldIncludeInNormalExamsFunction()).Cast<ViewItemExam>());
                OnPropertyChanged(nameof(Homework));
                OnPropertyChanged(nameof(Exams));

                _loadHomeworkAndExamsCompletionSource.SetResult(true);
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

        private BaseViewItemHomeworkExam CreateHomeworkOrExam(DataItemMegaItem dataHomework)
        {
            if (dataHomework.MegaItemType == PowerPlannerSending.MegaItemType.Homework)
            {
                return new ViewItemHomework(dataHomework);
            }
            else if (dataHomework.MegaItemType == PowerPlannerSending.MegaItemType.Exam)
            {
                return new ViewItemExam(dataHomework);
            }
            else
            {
                throw new NotImplementedException("Unknown type: " + dataHomework.MegaItemType);
            }
        }

        private static ViewItemSchedule CreateSchedule(DataItemSchedule dataSchedule)
        {
            return new ViewItemSchedule(dataSchedule);
        }

        private static Func<DataItemMegaItem, bool> ShouldIncludeHomeworkOrExamFunction(Guid classId, DateTime todayAsUtc)
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

        private static Func<BaseViewItemHomeworkExam, bool> ShouldIncludeInNormalHomeworkFunction(DateTime todayAsUtc)
        {
            return i => i is ViewItemHomework && ((i as ViewItemHomework).PercentComplete < 1 || i.Date >= todayAsUtc);
        }

        private static Func<DataItemMegaItem, bool> IsPastCompletedHomeworkOrExamFunction(Guid classId, DateTime todayAsUtc)
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

        private static Func<DataItemMegaItem, bool> IsPastCompletedHomeworkFunction(Guid classId, DateTime todayAsUtc)
        {
            return i =>
                (i.MegaItemType == PowerPlannerSending.MegaItemType.Homework
                && i.UpperIdentifier == classId
                && (i.PercentComplete >= 1 && !(i.Date >= todayAsUtc))); // Negate that last date operation since we need to ignore the seconds on the date
        }

        private static Func<DataItemMegaItem, bool> IsPastCompletedExamFunction(Guid classId, DateTime todayAsUtc)
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

        private static Func<BaseViewItemHomeworkExam, bool> ShouldIncludeInNormalExamsFunction()
        {
            return i => i is ViewItemExam && !i.IsComplete();
        }

        private ViewItemWeightCategory CreateWeight(DataItemWeightCategory dataWeight)
        {
            return new ViewItemWeightCategory(
                dataWeight,
                createGradeMethod: IsGradesLoaded ? new Func<BaseDataItemHomeworkExamGrade, BaseViewItemHomeworkExamGrade>(ViewItemWeightCategory.CreateGradeHelper) : null);
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
                if (Class.HomeworkAndExams != null)
                {
                    foreach (var edited in e.EditedItems.OfType<DataItemMegaItem>())
                    {
                        var matched = Class.HomeworkAndExams.FirstOrDefault(i => i.Identifier == edited.Identifier);

                        // If found matching
                        if (matched != null)
                        {
                            // If no longer under this class, we need to re-assign the class
                            if (matched is ViewItemHomework)
                            {
                                var h = matched as ViewItemHomework;
                                if (h.Class.Identifier != edited.UpperIdentifier)
                                {
                                    if (edited.UpperIdentifier == _semester.NoClassClass.Identifier)
                                    {
                                        h.Class = _semester.NoClassClass;
                                    }
                                    else
                                    {
                                        h.Class = _semester.Classes.FirstOrDefault(i => i.Identifier == edited.UpperIdentifier);
                                    }
                                }
                            }
                            else if (matched is ViewItemExam)
                            {
                                var exam = matched as ViewItemExam;
                                if (exam.Class.Identifier != edited.UpperIdentifier)
                                {
                                    if (edited.UpperIdentifier == _semester.NoClassClass.Identifier)
                                    {
                                        exam.Class = _semester.NoClassClass;
                                    }
                                    else
                                    {
                                        exam.Class = _semester.Classes.FirstOrDefault(i => i.Identifier == edited.UpperIdentifier);
                                    }
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
                if (!HasPastCompletedHomework)
                {
                    bool hasPastCompleted = e.EditedItems.Concat(e.NewItems).OfType<DataItemMegaItem>().Any(
                        h => IsPastCompletedHomeworkFunction(_classId, TodayAsUtc).Invoke(h));

                    if (hasPastCompleted)
                    {
                        HasPastCompletedHomework = true;
                    }
                }

                if (!HasPastCompletedExams)
                {
                    bool hasPastCompleted = e.EditedItems.Concat(e.NewItems).OfType<DataItemMegaItem>().Any(
                        exam => IsPastCompletedExamFunction(_classId, TodayAsUtc).Invoke(exam));

                    if (hasPastCompleted)
                    {
                        HasPastCompletedExams = true;
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
                            BaseViewItemHomeworkExam newViewItem;
                            if (newItem.MegaItemType == PowerPlannerSending.MegaItemType.Homework)
                            {
                                newViewItem = new ViewItemHomework(newItem)
                                {
                                    Class = this.Class,
                                    WeightCategory = ViewItemWeightCategory.UNASSIGNED
                                };
                            }
                            else if (newItem.MegaItemType == PowerPlannerSending.MegaItemType.Exam)
                            {
                                newViewItem = new ViewItemExam(newItem)
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

        private bool _hasLoadedPastCompletedHomeworkAndExams;
        private async void LoadPastCompletedHomeworkAndExams()
        {
            if (_hasLoadedPastCompletedHomeworkAndExams)
            {
                return;
            }

            _hasLoadedPastCompletedHomeworkAndExams = true;

            try
            {
                var dataStore = await GetDataStore();

                DataItemMegaItem[] additionalHomeworkAndExams;

                using (await Locks.LockDataForReadAsync())
                {
                    // Get the data items that we haven't loaded yet
                    additionalHomeworkAndExams = dataStore.TableMegaItems.Where(IsPastCompletedHomeworkOrExamFunction(_classId, TodayAsUtc)).ToArray();

                    // Exclude any that are already loaded (due to the events being complicated to calculate whether they're incomplete, we end up double loading items that are on today)
                    additionalHomeworkAndExams = additionalHomeworkAndExams.Where(a => !this.Class.HomeworkAndExams.Any(i => i.Identifier == a.Identifier)).ToArray();

                    // And then update the child function so that we include all homework for the class, and inject the new items
                    this.Class.UpdateIsChildMethod<DataItemMegaItem, BaseViewItemHomeworkExam>(i => i.UpperIdentifier == _classId && i.MegaItemType == PowerPlannerSending.MegaItemType.Homework || i.MegaItemType == PowerPlannerSending.MegaItemType.Exam, additionalHomeworkAndExams);
                }

                // Include the opposite of the other function
                PastCompletedHomework = new PastCompletedHomeworkList(this.Class.HomeworkAndExams.OfTypeObservable<ViewItemHomework>(), TodayAsUtc);
                PastCompletedExams = new PastCompletedExamsList(this.Class.HomeworkAndExams.OfTypeObservable<ViewItemExam>(), TodayAsUtc);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private bool _isPastCompletedHomeworkDisplayed;
        public bool IsPastCompletedHomeworkDisplayed
        {
            get { return _isPastCompletedHomeworkDisplayed; }
            private set { SetProperty(ref _isPastCompletedHomeworkDisplayed, value, "IsPastCompletedHomeworkDisplayed"); }
        }

        public void ShowPastCompletedHomework()
        {
            IsPastCompletedHomeworkDisplayed = true;
            LoadPastCompletedHomeworkAndExams();
        }

        public void HidePastCompletedHomework()
        {
            IsPastCompletedHomeworkDisplayed = false;
        }

        private bool _isPastCompletedExamsDisplayed;
        public bool IsPastCompletedExamsDisplayed
        {
            get { return _isPastCompletedExamsDisplayed; }
            private set { SetProperty(ref _isPastCompletedExamsDisplayed, value, "IsPastCompletedExamsDisplayed"); }
        }

        public void ShowPastCompletedExams()
        {
            IsPastCompletedExamsDisplayed = true;
            LoadPastCompletedHomeworkAndExams();
        }

        public void HidePastCompletedExams()
        {
            IsPastCompletedExamsDisplayed = false;
        }

        public bool HasPastCompletedHomework { get; set; }

        public bool HasPastCompletedExams { get; set; }

        private class PastCompletedExamsList : MyObservableList<ViewItemExam>
        {
            public PastCompletedExamsList(IMyObservableReadOnlyList<ViewItemExam> sourceList, DateTime todayAsUtc)
            {
                base.Filter = new FilterUsingFunction(i => !ShouldIncludeInNormalExamsFunction().Invoke(i));
                base.Comparer = new PastCompletedExamsComparer();
                base.InsertSorted(sourceList);
            }

            private class PastCompletedExamsComparer : IComparer<ViewItemExam>
            {
                public int Compare(ViewItemExam x, ViewItemExam y)
                {
                    // Show in reverse order
                    return x.CompareTo(y) * -1;
                }
            }
        }

        private class PastCompletedHomeworkList : MyObservableList<ViewItemHomework>
        {
            public PastCompletedHomeworkList(IMyObservableReadOnlyList<ViewItemHomework> sourceList, DateTime todayAsUtc)
            {
                base.Filter = new FilterUsingFunction(i => !ShouldIncludeInNormalHomeworkFunction(todayAsUtc).Invoke(i));
                base.Comparer = new PastCompletedHomeworkComparer();
                base.InsertSorted(sourceList);
            }

            private class PastCompletedHomeworkComparer : IComparer<ViewItemHomework>
            {
                public int Compare(ViewItemHomework x, ViewItemHomework y)
                {
                    // Show in reverse order
                    return x.CompareTo(y) * -1;
                }
            }
        }
    }
}