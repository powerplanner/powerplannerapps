using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using Windows.ApplicationModel.Appointments;
using static PowerPlannerAppDataLibrary.Extensions.AppointmentsExtension;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.Exceptions;

namespace PowerPlannerUWPLibrary.Extensions
{


    internal static class AppointmentsHelperExtensions
    {
        public static IEnumerable<T> WhereForAccount<T>(this IEnumerable<T> items, AccountDataItem account) where T : AppointmentsHelper
        {
            return items.Where(i => i.Account.LocalAccountId == account.LocalAccountId);
        }
    }

    public class UWPAppointmentsExtension : AppointmentsExtension
    {
        public override Task DeleteAsync(Guid localAccountId)
        {
            return AppointmentsHelper.DeleteAccount(localAccountId);
        }

        public override Task GetTaskForAllCompleted()
        {
            return AppointmentsHelper.GetTaskForAllCompleted();
        }

        public override void ResetAll(AccountDataItem account, AccountDataStore dataStore)
        {
            AppointmentsHelper.ResetAll(account, dataStore);
        }

        public override void ResetAllIfNeeded(AccountDataItem account, AccountDataStore dataStore)
        {
            AppointmentsHelper.ResetAllIfNeeded(account, dataStore);
        }

        public override UpdateResponse Update(AccountDataItem account, AccountDataStore dataStore, DataChangedEvent dataChangedEvent)
        {
            // If we deleted or edited a holiday, we just need to reset everything, since the holiday deletes a recurring
            // instance, and there's no way to restore the series to its original without the deleted recurrence
            if (!account.IsClassesCalendarIntegrationDisabled && (dataChangedEvent.DeletedItems.DidDeleteHoliday || dataChangedEvent.EditedItems.OfType<DataItemMegaItem>().Any(i => i.MegaItemType == PowerPlannerSending.MegaItemType.Holiday)))
            {
                ResetAll(account, dataStore);
                return new UpdateResponse();
            }

            return AppointmentsHelper.Update(account, dataStore, dataChangedEvent);
        }
    }


    public abstract class AppointmentsHelper
    {
        private class AppointmentsResetHelper : AppointmentsHelper
        {
            internal AppointmentsResetHelper(AccountDataItem account, AccountDataStore dataStore) : base(account, dataStore) { }

            internal override Task<bool> ActuallyStartAsync()
            {
                return ResetAllAsync();
            }

            private async Task<bool> ResetAllAsync()
            {
                try
                {
                    ThrowIfCanceled();

                    // We're on a separate thread, so can use blocking calls
                    AllData allData = await this.LoadAllDataBlocking();

                    ThrowIfCanceled();

                    // If the tasks calendar is enabled
                    if (this.IsTasksEnabled)
                        await ResetTasksCalendarAsync(allData);

                    ThrowIfCanceled();

                    // If the classes calendar is enabled
                    if (this.IsClassesEnabled)
                        await ResetClassesCalendarAsync(allData);

                    ThrowIfCanceled();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                    return false;
                }

                return true;
            }


            private async Task ResetTasksCalendarAsync(AllData allData)
            {
                // Clear all
                await ClearCalendarAsync(this.TasksCalendar);

                ThrowIfCanceled();

                if (allData?.Semester != null)
                {
                    await AddNewTasks(allData.Classes, allData.Tasks, allData.Semester.NoClassClass);
                }
            }

            private async Task ResetClassesCalendarAsync(AllData allData)
            {
                // Clear all
                await ClearCalendarAsync(this.ClassesCalendar);

                ThrowIfCanceled();

                if (allData?.Semester != null)
                {
                    await AddNewSchedules(allData.Semester, allData.Classes, allData.Schedules);
                    await AddNewHolidays(allData.Holidays);
                }
            }

            private class AllData
            {
                public ViewItemSemester Semester;
                public Dictionary<Guid, ViewItemClass> Classes = new Dictionary<Guid, ViewItemClass>();
                public List<DataItemMegaItem> Tasks = new List<DataItemMegaItem>();
                public List<ViewItemSchedule> Schedules = new List<ViewItemSchedule>();
                public List<DataItemMegaItem> Holidays = new List<DataItemMegaItem>();
            }

            private async Task<AllData> LoadAllDataBlocking()
            {
                AllData answer = new AllData();

                if (Account.CurrentSemesterId != Guid.Empty)
                {
                    // We need all class times loaded for tasks, to calculate things like "End of day", so just use the view items.
                    // They might be cached anyways.
                    ScheduleViewItemsGroup scheduleViewItemsGroup = null;
                    try
                    {
                        scheduleViewItemsGroup = await ScheduleViewItemsGroup.LoadAsync(Account.LocalAccountId, Account.CurrentSemesterId, trackChanges: true, includeWeightCategories: false);
                    }
                    catch (SemesterNotFoundException) { }

                    if (scheduleViewItemsGroup != null)
                    {
                        // Grab necessary items under a lock
                        using (await scheduleViewItemsGroup.DataChangeLock.LockForReadAsync())
                        {
                            ThrowIfCanceled();

                            answer.Semester = scheduleViewItemsGroup.Semester;
                            foreach (var c in scheduleViewItemsGroup.Classes)
                            {
                                answer.Classes[c.Identifier] = c;
                            }

                            answer.Schedules.AddRange(scheduleViewItemsGroup.Classes.SelectMany(i => i.Schedules));

                            ThrowIfCanceled();
                        }

                        using (await Locks.LockDataForReadAsync())
                        {
                            ThrowIfCanceled();

                            var currSemesterId = answer.Semester.Identifier;
                            Guid[] classIdsForCurrSemester = answer.Semester.Classes.Select(i => i.Identifier).ToArray();

                            ThrowIfCanceled();

                            // If tasks is enabled
                            if (this.IsTasksEnabled)
                            {
                                // TODO: Not loading tasks/events
                                answer.Tasks.AddRange(DataStore.TableMegaItems.Where(i =>

                                    // Homework/exams
                                    ((i.MegaItemType == PowerPlannerSending.MegaItemType.Homework || i.MegaItemType == PowerPlannerSending.MegaItemType.Exam)
                                    && classIdsForCurrSemester.Contains(i.UpperIdentifier))

                                    ||

                                    // Tasks/events
                                    ((i.MegaItemType == PowerPlannerSending.MegaItemType.Task || i.MegaItemType == PowerPlannerSending.MegaItemType.Event)
                                    && i.UpperIdentifier == currSemesterId)

                                    ));
                                ThrowIfCanceled();
                            }

                            // If classes is enabled
                            if (this.IsClassesEnabled && answer.Semester != null)
                            {
                                // Select the schedules for this semester
                                answer.Holidays.AddRange(DataStore.TableMegaItems.Where(i =>
                                    i.MegaItemType == PowerPlannerSending.MegaItemType.Holiday
                                    && i.UpperIdentifier == currSemesterId));
                            }
                        }
                    }
                }

                return answer;
            }
        }

        private async Task AddNewTasks(Dictionary<Guid, ViewItemClass> classes, IEnumerable<DataItemMegaItem> tasks, ViewItemClass noClassClass)
        {
            foreach (var t in tasks)
            {
                ThrowIfCanceled();

                await AddNewTask(classes, t, noClassClass);
            }
        }

        private readonly Dictionary<Guid, string> _megaItemLocalIdsToSave = new Dictionary<Guid, string>();
        private readonly Dictionary<Guid, string> _scheduleLocalIdsToSave = new Dictionary<Guid, string>();

        private async Task AddNewTask(Dictionary<Guid, ViewItemClass> classes, DataItemMegaItem task, ViewItemClass noClassClass)
        {

            if (classes.TryGetValue(task.UpperIdentifier, out ViewItemClass c))
            {
                // Nothing, obtained class
            }

            else if (task.UpperIdentifier == noClassClass.Identifier)
            {
                c = noClassClass;
            }

            // Then if we found the class, add the item
            if (c != null)
            {
                await AddNewTask(c, task, classes.Values, noClassClass);
            }
        }

        private async Task AddNewTask(ViewItemClass c, DataItemMegaItem task, IEnumerable<ViewItemClass> classes, ViewItemClass noClassClass)
        {
            try
            {
                Appointment a = new Appointment()
                {
                    Uri = new Uri("powerplanner:?" + ArgumentsHelper.CreateArgumentsForView(task, this.Account.LocalAccountId, LaunchSurface.Calendar).SerializeToString()),
                    RoamingId = GenerateAppointmentRoamingId(task)
                };

                PopulateAppointment(a, task, c, classes, noClassClass);

                await TasksCalendar.SaveAppointmentAsync(a);

                task.AppointmentLocalId = a.LocalId;

                _megaItemLocalIdsToSave[task.Identifier] = a.LocalId;
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async Task AddNewSchedules(ViewItemSemester semester, Dictionary<Guid, ViewItemClass> classes, IEnumerable<ViewItemSchedule> schedules)
        {
            foreach (var s in schedules.Where(i => (i.DataItem as DataItemSchedule).AreTimesValid()))
            {
                ThrowIfCanceled();

                await AddNewSchedule(semester, classes, s);
            }
        }

        private async Task AddNewSchedule(ViewItemSemester semester, Dictionary<Guid, ViewItemClass> classes, ViewItemSchedule schedule)
        {
            if (classes.TryGetValue(schedule.Class.Identifier, out ViewItemClass c))
            {
                await AddNewSchedule(semester, c, schedule);
            }
        }

        private async Task AddNewSchedule(ViewItemSemester semester, ViewItemClass c, ViewItemSchedule schedule)
        {
            try
            {
                Appointment a = new Appointment()
                {
                    Uri = new Uri("powerplanner:?" + new ViewScheduleArguments()
                    {
                        LocalAccountId = this.Account.LocalAccountId,
                        LaunchSurface = LaunchSurface.Calendar
                    }.SerializeToString()),
                    RoamingId = GenerateAppointmentRoamingId(schedule.DataItem)
                };

                if (!PopulateAppointment(a, schedule, c, semester))
                {
                    // Invalid appointment, don't add
                    return;
                }

                await ClassesCalendar.SaveAppointmentAsync(a);

                (schedule.DataItem as DataItemSchedule).AppointmentLocalId = a.LocalId;

                _scheduleLocalIdsToSave[schedule.Identifier] = a.LocalId;
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async Task AddNewHolidays(IEnumerable<DataItemMegaItem> holidays)
        {
            foreach (var h in holidays)
            {
                await AddNewHoliday(h);
            }
        }

        private async Task AddNewHoliday(DataItemMegaItem holiday)
        {
            Appointment a = new Appointment()
            {
                Uri = new Uri("powerplanner:?" + new ViewHolidayArguments()
                {
                    LocalAccountId = this.Account.LocalAccountId,
                    ItemId = holiday.Identifier,
                    LaunchSurface = LaunchSurface.Calendar
                }.SerializeToString()),
                RoamingId = GenerateAppointmentRoamingId(holiday)
            };

            PopulateHoliday(a, holiday);

            var appointments = await ClassesCalendar.FindAppointmentsAsync(a.StartTime, a.Duration);
            foreach (var other in appointments)
            {
                await ClassesCalendar.DeleteAppointmentInstanceAsync(other.LocalId, other.StartTime);
            }

            await ClassesCalendar.SaveAppointmentAsync(a);

            holiday.AppointmentLocalId = a.LocalId;

            _megaItemLocalIdsToSave[holiday.Identifier] = a.LocalId;
        }

        private class AppointmentsUpdateHelper : AppointmentsHelper
        {
            internal AppointmentsUpdateHelper(AccountDataItem account, AccountDataStore dataStore, DataChangedEvent dataChangedEvent) : base(account, dataStore)
            {
                DataChangedEvent = dataChangedEvent;
            }

            public DataChangedEvent DataChangedEvent
            {
                get; private set;
            }

            internal override async Task<bool> ActuallyStartAsync()
            {
                try
                {
                    // Need to load data like classes and whatnot since we need their names
                    AllData allData = await LoadAllDataBlocking();

                    ThrowIfCanceled();


                    // Delete task appointments
                    if (this.IsTasksEnabled)
                    {
                        if (allData.HasSemester())
                        {
                            foreach (string deletedLocalId in DataChangedEvent.DeletedItems.DeletedTaskEventAppointments)
                            {
                                try
                                {
                                    await TasksCalendar.DeleteAppointmentAsync(deletedLocalId);
                                }

                                catch { }

                                ThrowIfCanceled();
                            }
                        }
                        else
                        {
                            await ClearCalendarAsync(TasksCalendar);

                            ThrowIfCanceled();
                        }
                    }

                    // Delete class appointments
                    if (this.IsClassesEnabled)
                    {
                        // If there's no semester, we delete all
                        if (allData.Semester == null)
                            await ClearCalendarAsync(ClassesCalendar);

                        else
                        {
                            foreach (string deletedLocalId in DataChangedEvent.DeletedItems.DeletedScheduleAppointments)
                            {
                                try
                                {
                                    await ClassesCalendar.DeleteAppointmentAsync(deletedLocalId);
                                }

                                catch { }

                                ThrowIfCanceled();
                            }
                        }
                    }

                    var classIds = allData.Classes.Select(i => i.Value.Identifier).ToArray();

                    // Add new ones
                    if (this.IsTasksEnabled && allData.HasSemester())
                        await AddNewTasks(allData.Classes, DataChangedEvent.NewItems.OfType<DataItemMegaItem>().Where(i =>

                            // Homework/exams
                            ((i.MegaItemType == PowerPlannerSending.MegaItemType.Exam || i.MegaItemType == PowerPlannerSending.MegaItemType.Homework)
                            && classIds.Contains(i.UpperIdentifier))

                            ||

                            // Tasks/events
                            ((i.MegaItemType == PowerPlannerSending.MegaItemType.Task || i.MegaItemType == PowerPlannerSending.MegaItemType.Event)
                            && i.UpperIdentifier == allData.Semester.Identifier)
                            
                            ), allData.Semester.NoClassClass);

                    ThrowIfCanceled();

                    if (this.IsClassesEnabled && allData.Semester != null)
                    {
                        Guid[] newScheduleIdentifiers = DataChangedEvent.NewItems.OfType<DataItemSchedule>().Select(i => i.Identifier).ToArray();
                        var newSchedules = allData.Classes.Values.SelectMany(i => i.Schedules).Where(i => newScheduleIdentifiers.Contains(i.Identifier)).ToArray();
                        await AddNewSchedules(allData.Semester, allData.Classes, newSchedules);
                        await AddNewHolidays(DataChangedEvent.NewItems.OfType<DataItemMegaItem>().Where(i => i.MegaItemType == PowerPlannerSending.MegaItemType.Holiday));
                    }



                    ThrowIfCanceled();


                    // And then update the edited items
                    if (this.IsTasksEnabled && allData.HasSemester())
                    {
                        foreach (var edited in allData.TasksWithEditedParents.Union(DataChangedEvent.EditedItems.OfType<DataItemMegaItem>()))
                        {
                            ThrowIfCanceled();

                            // Skip any tasks that are also new (if a new task was created and the parent class was edited at the same time, the new task would appear
                            // both in this "edited parents" list, and the new items list from earlier, causing a duplicate to be added!
                            if (DataChangedEvent.NewItems.OfType<DataItemMegaItem>().Any(i => i.Identifier == edited.Identifier))
                            {
                                continue;
                            }

                            Appointment existing = null;

                            if (edited.AppointmentLocalId != null)
                            {
                                try
                                {
                                    existing = await TasksCalendar.GetAppointmentAsync(edited.AppointmentLocalId);
                                }

                                catch { }

                                ThrowIfCanceled();
                            }

                            if (existing == null)
                                await AddNewTask(allData.Classes, edited, allData.Semester.NoClassClass);

                            else
                            {
                                ViewItemClass c = GetClassOrNull(edited, allData.Classes.Values, allData.Semester.NoClassClass);

                                // If we found the class, we can update it
                                if (c != null)
                                {
                                    PopulateAppointment(existing, edited, c, allData.Classes.Values, allData.Semester.NoClassClass);
                                    await TasksCalendar.SaveAppointmentAsync(existing);
                                }

                                // Otherwise, we should delete it
                                else
                                {
                                    await TasksCalendar.DeleteAppointmentAsync(existing.LocalId);
                                }
                            }
                        }

                        ThrowIfCanceled();
                    }


                    if (this.IsClassesEnabled && allData.Semester != null)
                    {
                        var editedScheduleIdentifiers = allData.SchedulesWithEditedParents.Select(i => i.Identifier).Union(DataChangedEvent.EditedItems.OfType<DataItemSchedule>().Select(i => i.Identifier)).ToArray();
                        foreach (var edited in allData.Classes.Values.SelectMany(i => i.Schedules).Where(i => editedScheduleIdentifiers.Contains(i.Identifier)))
                        {
                            ThrowIfCanceled();

                            Appointment existing = null;

                            if (edited.DataItem.AppointmentLocalId != null)
                            {
                                try
                                {
                                    existing = await ClassesCalendar.GetAppointmentAsync(edited.DataItem.AppointmentLocalId);
                                }

                                catch { }

                                ThrowIfCanceled();
                            }

                            if (existing == null)
                            {
                                if (edited.DataItem.AreTimesValid())
                                {
                                    await AddNewSchedule(allData.Semester, allData.Classes, edited);
                                }
                            }

                            else
                            {
                                // If we found the class, we can update it
                                if (edited.DataItem.AreTimesValid()
                                    && PopulateAppointment(existing, edited, edited.Class, allData.Semester))
                                {
                                    await ClassesCalendar.SaveAppointmentAsync(existing);
                                }

                                // Otherwise, we should delete it
                                else
                                {
                                    await ClassesCalendar.DeleteAppointmentAsync(existing.LocalId);
                                }
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    TelemetryExtension.Current?.TrackException(e);
                    return false;
                }

                return true;
            }

            private static ViewItemClass GetClassOrNull(DataItemMegaItem item, IEnumerable<ViewItemClass> classes, ViewItemClass noClassClass)
            {
                if (item.MegaItemType == PowerPlannerSending.MegaItemType.Task || item.MegaItemType == PowerPlannerSending.MegaItemType.Event)
                {
                    if (item.UpperIdentifier == noClassClass.Identifier)
                    {
                        return noClassClass;
                    }
                    else
                    {
                        return null;
                    }
                }

                return classes.FirstOrDefault(i => item.UpperIdentifier == i.Identifier);
            }

            internal void MergeDataChangedEvent(DataChangedEvent newerEvent)
            {
                this.DataChangedEvent.Merge(newerEvent);
            }

            private class AllData
            {
                public ViewItemSemester Semester;

                public Dictionary<Guid, ViewItemClass> Classes = new Dictionary<Guid, ViewItemClass>();

                public List<DataItemMegaItem> TasksWithEditedParents = new List<DataItemMegaItem>();

                public List<ViewItemSchedule> SchedulesWithEditedParents = new List<ViewItemSchedule>();

                public bool HasSemester()
                {
                    return Semester != null;
                }
            }

            private async Task<AllData> LoadAllDataBlocking()
            {
                AllData answer = new AllData();

                if (Account.CurrentSemesterId != Guid.Empty)
                {
                    ScheduleViewItemsGroup scheduleViewItemsGroup = null;
                    try
                    {
                        scheduleViewItemsGroup = await ScheduleViewItemsGroup.LoadAsync(Account.LocalAccountId, Account.CurrentSemesterId, trackChanges: true, includeWeightCategories: false);
                    }
                    catch (SemesterNotFoundException) { }

                    if (scheduleViewItemsGroup != null)
                    {
                        // Obtain necessary items under a lock
                        using (await scheduleViewItemsGroup.DataChangeLock.LockForReadAsync())
                        {
                            ThrowIfCanceled();

                            answer.Semester = scheduleViewItemsGroup.Semester;
                            foreach (var c in scheduleViewItemsGroup.Classes)
                            {
                                answer.Classes[c.Identifier] = c;
                            }
                        }

                        using (await Locks.LockDataForReadAsync())
                        {
                            ThrowIfCanceled();

                            var newAndEditedItems = DataChangedEvent.EditedItems.Concat(DataChangedEvent.NewItems);

                            // Classes that were edited will require loading and updating tasks/events/schedules, since the class name might have been edited
                            Guid[] classIdentifiersNeedingChildrenLoaded = DataChangedEvent.EditedItems.OfType<DataItemClass>()
                                .Select(i => i.Identifier)
                                .Except(DataChangedEvent.EditedClassIdentifiersToIgnoreFromCalendarIntegration)
                                .ToArray();

                            // Now we need to load those classes and children of edited classes
                            if (IsClassesEnabled || classIdentifiersNeedingChildrenLoaded.Length > 0)
                            {
                                ThrowIfCanceled();

                                if (this.IsTasksEnabled && classIdentifiersNeedingChildrenLoaded.Length > 0)
                                {
                                    answer.TasksWithEditedParents.AddRange(DataStore.TableMegaItems.Where(i =>
                                        (i.MegaItemType == PowerPlannerSending.MegaItemType.Homework || i.MegaItemType == PowerPlannerSending.MegaItemType.Exam)
                                        && classIdentifiersNeedingChildrenLoaded.Contains(i.UpperIdentifier)));
                                    ThrowIfCanceled();
                                }

                                if (this.IsClassesEnabled)
                                {
                                    Guid[] affectedClassesForSchedule;

                                    // If the semester was edited, we have to load all its children classes
                                    if (newAndEditedItems.OfType<DataItemSemester>().Any(i => i.Identifier == Account.CurrentSemesterId))
                                    {
                                        Guid[] classIdentifiersInSemester = answer.Classes.Values.Select(i => i.Identifier).ToArray();

                                        affectedClassesForSchedule = classIdentifiersInSemester.Union(classIdentifiersNeedingChildrenLoaded).ToArray();
                                    }

                                    else
                                        affectedClassesForSchedule = classIdentifiersNeedingChildrenLoaded;

                                    if (affectedClassesForSchedule.Length > 0)
                                        answer.SchedulesWithEditedParents.AddRange(answer.Classes.Values.Where(i => affectedClassesForSchedule.Contains(i.Identifier)).SelectMany(i => i.Schedules));

                                    ThrowIfCanceled();
                                }
                            }
                        }
                    }
                }

                return answer;
            }
        }

        private AppointmentsHelper(AccountDataItem account, AccountDataStore dataStore)
        {
            Account = account;
            DataStore = dataStore;
        }

        private void MarkCanceled()
        {
            lock (_lock)
            {
                _activeHelpers.Remove(this);

                // We need to start next
                _ = StartNextHelper();
            }
        }

        private void UpdateAppointmentLocalIds(string table, Dictionary<Guid, string> localIdsToSave)
        {
            foreach (var pair in localIdsToSave)
            {
                DataStore._db.Execute($"update {table} set AppointmentLocalId = ? where Identifier = ?", pair.Value, pair.Key);
            }
        }

        private async Task MarkFinished()
        {
            bool needsToSaveAccount = false;

            // If we need to store the local ID's
            if (_megaItemLocalIdsToSave.Count > 0
                || _scheduleLocalIdsToSave.Count > 0)
            {
                using (await Locks.LockDataForWriteAsync(callerName: "UWPAppointmentsExtension.MarkFinished", customMessage: delegate
                {
                    return $"Number of items and schedules to save were {_megaItemLocalIdsToSave.Count} and {_scheduleLocalIdsToSave.Count}, respectively.";
                }))
                {
                    DataStore._db.RunInTransaction(delegate
                    {
                        UpdateAppointmentLocalIds(DataStore.ActualTableMegaItems.Table.TableName, _megaItemLocalIdsToSave);
                        UpdateAppointmentLocalIds(DataStore.ActualTableSchedules.Table.TableName, _scheduleLocalIdsToSave);
                    });
                }
            }

            lock (_lock)
            {
                _activeHelpers.Remove(this);

                // If there's no others for this account
                if (!_activeHelpers.WhereForAccount(Account).Any())
                {
                    // Then it's all up to date
                    Account.IsAppointmentsUpToDate = true;
                    needsToSaveAccount = true;
                }

                // We need to start next
                var dontWait = StartNextHelper();
            }

            if (needsToSaveAccount)
                await AccountsManager.Save(Account);
        }

        /// <summary>
        /// This must be called from within lock
        /// </summary>
        private static void CompleteTaskThatWaitsForAll()
        {
            if (_taskForAllCompleted != null)
            {
                _taskForAllCompleted.SetResult(true);
                _taskForAllCompleted = null;
            }
        }

        internal abstract Task<bool> ActuallyStartAsync();

        internal async Task StartAsync()
        {
            try
            {
                ThrowIfCanceled();

                await Task.Run(async delegate
                {
                    ThrowIfCanceled();

                    if (!await InitializeAsync())
                    {
                        MarkFailure();
                        return;
                    }

                    ThrowIfCanceled();

                    // As long as one is enabled, do the task
                    if (this.IsTasksEnabled || this.IsClassesEnabled)
                    {
                        if (!(await ActuallyStartAsync()))
                        {
                            MarkFailure();
                            return;
                        }

                        ThrowIfCanceled();
                    }

                    // Mark completed, remove from list, save account, start next
                    await this.MarkFinished();
                });
            }

            catch (OperationCanceledException)
            {
                this.MarkCanceled();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);

                MarkFailure();
            }
        }

        private void MarkFailure()
        {
            lock (_lock)
            {
                _activeHelpers.Clear();

                CompleteTaskThatWaitsForAll();
            }
        }

        public AccountDataItem Account { get; private set; }
        public AccountDataStore DataStore { get; private set; }

        public AppointmentCalendar TasksCalendar { get; private set; }
        public AppointmentCalendar ClassesCalendar { get; private set; }

        public bool IsTasksEnabled
        {
            get { return TasksCalendar != null; }
        }

        public bool IsClassesEnabled
        {
            get { return ClassesCalendar != null; }
        }



        public class RoamingIdData
        {
            public Guid LocalAccountId { get; set; }
            public Guid Identifier { get; set; }
            public PowerPlannerSending.ItemType ItemType { get; set; }

            public RoamingIdData(Guid localAccountId, PowerPlannerSending.ItemType itemType, Guid identifier)
            {
                LocalAccountId = localAccountId;
                ItemType = itemType;
                Identifier = identifier;
            }


            public static RoamingIdData FromString(string s)
            {
                string[] vals = s.Split(':');

                return new RoamingIdData(
                    localAccountId: Guid.Parse(vals[0]),
                    itemType: (PowerPlannerSending.ItemType)Enum.Parse(typeof(PowerPlannerSending.ItemType), vals[1]),
                    identifier: Guid.Parse(vals[2]));
            }
        }

        private string GenerateAppointmentRoamingId(BaseDataItem item)
        {
            return Account.LocalAccountId.ToString() + ":" + item.ItemType.ToString() + ":" + item.Identifier;
        }

        private static void PopulateAppointment(Appointment appointment, DataItemMegaItem task, ViewItemClass c, IEnumerable<ViewItemClass> classes, ViewItemClass noClassClass)
        {
            appointment.Subject = task.Name;
            appointment.Location = c.Name; // Strings that are too long will automatically be trimmed
                                           // No need for details, since when clicked, it just launches Power Planner
                                           //appointment.Details = task.Details;

            var viewItemTask = task.CreateViewItemTaskOrEvent(classes, noClassClass);

            if (viewItemTask.TryGetDueDateWithTime(out DateTime startTime))
            {
                appointment.StartTime = startTime;
                appointment.AllDay = false;

                if (viewItemTask.TryGetEndDateWithTime(out DateTime endTime) && endTime > startTime)
                {
                    appointment.Duration = endTime - startTime;
                    appointment.BusyStatus = AppointmentBusyStatus.Busy;
                }
                else
                {
                    // We need to include a duration, otherwise if item starts as all day and then gets edited to a time WITHOUT assigning Duration,
                    // the item still gets displayed in the all day section and bugs in the Calendar app occur
                    appointment.Duration = TimeSpan.FromMinutes(1);
                    appointment.BusyStatus = AppointmentBusyStatus.Free;
                }
            }
            else
            {
                appointment.StartTime = DateTime.SpecifyKind(task.Date, DateTimeKind.Local);
                appointment.AllDay = true;
                appointment.Duration = new TimeSpan();
                appointment.BusyStatus = AppointmentBusyStatus.Free;
            }
        }

        private bool PopulateAppointment(Appointment appointment, ViewItemSchedule schedule, ViewItemClass c, ViewItemSemester semester)
        {
            // If class not in current semester, return false
            if (c.Semester.Identifier != semester.Identifier)
            {
                return false;
            }

            appointment.Subject = c.Name;
            appointment.Location = schedule.Room; // Strings that are too long will automatically be trimmed
                                                  // No need for details, since when clicked, it just launches Power Planner
                                                  //appointment.Details = task.Details;

            appointment.StartTime = (schedule.DataItem as DataItemSchedule).GetLocalStartDateAndTime(Account, (semester.DataItem as DataItemSemester), (c.DataItem as DataItemClass));
            appointment.Duration = schedule.EndTimeInSchoolTime.TimeOfDay - schedule.StartTimeInSchoolTime.TimeOfDay;
            appointment.BusyStatus = AppointmentBusyStatus.Busy;

            appointment.Recurrence = new AppointmentRecurrence()
            {
                Interval = schedule.ScheduleWeek == PowerPlannerSending.Schedule.Week.BothWeeks ? 1u : 2u,
                Unit = AppointmentRecurrenceUnit.Weekly,
                DaysOfWeek = ConvertToAppointmentDayOfWeek(schedule.DayOfWeek)
            };

            DateTime? endDate = null;

            // If semester has an end date
            var semesterEnd = semester.End;
            if (!PowerPlannerSending.DateValues.IsUnassigned(semesterEnd))
            {
                endDate = semesterEnd;
            }

            // If class has an end date, that one wins over anything else
            var classEnd = c.EndDate;
            if (!PowerPlannerSending.DateValues.IsUnassigned(classEnd))
            {
                endDate = classEnd;
            }

            if (endDate != null)
            {
                appointment.Recurrence.Until = endDate.Value;
                if (appointment.Recurrence.Until.Value.Date < appointment.StartTime.Date)
                {
                    // If appointment invalid, return false
                    return false;
                }
            }

            return true;
        }

        private void PopulateHoliday(Appointment appointment, DataItemMegaItem holiday)
        {
            appointment.Subject = holiday.Name;

            appointment.StartTime = DateTime.SpecifyKind(holiday.Date.Date, DateTimeKind.Local);
            appointment.AllDay = true;

            if (holiday.EndTime.Date > holiday.Date.Date)
            {
                // We add 1 day so that it fully includes the last day. Otherwise the last day ends up being skipped in Outlook.
                appointment.Duration = holiday.EndTime.Date.AddDays(1) - holiday.Date.Date;
            }
            else
            {
                appointment.Duration = TimeSpan.FromDays(1);
            }

            appointment.BusyStatus = AppointmentBusyStatus.OutOfOffice;
        }

        private static AppointmentDaysOfWeek ConvertToAppointmentDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return AppointmentDaysOfWeek.Sunday;

                case DayOfWeek.Monday:
                    return AppointmentDaysOfWeek.Monday;

                case DayOfWeek.Tuesday:
                    return AppointmentDaysOfWeek.Tuesday;

                case DayOfWeek.Wednesday:
                    return AppointmentDaysOfWeek.Wednesday;

                case DayOfWeek.Thursday:
                    return AppointmentDaysOfWeek.Thursday;

                case DayOfWeek.Friday:
                    return AppointmentDaysOfWeek.Friday;

                default:
                    return AppointmentDaysOfWeek.Saturday;
            }
        }

        private async Task ClearCalendarAsync(AppointmentCalendar cal)
        {
            foreach (var a in await cal.FindUnexpandedAppointmentsAsync())
            {
                ThrowIfCanceled();

                await cal.DeleteAppointmentAsync(a.LocalId);
            }
        }

        private void ThrowIfCanceled()
        {
            if (_isCanceled)
                throw new OperationCanceledException();
        }

        /// <summary>
        /// Returns a task that can be awaited till all pending tasks are done.
        /// </summary>
        /// <returns></returns>
        public static Task GetTaskForAllCompleted()
        {
            lock (_lock)
            {
                if (_activeHelpers.Count == 0)
                    return Task.CompletedTask;

                if (_taskForAllCompleted == null)
                    _taskForAllCompleted = new TaskCompletionSource<bool>();

                return _taskForAllCompleted.Task;
            }
        }

        private static TaskCompletionSource<bool> _taskForAllCompleted;
        private static readonly List<AppointmentsHelper> _activeHelpers = new List<AppointmentsHelper>();

        private static readonly object _lock = new object();

        public static void ResetAll(AccountDataItem account, AccountDataStore dataStore)
        {
            // If all are disabled and account is up to date, nothing to do
            if (account.IsAllCalendarIntegrationDisabled() && account.IsAppointmentsUpToDate)
                return;

            lock (_lock)
            {
                if (account.IsAppointmentsUpToDate)
                {
                    account.IsAppointmentsUpToDate = false;
                    _ = AccountsManager.Save(account);
                }

                // If there's a reset in the queue, we're all good
                if (_activeHelpers.Skip(1).OfType<AppointmentsResetHelper>().WhereForAccount(account).Any())
                    return;

                // Grab the current task
                AppointmentsHelper current = _activeHelpers.FirstOrDefault();

                // Add this reset to the queue
                _activeHelpers.Add(new AppointmentsResetHelper(account, dataStore));

                // Remove any queued updates
                foreach (var u in _activeHelpers.Skip(1).OfType<AppointmentsUpdateHelper>().WhereForAccount(account).ToArray())
                    _activeHelpers.Remove(u);

                // If there was a current task
                if (current != null)
                {
                    // If the current task was for this account, cancel it (which automatically starts the next)
                    if (current.Account.LocalAccountId == account.LocalAccountId)
                    {
                        current.Cancel();
                    }

                    // Regardless, the next will start automatically in both cases
                    return;
                }

                // Otherwise, we need to trigger this one to start since it's the only one in the queue
                _ = StartNextHelper();
            }
        }

        public static async Task DeleteAccount(Guid localAccountId)
        {
            lock (_lock)
            {
                // Remove any for this account
                for (int i = 1; i < _activeHelpers.Count; i++)
                {
                    if (_activeHelpers[i].Account.LocalAccountId == localAccountId)
                    {
                        _activeHelpers.RemoveAt(i);
                        i--;
                    }
                }

                // Grab the current task
                AppointmentsHelper current = _activeHelpers.FirstOrDefault();

                // If there was a current task
                if (current != null)
                {
                    // If the current task was for this account, cancel it (which automatically starts the next)
                    if (current.Account.LocalAccountId == localAccountId)
                    {
                        current.Cancel();
                    }
                }
            }

            AppointmentStore store = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AppCalendarsReadWrite);

            var cals = await store.FindAppointmentCalendarsAsync();

            var tasksRemoteId = GenerateTasksCalendarRemoteId(localAccountId);

            var tasksCalendar = cals.FirstOrDefault(i => object.Equals(i.RemoteId, tasksRemoteId));
            if (tasksCalendar != null)
            {
                await tasksCalendar.DeleteAsync();
            }

            var classesRemoteId = GenerateClassesCalendarRemoteId(localAccountId);
            var classesCalendar = cals.FirstOrDefault(i => object.Equals(i.RemoteId, classesRemoteId));
            if (classesCalendar != null)
            {
                await classesCalendar.DeleteAsync();
            }
        }

        public static void ResetAllIfNeeded(AccountDataItem account, AccountDataStore dataStore)
        {
            // If all are disabled and account is up to date, nothing to do
            if (account.IsAllCalendarIntegrationDisabled() && account.IsAppointmentsUpToDate)
                return;

            lock (_lock)
            {
                // If appointments aren't up to date and we don't have an item running or in the queue, we need to reset
                if (!account.IsAppointmentsUpToDate && !_activeHelpers.WhereForAccount(account).Any())
                {
                    ResetAll(account, dataStore);
                }
            }
        }

        public static UpdateResponse Update(AccountDataItem account, AccountDataStore dataStore, DataChangedEvent dataChangedEvent)
        {
            // If all are disabled and account is up to date, nothing to do
            if (account.IsAllCalendarIntegrationDisabled() && account.IsAppointmentsUpToDate)
                return new UpdateResponse();

            UpdateResponse response = new UpdateResponse();

            lock (_lock)
            {
                bool prevIsAppointmentsUpToDate = account.IsAppointmentsUpToDate;

                if (prevIsAppointmentsUpToDate)
                {
                    account.IsAppointmentsUpToDate = false;
                    response.NeedsAccountToBeSaved = true;
                }


                // If there's a reset in the queue, we're all good
                if (_activeHelpers.Skip(1).OfType<AppointmentsResetHelper>().WhereForAccount(account).Any())
                    return response;

                // See if there's an update in the queue
                AppointmentsUpdateHelper queuedUpdate = _activeHelpers.Skip(1).OfType<AppointmentsUpdateHelper>().WhereForAccount(account).FirstOrDefault();

                // If there's an update in the queue, merge items with it, and we're done
                if (queuedUpdate != null)
                {
                    queuedUpdate.MergeDataChangedEvent(dataChangedEvent);
                    return response;
                }


                var current = _activeHelpers.FirstOrDefault();

                // If we don't have an in-progress one
                if (current == null || current.Account.LocalAccountId != account.LocalAccountId)
                {
                    // If appointments previously weren't up to date, we need to do a full reset
                    if (!prevIsAppointmentsUpToDate)
                    {
                        ResetAll(account, dataStore);
                        return response;
                    }
                }

                // If there's an update/reset in progress, that's ok, we just add this update to the queue
                // Otherwise, we're logically left with no updates/resets in the queue, which in that case we also just add this update to the queue

                // Add this to the queue
                _activeHelpers.Add(new AppointmentsUpdateHelper(account, dataStore, dataChangedEvent));

                // If this is the only one in the queue, start it
                if (_activeHelpers.Count == 1)
                {
                    _ = StartNextHelper();
                }
            }

            return response;
        }

        private static Task StartNextHelper()
        {
            var helper = _activeHelpers.FirstOrDefault();

            // If nothing next
            if (helper == null)
            {
                CompleteTaskThatWaitsForAll();
                return Task.CompletedTask;
            }

            else
                return helper.StartAsync();
        }

        private Task<bool> _initializeAsyncTask;

        private Task<bool> InitializeAsync()
        {
            if (_initializeAsyncTask == null)
                _initializeAsyncTask = this.CreateInitializeAsyncTask();

            return _initializeAsyncTask;
        }

        private Task<bool> CreateInitializeAsyncTask()
        {
            return InitializeCalendarsAsync();
        }

        private static string GenerateCalendarRemoteId(Guid localAccountId, string type)
        {
            return localAccountId + ":" + type;
        }

        private static string GenerateTasksCalendarRemoteId(Guid localAccountId)
        {
            return GenerateCalendarRemoteId(localAccountId, "Tasks");
        }

        private string GenerateTasksCalendarRemoteId()
        {
            return GenerateTasksCalendarRemoteId(Account.LocalAccountId);
        }

        private static string GenerateClassesCalendarRemoteId(Guid localAccountId)
        {
            return GenerateCalendarRemoteId(localAccountId, "Classes");
        }

        private string GenerateClassesCalendarRemoteId()
        {
            return GenerateClassesCalendarRemoteId(Account.LocalAccountId);
        }

        private bool _isCanceled;

        private void Cancel()
        {
            _isCanceled = true;
        }

        private async Task<bool> InitializeCalendarsAsync()
        {
            try
            {
                AppointmentStore store = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AppCalendarsReadWrite);

                if (store == null)
                {
                    return false;
                }

                var cals = await store.FindAppointmentCalendarsAsync(FindAppointmentCalendarsOptions.IncludeHidden);

                string tasksCalendarDisplayName = $"{this.Account.Username}'s Tasks";

                var tasksRemoteId = GenerateTasksCalendarRemoteId();

                this.TasksCalendar = cals.FirstOrDefault(i => object.Equals(i.RemoteId, tasksRemoteId));

                // If integration has been disabled
                if (Account.IsTasksCalendarIntegrationDisabled)
                {
                    // Delete calendar if it exists
                    if (this.TasksCalendar != null)
                    {
                        await this.TasksCalendar.DeleteAsync();
                        this.TasksCalendar = null;
                    }
                }

                else
                {
                    if (this.TasksCalendar == null)
                    {
                        this.TasksCalendar = await store.CreateAppointmentCalendarAsync(tasksCalendarDisplayName);
                        this.TasksCalendar.OtherAppReadAccess = AppointmentCalendarOtherAppReadAccess.Full;
                        this.TasksCalendar.OtherAppWriteAccess = AppointmentCalendarOtherAppWriteAccess.None;
                        //this.TasksCalendar.SummaryCardView = AppointmentSummaryCardView.App;
                        this.TasksCalendar.RemoteId = tasksRemoteId;
                        await this.TasksCalendar.SaveAsync();
                    }

                    else
                    {
                        if (!this.TasksCalendar.DisplayName.Equals(tasksCalendarDisplayName))
                        {
                            this.TasksCalendar.DisplayName = tasksCalendarDisplayName;
                            await this.TasksCalendar.SaveAsync();
                        }
                    }
                }


                // Classes calendar

                string classesCalendarDisplayName = $"{this.Account.Username}'s Classes";
                var classesRemoteId = GenerateClassesCalendarRemoteId();

                this.ClassesCalendar = cals.FirstOrDefault(i => object.Equals(i.RemoteId, classesRemoteId));

                // If integration has been disabled
                if (Account.IsClassesCalendarIntegrationDisabled)
                {
                    // Delete calendar if exists
                    if (this.ClassesCalendar != null)
                    {
                        await this.ClassesCalendar.DeleteAsync();
                        this.ClassesCalendar = null;
                    }
                }

                else
                {
                    if (this.ClassesCalendar == null)
                    {
                        this.ClassesCalendar = await store.CreateAppointmentCalendarAsync(classesCalendarDisplayName);
                        this.ClassesCalendar.OtherAppReadAccess = AppointmentCalendarOtherAppReadAccess.Full;
                        this.ClassesCalendar.OtherAppWriteAccess = AppointmentCalendarOtherAppWriteAccess.None;
                        //this.ClassesCalendar.SummaryCardView = AppointmentSummaryCardView.App;
                        this.ClassesCalendar.RemoteId = classesRemoteId;
                        await this.ClassesCalendar.SaveAsync();
                    }

                    else
                    {
                        if (!this.ClassesCalendar.DisplayName.Equals(classesCalendarDisplayName))
                        {
                            this.ClassesCalendar.DisplayName = classesCalendarDisplayName;
                            await this.ClassesCalendar.SaveAsync();
                        }
                    }
                }

                return true;
            }

            catch (Exception ex)
            {
                // Unable to complete the requested operation because of either a catastrophic media failure or a data structure corruption on the disk. (Excep_FromHResult 0x8007054E) 
                // The dependency service or group failed to start. (Excep_FromHResult 0x8007042C)
                // After starting, the service hung in a start-pending state. (Excep_FromHResult 0x8007042E)
                if (!ExceptionHelper.IsHResult(ex, 0x8007054E)
                    && !ExceptionHelper.IsHResult(ex, 0x8007042C)
                    && !ExceptionHelper.IsHResult(ex, 0x8007042E))
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
                return false;
            }
        }
    }
}