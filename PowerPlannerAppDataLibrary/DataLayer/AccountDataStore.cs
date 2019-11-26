using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using ToolsPortable;
using System.Diagnostics;
using PowerPlannerAppDataLibrary.Helpers;
using StorageEverywhere;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Extensions.Telemetry;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// ClassAttribute
/// ClassAttributeUnderClass
/// ClassSubject
/// ClassSubjectUnderClass
/// Class
/// Exam
/// Grade
/// Homework
/// Schedule
/// Semester
/// Task
/// TeacherUnderSchedule
/// Teacher
/// WeightCategory
/// Year
/// </summary>
/// 

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public class DeletedItems : IEnumerable<Guid>
    {
        public List<Guid> DeletedIdentifiers = new List<Guid>();

        /// <summary>
        /// A list of the AppointmentLocalId's of homeworks or exams that were deleted
        /// </summary>
        public List<string> DeletedHomeworkExamAppointments = new List<string>();

        public bool DidDeleteHoliday { get; set; }

        /// <summary>
        /// A list of the AppointmentLocalId's of schedules that were deleted
        /// </summary>
        public List<string> DeletedScheduleAppointments = new List<string>();

        public bool Contains(Guid identifier)
        {
            return DeletedIdentifiers.Contains(identifier);
        }

        public IEnumerator<Guid> GetEnumerator()
        {
            return DeletedIdentifiers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return DeletedIdentifiers.GetEnumerator();
        }

        public void Merge(DeletedItems newer)
        {
            // There's never any conflicts since items can only be deleted once
            DeletedIdentifiers.AddRange(newer.DeletedIdentifiers);

            DeletedHomeworkExamAppointments.AddRange(newer.DeletedHomeworkExamAppointments);
            DeletedScheduleAppointments.AddRange(newer.DeletedScheduleAppointments);
            DidDeleteHoliday = DidDeleteHoliday || newer.DidDeleteHoliday;
        }
    }

    public class ChangesOnDataItem : Dictionary<DataItemProperty, object>
    {
        public Guid Identifier { get; private set; }

        public Type Type { get; private set; }

        public ChangesOnDataItem(Type type, Guid identifier)
        {
            Type = type;
            Identifier = identifier;
        }
    }

    public class SaveChangesTasks
    {
        public System.Threading.Tasks.Task UpdateTilesTask { get; internal set; }

        public System.Threading.Tasks.Task UpdateRemindersTask { get; internal set; }

        public bool NeedsAccountToBeSaved { get; internal set; }

        public AccountDataItem Account { get; internal set; }

        /// <summary>
        /// Will not throw
        /// </summary>
        /// <returns></returns>
        public async System.Threading.Tasks.Task WaitForAllTasksAsync()
        {
            if (UpdateTilesTask != null)
            {
                try
                {
                    await UpdateTilesTask;
                }
                catch (Exception ex) { TelemetryExtension.Current?.TrackException(ex); }
            }

            if (UpdateRemindersTask != null)
            {
                try
                {
                    await UpdateRemindersTask;
                }
                catch (Exception ex) { TelemetryExtension.Current?.TrackException(ex); }
            }
        }
    }

    public interface IDataChangedEventHandler
    {
        void DataChanged(AccountDataStore dataStore, DataChangedEvent e);
    }

    public class DataChanges
    {
        /// <summary>
        /// Edited (or new) items
        /// </summary>
        public IEnumerable<BaseDataItem> EditedItems
        {
            get { return _storage.Where(i => i.Value != null).Select(i => i.Value); }
        }

        public IEnumerable<Guid> IdentifiersToDelete
        {
            get { return _storage.Where(i => i.Value == null).Select(i => i.Key); }
        }

        private HashSet<Guid> _editedClassIdentifiersToIgnoreFromCalendarIntegration;
        public IEnumerable<Guid> EditedClassIdentifiersToIgnoreFromCalendarIntegration
        {
            get
            {
                if (_editedClassIdentifiersToIgnoreFromCalendarIntegration == null)
                {
                    return new Guid[0];
                }
                else
                {
                    return _editedClassIdentifiersToIgnoreFromCalendarIntegration;
                }
            }
        }

        public void IgnoreEditedClassIdentifierFromCalendarIntegration(Guid classId)
        {
            if (_editedClassIdentifiersToIgnoreFromCalendarIntegration == null)
            {
                _editedClassIdentifiersToIgnoreFromCalendarIntegration = new HashSet<Guid>();
            }

            _editedClassIdentifiersToIgnoreFromCalendarIntegration.Add(classId);
        }

        private Dictionary<Guid, BaseDataItem> _storage = new Dictionary<Guid, BaseDataItem>();

        private bool DoesGuidExist(Guid id)
        {
            return _storage.ContainsKey(id);
        }

        /// <summary>
        /// Adds a new or edited item
        /// </summary>
        /// <param name="item"></param>
        public void Add(BaseDataItem item, bool throwIfExists = true)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (item.Identifier == Guid.Empty)
                throw new ArgumentException("Identifier on edited item cannot be empty.");

            if (throwIfExists)
            {
                if (DoesGuidExist(item.Identifier))
                    throw new ArgumentException("This item has already been added.");

                _storage.Add(item.Identifier, item);
            }
            else
            {
                _storage[item.Identifier] = item;
            }
        }

        public void DeleteItem(Guid identifier, bool throwIfExists = true)
        {
            if (throwIfExists)
            {
                // Ensure no conflicts
                if (DoesGuidExist(identifier))
                    throw new ArgumentException("This item has already been added.");

                _storage.Add(identifier, null);
            }
            else
            {
                _storage[identifier] = null;
            }
        }

        public bool IsEmpty()
        {
            return _storage.Count == 0;
        }
    }

    /// <summary>
    /// This should be used from a thread. Things in here will block the calling thread, like the locks.
    /// </summary>
    public class AccountDataStore
    {
        public class AccountApplier<T> : IEnumerable<T> where T : BaseDataItem
        {
            private AccountDataItem _account;
            private TableQuery<T> _tableQuery;

            public AccountApplier(AccountDataItem account, TableQuery<T> tableQuery)
            {
                _account = account;
                _tableQuery = tableQuery;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return (_tableQuery as IEnumerable<T>).Select(i => ApplyAccount(i)).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private T ApplyAccount(T item)
            {
                if (item != null)
                {
                    item.Account = _account;
                }
                return item;
            }

            public int Count() { return _tableQuery.Count(); }
            public int Count(Expression<Func<T, bool>> predExpr) { return _tableQuery.Count(predExpr); }
            public T ElementAt(int index) { return ApplyAccount(_tableQuery.ElementAt(index)); }
            public T First() { return ApplyAccount(_tableQuery.First()); }
            public T First(Expression<Func<T, bool>> predExpr) { return ApplyAccount(_tableQuery.First(predExpr)); }
            public T FirstOrDefault() { return ApplyAccount(_tableQuery.FirstOrDefault()); }
            public T FirstOrDefault(Expression<Func<T, bool>> predExpr) { return ApplyAccount(_tableQuery.FirstOrDefault(predExpr)); }
            public AccountApplier<T> Where(Expression<Func<T, bool>> predExpr) { return new AccountApplier<T>(_account, _tableQuery.Where(predExpr)); }
        }

        public AccountDataItem Account { get; private set; }

        private ChangedItems _loadedChangedItems;

        private const string WAS_UPDATED_BY_BACKGROUND_TASK = "WasUpdatedByBackground";

        /// <summary>
        /// If data was updated by background task, clears cached accounts/data, resets flag to false, and returns true.
        /// </summary>
        /// <returns></returns>
        public static bool RetrieveAndResetWasUpdatedByBackgroundTask()
        {
            if (Settings.WasUpdatedByBackgroundTask)
            {
                _dataStoreCache.Clear();
                AccountsManager.ClearCachedAccounts();
                Settings.WasUpdatedByBackgroundTask = false;
                return true;
            }

            return false;
        }

        public static void SetUpdatedByBackgroundTask()
        {
            Settings.WasUpdatedByBackgroundTask = true;
        }

        public static event EventHandler<DataChangedEvent> DataChangedEvent;
        private static List<WeakReference<IDataChangedEventHandler>> _dataChangedEventHandlers = new List<WeakReference<IDataChangedEventHandler>>();
        public static void AddDataChangedEventHandler(IDataChangedEventHandler handler)
        {
            lock (_dataChangedEventHandlers)
            {
                ClearDisposedDataChangedEventHandlers();
                _dataChangedEventHandlers.Add(new WeakReference<IDataChangedEventHandler>(handler));
            }
        }

        private static void ClearDisposedDataChangedEventHandlers()
        {
            IDataChangedEventHandler handler;
            _dataChangedEventHandlers.RemoveAll(i => !i.TryGetTarget(out handler));
        }

        private void NotifyDataChangedEventHandlers(DataChangedEvent e)
        {
            lock (_dataChangedEventHandlers)
            {
                IDataChangedEventHandler handler = null;
                foreach (var reference in _dataChangedEventHandlers.Where(i => i.TryGetTarget(out handler)))
                {
                    try
                    {
                        handler.DataChanged(this, e);
                    }
                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                }
            }
        }

        internal class ChangedItems
        {
            private Guid _localAccountId;
            private Dictionary<Guid, ChangedPropertiesOfDataItem> _items;

            private ChangedItems(Guid localAccountId, Dictionary<Guid, ChangedPropertiesOfDataItem> items)
            {
                _localAccountId = localAccountId;
                _items = items;
            }

            private ChangedPropertiesOfDataItem GetOrCreate(Guid identifier)
            {
                ChangedPropertiesOfDataItem answer;

                if (_items.TryGetValue(identifier, out answer))
                    return answer;

                answer = new ChangedPropertiesOfDataItem();

                _items[identifier] = answer;

                return answer;
            }

            public void AddDeletedItem(Guid identifier)
            {
                GetOrCreate(identifier).SetDeleted();
            }

            public void AddNewItem(Guid identifier)
            {
                GetOrCreate(identifier).SetNew();
            }

            public void AddEditedItem(Guid identifier, IEnumerable<BaseDataItem.SyncPropertyNames> changedProperties)
            {
                GetOrCreate(identifier).SetEdited(changedProperties);
            }

            /// <summary>
            /// Returns true if made changes
            /// </summary>
            /// <returns></returns>
            public bool ClearSyncing()
            {
                bool changed = false;

                foreach (ChangedPropertiesOfDataItem changedProperties in _items.Values)
                {
                    if (changedProperties.ClearSyncing())
                        changed = true;
                }

                Guid[] emptyToRemove = _items.Where(i => i.Value.IsEmpty()).Select(i => i.Key).ToArray();
                

                if (emptyToRemove.Length > 0)
                {
                    changed = true;

                    foreach (Guid id in emptyToRemove)
                        _items.Remove(id);
                }


                return changed;
            }

            public bool NeedsClearSyncing()
            {
                foreach (ChangedPropertiesOfDataItem changedProperties in _items.Values)
                {
                    if (changedProperties.NeedsClearSyncing())
                    {
                        return true;
                    }
                }

                return _items.Any(i => i.Value.IsEmpty());
            }

            /// <summary>
            /// Saves. Caller should establish data lock
            /// </summary>
            public async System.Threading.Tasks.Task Save()
            {
                // Get the account folder
                var timeTracker = TimeTracker.Start();
                IFolder accountFolder = await FileHelper.GetOrCreateAccountFolder(_localAccountId);
                timeTracker.End(3, "ChangedItems.Save get account folder");

                // Create temp file to write to
                timeTracker = TimeTracker.Start();
                IFile tempFile = await accountFolder.CreateFileAsync(FileNames.TEMP_ACCOUNT_CHANGED_ITEMS_FILE_NAME, CreationCollisionOption.ReplaceExisting);
                timeTracker.End(3, "ChangedItems.Save create temp file");

                // Write the data to the temp file
                timeTracker = TimeTracker.Start();
                using (Stream s = await tempFile.OpenAsync(StorageEverywhere.FileAccess.ReadAndWrite))
                {
                    timeTracker.End(3, "ChangedItems.Save open stream");

                    timeTracker = TimeTracker.Start();
                    using (StreamWriter writer = new StreamWriter(s))
                    {
                        GetSerializer().Serialize(writer, _items);
                    }
                    timeTracker.End(3, $"ChangedItems.Save serializing to stream. {_items.Count} items.");
                }

                // Move the temp file to the actual file
                timeTracker = TimeTracker.Start();
                await tempFile.RenameAsync(FileNames.ACCOUNT_CHANGED_ITEMS_FILE_NAME, NameCollisionOption.ReplaceExisting);
                timeTracker.End(3, "ChangedItems.Save renaming temp to actual");
            }

            
            internal static async System.Threading.Tasks.Task ImportChangesAsync(Guid localAccountId, Guid[] newOrEdited, Guid[] deletes)
            {
                Dictionary<Guid, ChangedPropertiesOfDataItem> rawData = new Dictionary<Guid, ChangedPropertiesOfDataItem>();

                foreach (var i in newOrEdited)
                {
                    ChangedPropertiesOfDataItem changed = new ChangedPropertiesOfDataItem();
                    changed.SetNew();
                    rawData[i] = changed;
                }

                foreach (var deleted in deletes)
                {
                    ChangedPropertiesOfDataItem changed = new ChangedPropertiesOfDataItem();
                    changed.SetDeleted();
                    rawData[deleted] = changed;
                }

                ChangedItems changedItems = new ChangedItems(localAccountId, rawData);
                await changedItems.Save();
            }

            private static Newtonsoft.Json.JsonSerializer GetSerializer()
            {
                return new Newtonsoft.Json.JsonSerializer();
            }

            private static async Task<IFile> CreateFile(Guid localAccountId)
            {
                return await FileSystem.Current.LocalStorage.CreateFileByPathAsync(FileNames.ACCOUNT_FOLDER_PATH(localAccountId), FileNames.ACCOUNT_CHANGED_ITEMS_FILE_NAME, CreationCollisionOption.ReplaceExisting);
            }

            private static async Task<IFile> GetFile(Guid localAccountId)
            {
                try
                {
                    return await FileSystem.Current.LocalStorage.GetFileByPathAsync(FileNames.ACCOUNT_FOLDER_PATH(localAccountId), FileNames.ACCOUNT_CHANGED_ITEMS_FILE_NAME);
                }

                catch (FileNotFoundException)
                {
                    return null;
                }
            }

            /// <summary>
            /// Caller should establish data lock
            /// </summary>
            /// <param name="localAccountId"></param>
            /// <returns></returns>
            public static async Task<ChangedItems> Load(AccountDataStore dataStore)
            {
                if (dataStore._loadedChangedItems != null)
                {
                    return dataStore._loadedChangedItems;
                }

                var localAccountId = dataStore.LocalAccountId;

                var timeTracker = TimeTracker.Start();
                IFile file = await GetFile(localAccountId);
                timeTracker.End(3, "ChangedItems.Load GetFile");

                if (file == null)
                {
                    dataStore._loadedChangedItems = new ChangedItems(localAccountId, new Dictionary<Guid, ChangedPropertiesOfDataItem>());
                    return dataStore._loadedChangedItems;
                }

                timeTracker = TimeTracker.Start();
                using (Stream s = await file.OpenAsync(StorageEverywhere.FileAccess.Read))
                {
                    timeTracker.End(3, "ChangedItems.Load OpenAsync");

                    var serializer = GetSerializer();
                    Dictionary<Guid, ChangedPropertiesOfDataItem> answer;

                    try
                    {
                        timeTracker = TimeTracker.Start();
                        using (StreamReader reader = new StreamReader(s))
                        {
                            using (var jsonReader = new JsonTextReader(reader))
                            {
                                answer = serializer.Deserialize<Dictionary<Guid, ChangedPropertiesOfDataItem>>(jsonReader);
                            }
                        }
                        timeTracker.End(3, "ChangedItems.Load read and deserialize");
                    }

                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                        answer = new Dictionary<Guid, ChangedPropertiesOfDataItem>();
                    }

                    dataStore._loadedChangedItems = new ChangedItems(localAccountId, answer);
                    return dataStore._loadedChangedItems;
                }
            }

            public Guid[] GetAllDeleted()
            {
                return _items.Where(i => i.Value.Type == ChangedPropertiesOfDataItem.ChangeType.Deleted).Select(i => i.Key).ToArray();
            }

            public Guid[] GetAllNew()
            {
                return _items.Where(i => i.Value.Type == ChangedPropertiesOfDataItem.ChangeType.New).Select(i => i.Key).ToArray();
            }

            public Tuple<Guid, BaseDataItem.SyncPropertyNames[]>[] GetAllEdited()
            {
                return _items.Where(i => i.Value.Type == ChangedPropertiesOfDataItem.ChangeType.Edited).Select(i => new Tuple<Guid, BaseDataItem.SyncPropertyNames[]>(i.Key, i.Value.GetEditedProperties())).ToArray();
            }

            public void MarkAllSent()
            {
                foreach (var pair in _items)
                    pair.Value.MarkSent();
            }
        }

        public Guid LocalAccountId
        {
            get { return Account.LocalAccountId; }
        }
        public SQLiteConnection _db;

        private static WeakReferenceCache<Guid, AccountDataStore> _dataStoreCache = new WeakReferenceCache<Guid, AccountDataStore>();

        public static async Task<AccountDataStore> Get(Guid localAccountId)
        {
            Debug.WriteLine("AccountDataStore Get: " + localAccountId.ToString());

            // Check if already loaded
            AccountDataStore existing = GetCached(localAccountId);

            // If already loaded, yay done!
            if (existing != null)
                return existing;

            AccountDataItem account = await AccountsManager.GetOrLoad(localAccountId);
            if (account == null)
            {
                throw new Exception("Account doesn't exist");
            }

            // Otherwise will need to load, so run a task for the data lock
            return await System.Threading.Tasks.Task.Run(async delegate
            {
                // Establish data lock (Write because we might be initializing database)
                using (await Locks.LockDataForWriteAsync("AccountDataStore.Get"))
                {
                    // Check if we've already loaded it in the meantime
                    AccountDataStore dataStore = GetCached(localAccountId);

                    // If we've already loaded it, then yay done!
                    if (dataStore != null)
                        return dataStore;

                    // Otherwise need to load it
#if DEBUG
                    Debug.WriteLine("Initializing new data store: " + localAccountId);
                    DateTime start = DateTime.Now;
#endif
                    try
                    {
                        dataStore = new AccountDataStore(account);
                        await dataStore.InitializeDatabaseAsync();
                    }
                    catch (SQLiteException)
                    {
                        // Database corrupted, delete and re-create
                        var file = await FileSystem.Current.GetFileFromPathAsync(GetDatabaseFilePath(localAccountId));
                        if (file == null)
                        {
                            throw;
                        }

                        // Need to reset the account's change number, so it re-syncs everything
                        account.CurrentChangeNumber = 0;
                        await AccountsManager.Save(account);

                        // Delete the database
                        await file.DeleteAsync();

                        // Re-create
                        dataStore = new AccountDataStore(account);
                        await dataStore.InitializeDatabaseAsync();

                        TelemetryExtension.Current?.TrackEvent("DatabaseCorruptAndReset");
                    }
#if DEBUG
                    DateTime end = DateTime.Now;
                    Debug.WriteLine("Initialized new data store: " + localAccountId + ". " + (end - start).TotalMilliseconds + " milliseconds");
#endif

                    // And then set it as cached
                    lock (_dataStoreCache)
                    {
                        _dataStoreCache[localAccountId] = dataStore;
                    }

                    return dataStore;
                }

            });
        }

        private static AccountDataStore GetCached(Guid localAccountId)
        {
            lock (_dataStoreCache)
            {
                AccountDataStore existing;

                if (_dataStoreCache.TryGetValue(localAccountId, out existing) && existing != null)
                {
                    Debug.WriteLine("Returning cached data store: " + localAccountId);
                    return existing;
                }
            }

            return null;
        }

        /// <summary>
        /// Trashes the DB connection so that we can delete everything
        /// </summary>
        /// <param name="localAccountId"></param>
        public static void Dispose(Guid localAccountId)
        {
            lock (_dataStoreCache)
            {
                AccountDataStore existing;

                if (_dataStoreCache.TryGetValue(localAccountId, out existing))
                {
                    if (existing._db != null)
                    {
                        existing._db.Close();
                        existing._db = null;
                    }

                    _dataStoreCache.Remove(localAccountId);
                }
            }
        }

        /// <summary>
        /// Creates a data lock, saves changes
        /// </summary>
        /// <returns></returns>
        public async System.Threading.Tasks.Task ClearSyncing()
        {
            await System.Threading.Tasks.Task.Run(async delegate
            {
                ChangedItems changes;
                using (await Locks.LockDataForReadAsync())
                {
                    changes = await ChangedItems.Load(this);

                    // If nothing changed, just return
                    if (!changes.NeedsClearSyncing())
                        return;
                }

                // Otherwise need to clear and save changes
                using (await Locks.LockDataForWriteAsync("AccountDataStore.ClearSyncing"))
                {
                    if (changes.ClearSyncing())
                    {
                        await changes.Save();
                    }
                }
            });
        }

        /// <summary>
        /// Caller must establish data lock. Caller must call InitializeDatabaseAsync().
        /// </summary>
        /// <param name="localAccountId"></param>
        private AccountDataStore(AccountDataItem account)
        {
            Account = account;
        }

        private static readonly Type[] DataItemTableTypes = new Type[]
        {
            typeof(DataItemClass),
            typeof(DataItemMegaItem),
            typeof(DataItemGrade),
            typeof(DataItemSchedule),
            typeof(DataItemSemester),
            typeof(DataItemWeightCategory),
            typeof(DataItemYear)
        };

        public string DatabaseFilePath
        {
            get { return GetDatabaseFilePath(LocalAccountId); }
        }

        public static string GetDatabaseFilePath(Guid localAccountId)
        {
            return Path.Combine(FileSystem.Current.LocalStorage.Path, Path.Combine(FileNames.ACCOUNT_FOLDER_PATH(localAccountId)), FileNames.ACCOUNT_DATABASE_FILE_NAME);
        }

        /// <summary>
        /// Caller should already have data lock
        /// </summary>
        /// <returns></returns>
        public async System.Threading.Tasks.Task InitializeDatabaseAsync()
        {
            try
            {
                await InitializeDatabaseHelperAsync();
            }
            catch
            {
                if (_db != null)
                {
                    try
                    {
                        _db.Dispose();
                    }
                    catch { }
                    _db = null;
                }
                throw;
            }
        }

        /// <summary>
        /// Caller must establish data lock
        /// </summary>
        private async System.Threading.Tasks.Task InitializeDatabaseHelperAsync()
        {
            var timeTracker = TimeTracker.Start();
            _db = new SQLiteConnection(DatabaseFilePath);
            timeTracker.End(3, "AccountDataStore.InitializeDatabase create SQLiteConnection");

            // Create tables for data items (if they don't exist)
            timeTracker = TimeTracker.Start();
            foreach (Type t in DataItemTableTypes)
                _db.CreateTable(t);

            // Create table for storing images to upload
            _db.CreateTable<ImageToUpload>();


            _db.CreateTable<DataInfo>();
            timeTracker.End(8, "AccountDataStore.InitializeDatabase create tables");

            // Handle upgrading data
            timeTracker = TimeTracker.Start();
            var dataInfo = _db.Table<DataInfo>().FirstOrDefault();
            if (dataInfo == null)
            {
                // If not found, have to assume we came from version 1
                // That means newly created accounts will start at version 1
                // but that's ok since the upgrade operations won't do anything
                dataInfo = new DataInfo()
                {
                    Version = 1
                };
            }
            var version = dataInfo.Version;
            if (version < 3)
            {
                if (_db.GetTableInfo("DataItemHomework").Count > 0)
                {
#pragma warning disable 612, 618
                    _db.CreateTable<DataItemHomework>();
                    _db.CreateTable<DataItemExam>();
#pragma warning restore 612, 618

                    var semesters = TableSemesters.ToArray();
                    var classes = TableClasses.ToArray();

                    var handleV2update = new Action<BaseDataItemHomeworkExam>((dataItem) =>
                    {
                        if (version < 2)
                        {
                            dataItem.GradeReceived = Grade.UNGRADED;
                            dataItem.GradeTotal = 100;
                            dataItem.IsDropped = false;
                            dataItem.IndividualWeight = 1;
                            dataItem.WeightCategoryIdentifier = Guid.Empty;
                        }
                    });

                    using (var batchInserter = new BatchDbInserter(_db, 30))
                    {
#pragma warning disable 612, 618
                        foreach (var h in _db.Table<DataItemHomework>())
#pragma warning restore 612, 618
                        {
                            handleV2update(h);

                            var c = classes.FirstOrDefault(i => h.UpperIdentifier == i.Identifier);
                            if (c == null)
                            {
                                continue;
                            }

                            batchInserter.Insert(new DataItemMegaItem()
                            {
                                MegaItemType = MegaItemType.Homework,
                                Identifier = h.Identifier,
                                DateCreated = h.DateCreated,
                                Updated = h.Updated,
                                UpperIdentifier = c.Identifier,
                                AppointmentLocalId = h.AppointmentLocalId,
#if ANDROID
                                HasSentReminder = h.HasSentReminder,
#endif
                                Date = h.Date,
                                Details = h.Details,
                                EndTime = h.EndTime,
                                GradeReceived = h.GradeReceived,
                                GradeTotal = h.GradeTotal,
                                ImageNames = h.ImageNames,
                                IndividualWeight = h.IndividualWeight,
                                IsDropped = h.IsDropped,
                                Name = h.Name,
                                PercentComplete = h.PercentComplete,
                                Reminder = h.Reminder,
                                WeightCategoryIdentifier = h.WeightCategoryIdentifier
                            });
                        }

#pragma warning disable 612, 618
                        foreach (var e in _db.Table<DataItemExam>())
#pragma warning restore 612, 618
                        {
                            handleV2update(e);

                            var c = classes.FirstOrDefault(i => e.UpperIdentifier == i.Identifier);
                            if (c == null)
                            {
                                continue;
                            }

                            batchInserter.Insert(new DataItemMegaItem()
                            {
                                MegaItemType = MegaItemType.Exam,
                                Identifier = e.Identifier,
                                DateCreated = e.DateCreated,
                                Updated = e.Updated,
                                UpperIdentifier = c.Identifier,
                                AppointmentLocalId = e.AppointmentLocalId,
#if ANDROID
                                HasSentReminder = e.HasSentReminder,
#endif
                                Date = e.Date,
                                Details = e.Details,
                                EndTime = e.EndTime,
                                GradeReceived = e.GradeReceived,
                                GradeTotal = e.GradeTotal,
                                ImageNames = e.ImageNames,
                                IndividualWeight = e.IndividualWeight,
                                IsDropped = e.IsDropped,
                                Name = e.Name,
                                Reminder = e.Reminder,
                                WeightCategoryIdentifier = e.WeightCategoryIdentifier
                            });
                        }
                    }

#pragma warning disable 612, 618
                    _db.DropTable<DataItemHomework>();
                    _db.DropTable<DataItemExam>();
#pragma warning restore 612, 618
                }
            }
            if (version < 4)
            {
                // Added StartDate/EndDate to classes
                _db.Execute("update DataItemClass set StartDate = ?, EndDate = ?", SqlDate.MinValue, SqlDate.MinValue);
            }
            if (version < 5)
            {
                // Added PassingGrade to classes, which needs to be set to 60% by default
                _db.Execute("update DataItemClass set PassingGrade = ?", Class.DefaultPassingGrade);
            }
            if (version < 6 && version == 5)
            {
                // In previous version 5, I accidently was saving a class with the identifier of a semester, wiping out semesters
                try
                {
                    int countDeleted = _db.Execute("delete from DataItemClass where exists (select * from DataItemSemester where DataItemClass.Identifier = DataItemSemester.Identifier and DataItemClass.UpperIdentifier = ?)", Guid.Empty);

                    if (countDeleted > 0)
                    {
                        Dictionary<long, Guid> affectedOnlineAccountsAndSemesters = new Dictionary<long, Guid>();
                        using (Stream stream = typeof(AccountDataStore).GetTypeInfo().Assembly.GetManifestResourceStream("PowerPlannerAppDataLibrary.Files.AffectedWipedSemesters.txt"))
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                string text = reader.ReadToEnd();

                                string[] lines = text.Split('\n');
                                foreach (var line in lines)
                                {
                                    string[] accountAndSemester = line.Split(',');

                                    long accountId = long.Parse(accountAndSemester[0]);
                                    Guid semesterId = Guid.Parse(accountAndSemester[1].Trim());
                                    affectedOnlineAccountsAndSemesters[accountId] = semesterId;
                                }
                            }
                        }

                        if (Account.IsOnlineAccount && affectedOnlineAccountsAndSemesters.ContainsKey(Account.AccountId))
                        {
                            Guid semesterToReUpload = affectedOnlineAccountsAndSemesters[Account.AccountId];
                            string name = _db.ExecuteScalar<string>("select Name from DataItemSemester where Identifier = ?", semesterToReUpload);
                            if (name != null && name != "[Recovered]")
                            {
                                var changedItems = await ChangedItems.Load(this);
                                changedItems.AddEditedItem(semesterToReUpload, new BaseDataItem.SyncPropertyNames[]
                                {
                                    BaseDataItem.SyncPropertyNames.Name,
                                    BaseDataItem.SyncPropertyNames.Start,
                                    BaseDataItem.SyncPropertyNames.End,
                                    BaseDataItem.SyncPropertyNames.UpperIdentifier
                                });
                                await changedItems.Save();

                                TelemetryExtension.Current?.TrackEvent("ReUploadWipedSemester", new Dictionary<string, string>()
                                {
                                    { "AccountId", Account.AccountId.ToString() }
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            bool needsAppointmentReset = false;
            if (version < 7)
            {
                // On UWP, we were adding duplicates to calendar integration, so we'll reset calendar integration
                needsAppointmentReset = true;
            }
            if (version < DataInfo.LATEST_VERSION)
            {
                dataInfo.Version = DataInfo.LATEST_VERSION;
                _db.InsertOrReplace(dataInfo);
            }
            timeTracker.End(3, "AccountDataStore.InitializeDatabase handle upgrade");

            if (needsAppointmentReset)
            {
                AppointmentsExtension.Current?.ResetAll(Account, this);
            }
        }

        private class BatchDbInserter : IDisposable
        {
            public int MaxInBatch { get; private set; }

            private SQLiteConnection _db;
            private List<object> _queued = new List<object>();

            public BatchDbInserter(SQLiteConnection db, int maxInBatch)
            {
                _db = db;
                MaxInBatch = maxInBatch;
            }

            public void Insert(object obj)
            {
                _queued.Add(obj);

                if (_queued.Count >= MaxInBatch)
                {
                    _db.InsertAll(_queued);
                    _queued.Clear();
                }
            }

            public void Dispose()
            {
                if (_queued.Count > 0)
                {
                    _db.InsertAll(_queued);
                }
            }
        }

        public class DataInfo
        {
            public const int LATEST_VERSION = 7;

            [PrimaryKey]
            public short Key { get; set; } = 1;

            public int Version { get; set; }

            public static DataInfo CreateNew()
            {
                return new DataInfo()
                {
                    Version = LATEST_VERSION
                };
            }
        }

        ~AccountDataStore()
        {
            if (_db != null)
            {
                try
                {
                    _db.Close();
                }
                // SQLite exception for "library routine called out of sequence" is normal if the initialization of the database failed,
                // therefore calling close on the uninitialized database would be invalid. So we don't track that error.
                catch (SQLiteException) { }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                _db = null;
            }
        }

        public TableQuery<ImageToUpload> TableImagesToUpload
        {
            get { return _db.Table<ImageToUpload>(); }
        }

        public AccountApplier<DataItemClass> TableClasses
        {
            get { return new AccountApplier<DataItemClass>(Account, _db.Table<DataItemClass>()); }
        }

        public AccountApplier<DataItemMegaItem> TableMegaItems
        {
            get { return new AccountApplier<DataItemMegaItem>(Account, _db.Table<DataItemMegaItem>()); }
        }

        public AccountApplier<DataItemGrade> TableGrades
        {
            get { return new AccountApplier<DataItemGrade>(Account, _db.Table<DataItemGrade>()); }
        }

        public AccountApplier<DataItemSchedule> TableSchedules
        {
            get { return new AccountApplier<DataItemSchedule>(Account, _db.Table<DataItemSchedule>()); }
        }

        public AccountApplier<DataItemSemester> TableSemesters
        {
            get { return new AccountApplier<DataItemSemester>(Account, _db.Table<DataItemSemester>()); }
        }

        public AccountApplier<DataItemWeightCategory> TableWeightCategories
        {
            get { return new AccountApplier<DataItemWeightCategory>(Account, _db.Table<DataItemWeightCategory>()); }
        }

        public AccountApplier<DataItemYear> TableYears
        {
            get
            {
                return new AccountApplier<DataItemYear>(Account, _db.Table<DataItemYear>());
            }
        }

        public TableQuery<DataItemClass> ActualTableClasses
        {
            get { return _db.Table<DataItemClass>(); }
        }

        public TableQuery<DataItemMegaItem> ActualTableMegaItems
        {
            get { return _db.Table<DataItemMegaItem>(); }
        }

        public TableQuery<DataItemGrade> ActualTableGrades
        {
            get { return _db.Table<DataItemGrade>(); }
        }

        public TableQuery<DataItemSchedule> ActualTableSchedules
        {
            get { return _db.Table<DataItemSchedule>(); }
        }

        public TableQuery<DataItemSemester> ActualTableSemesters
        {
            get { return _db.Table<DataItemSemester>(); }
        }

        public TableQuery<DataItemWeightCategory> ActualTableWeightCategories
        {
            get { return _db.Table<DataItemWeightCategory>(); }
        }

        public TableQuery<DataItemYear> ActualTableYears
        {
            get
            {
                return _db.Table<DataItemYear>();
            }
        }

        public enum ProcessType
        {
            Local,
            Online,
            OnlineMultiPart
        }

        /// <summary>
        /// Uses DataLock
        /// </summary>
        /// <param name="toUpload"></param>
        public async System.Threading.Tasks.Task AddImagesToUploadAsync(IEnumerable<string> toUpload)
        {
            await System.Threading.Tasks.Task.Run(delegate
            {
                return AddImagesToUploadBlocking(toUpload);
            });
        }

        /// <summary>
        /// Uses DataLock
        /// </summary>
        /// <param name="toUpload"></param>
        private async System.Threading.Tasks.Task AddImagesToUploadBlocking(IEnumerable<string> toUpload)
        {
            using (await Locks.LockDataForWriteAsync())
            {
                _db.RunInTransaction(delegate
                {
                    foreach (var u in toUpload)
                    {
                        _db.InsertOrReplace(new ImageToUpload()
                        {
                            FileName = u
                        });
                    }
                });
            }
        }

        /// <summary>
        /// Uses DataLock
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetNextImageToUploadAsync()
        {
            return await System.Threading.Tasks.Task.Run(new Func<string>(GetNextImageToUploadBlocking));
        }

        /// <summary>
        /// Doesn't use data lock
        /// </summary>
        /// <returns></returns>
        private string GetNextImageToUploadBlocking()
        {
            var toUpload = TableImagesToUpload.FirstOrDefault();

            if (toUpload == null)
                return null;

            return toUpload.FileName;
        }

        public async System.Threading.Tasks.Task MarkImageUploadedAsync(string fileName)
        {
            await System.Threading.Tasks.Task.Run(delegate
            {
                MarkImageUploadedBlocking(fileName);
            });
        }

        /// <summary>
        /// Doesn't use data lock
        /// </summary>
        /// <param name="fileName"></param>
        private void MarkImageUploadedBlocking(string fileName)
        {
            _db.Delete<ImageToUpload>(fileName);
        }

        /// <summary>
        /// This establishes a data lock
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dictionary<string, object>>, Guid[]>> GetUpdatesAndDeletesAsync()
        {
            return await System.Threading.Tasks.Task.Run(async delegate
            {
                ChangedItems changedItems;
                Guid[] deletes;
                IEnumerable<Dictionary<string, object>> updates;

                using (await Locks.LockDataForReadAsync())
                {
                    changedItems = await ChangedItems.Load(this);

                    var newItemIdentifiers = changedItems.GetAllNew();
                    var editedItems = changedItems.GetAllEdited();
                    deletes = changedItems.GetAllDeleted();

                    // If nothing changed
                    if (newItemIdentifiers.Length == 0 && editedItems.Length == 0 && deletes.Length == 0)
                        return new Tuple<IEnumerable<Dictionary<string, object>>, Guid[]>(new Dictionary<string, object>[0], new Guid[0]);


                    // grab all new/edited
                    var timeTracker = TimeTracker.Start();
                    updates = GetUpdatesBlocking(newItemIdentifiers, editedItems);
                    timeTracker.End(3, $"AccountDataStore.GetUpdatesandDeletesAsync GetUpdatesBlocking, {newItemIdentifiers.Length} new items, {editedItems.Length} edited items");
                }

                using (await Locks.LockDataForWriteAsync())
                {
                    // Mark all the properties as sent, so that when sync completes, we can remove them
                    changedItems.MarkAllSent();

                    // And then save that
                    await changedItems.Save();
                }
                

                return new Tuple<IEnumerable<Dictionary<string, object>>, Guid[]>(updates, deletes);
            });
        }

        /// <summary>
        /// Caller should establish data lock
        /// </summary>
        /// <param name="newItemIdentifiers"></param>
        /// <param name="editedItems"></param>
        /// <returns></returns>
        private IEnumerable<Dictionary<string, object>> GetUpdatesBlocking(Guid[] newItemIdentifiers, Tuple<Guid, BaseDataItem.SyncPropertyNames[]>[] editedItems)
        {
            List<Dictionary<string, object>> answer = new List<Dictionary<string, object>>();

            foreach (var item in FindAll(newItemIdentifiers.Union(editedItems.Select(i => i.Item1)).ToArray()))
            {
                BaseDataItem.SyncPropertyNames[] changedProperties = editedItems.Where(i => i.Item1 == item.Identifier).Select(i => i.Item2).FirstOrDefault();

                // If edited, get properties selectively
                if (changedProperties != null)
                {
                    Dictionary<string, object> changes = new Dictionary<string, object>();

                    changes["Identifier"] = item.Identifier;
                    changes["Updated"] = item.Updated;
                    changes["ItemType"] = item.ItemType; // Include ItemType when updating so properties can be mapped correctly
                    // Otherwise without this, can't tell whether "GradeReceived" should map to Double1 or Double2, since it depends
                    // whether it's a Grade or Homework

                    foreach (var p in changedProperties)
                    {
                        changes[p.ToString()] = item.GetPropertyValue(p);
                    }

                    answer.Add(changes);
                }

                // Else new item
                else
                {
                    answer.Add(item.SerializeToDictionary());
                }
            }

            return answer;
        }

        /// <summary>
        /// Establishes data lock. Caller is responsible for checking the NeedsAccountToBeSaved property and saving the account
        /// </summary>
        /// <param name="dataChanges"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<SaveChangesTasks> ProcessOnlineChanges(DataChanges dataChanges, bool isMultiPart)
        {
            return await ProcessChangesHelper(
                dataChanges,
                isMultiPart ? ProcessType.OnlineMultiPart : ProcessType.Online);
        }

        public async System.Threading.Tasks.Task<SaveChangesTasks> ProcessLocalChanges(DataChanges dataChanges)
        {
            var saveChangesTask = await ProcessChangesHelper(dataChanges, ProcessType.Local);

            if (saveChangesTask.NeedsAccountToBeSaved)
            {
                await AccountsManager.Save(saveChangesTask.Account);
                saveChangesTask.NeedsAccountToBeSaved = false;
            }

            return saveChangesTask;
        }



        /// <summary>
        /// Items in the DataChanges will be modified.
        /// </summary>
        /// <param name="dataChanges"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task<SaveChangesTasks> ProcessChangesHelper(DataChanges dataChanges, ProcessType processType)
        {
            SaveChangesTasks pendingTasks = new SaveChangesTasks();

            var account = await AccountsManager.GetOrLoad(LocalAccountId);
            pendingTasks.Account = account;

            await System.Threading.Tasks.Task.Run(async delegate
            {
                using (await Locks.LockDataForWriteAsync())
                {
                    var commitChangesResponse = await CommitChanges(account, dataChanges, processType);

                    pendingTasks.NeedsAccountToBeSaved = commitChangesResponse.NeedsAccountToBeSaved;
                }
            });


            // If we're not in a multi-part insert...
            if (processType != ProcessType.OnlineMultiPart)
            {
                // Update the tiles (don't wait on it)
                try
                {
                    Debug.WriteLine("Updating tile notifications");
                    pendingTasks.UpdateTilesTask = TilesExtension.Current?.UpdateTileNotificationsForAccountAsync(account, this);
                }

                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to update tile notifications");
                    TelemetryExtension.Current?.TrackException(ex);
                }

                // And update reminders (don't wait on it)
                try
                {
                    Debug.WriteLine("Updating toast reminders");
                    pendingTasks.UpdateRemindersTask = RemindersExtension.Current?.ResetReminders(account, this);
                }

                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to update toast reminders");
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            return pendingTasks;
        }

        /// <summary>
        /// Used for upgrading from Windows 8, no need for data lock since we're not going to be calling this in a multi-threaded fashion.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        internal async System.Threading.Tasks.Task ImportItemsAsync(BaseDataItem[] items)
        {
            await System.Threading.Tasks.Task.Run(delegate
            {
                ImportItemsBlocking(items);
            });
        }

        private void ImportItemsBlocking(BaseDataItem[] items)
        {
            _db.InsertAll(items);
        }

        private class CommitChangesResponse
        {
            public bool NeedsAccountToBeSaved { get; set; }
        }

        /// <summary>
        /// Items in the DataChanges will be modified.
        /// </summary>
        /// <param name="dataChanges"></param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task<CommitChangesResponse> CommitChanges(AccountDataItem account, DataChanges dataChanges, ProcessType processType)
        {
            if (dataChanges == null)
                throw new ArgumentNullException("dataChanges");

            // Assign account to items
            foreach (var c in dataChanges.EditedItems)
            {
                c.Account = Account;
            }
            
            ChangedItems changedItems = null;

            // Obtain the current changes if local request
            if (processType == ProcessType.Local)
                changedItems = await ChangedItems.Load(this);

            // Obtain the existing data items (whichever match)
            var timeTracker = TimeTracker.Start();
            List<BaseDataItem> existingDataItems = GetExistingItems(dataChanges.EditedItems);
            timeTracker.End(3, delegate { return $"CommitChanges GetExistingItems, {dataChanges.EditedItems.Count()} edited items"; });

            // Whichever didn't exist will be new items
            List<BaseDataItem> newDataItems = new List<BaseDataItem>();

            DateTime now = DateTime.UtcNow;

            // Then write the new/edited items
            foreach (BaseDataItem edited in dataChanges.EditedItems)
            {
                // Assign the Updated time if this is local changes
                if (processType == ProcessType.Local)
                    edited.Updated = now;

                // Find the existing item, if there is one
                BaseDataItem existing = existingDataItems.FirstOrDefault(i => i.Identifier == edited.Identifier);

                // If there wasn't an existing data item
                if (existing == null)
                {
                    // We let this edited item become the item to save
                    BaseDataItem newItem = edited;

                    // And we also assign the DateCreated if this is local
                    if (processType == ProcessType.Local)
                        newItem.DateCreated = now;

                    // Flag that it's a new item
                    if (changedItems != null)
                        changedItems.AddNewItem(newItem.Identifier);

                    // Add it to our collection of new items to save
                    newDataItems.Add(newItem);
                }

                // Otherwise we need to copy properties into the existing
                else
                {
                    // Assign account to existing
                    existing.Account = Account;

                    // Apply the changes, while also getting which properties were actually affected
                    var changedPropertyNames = existing.ImportChanges(edited);

#if ANDROID
                    // We always set HasSentReminder to false when edited
                    if (existing is DataItemMegaItem)
                    {
                        (existing as DataItemMegaItem).HasSentReminder = false;
                    }
#endif

                    // And then flag that item as edited
                    if (changedItems != null)
                        changedItems.AddEditedItem(existing.Identifier, changedPropertyNames);
                }

                // Increment the now time so that each one is unique and maintains order
                now = now.AddTicks(1);
            }

            // And flag the items to delete
            if (changedItems != null)
                foreach (Guid id in dataChanges.IdentifiersToDelete)
                    changedItems.AddDeletedItem(id);

            

            var savePoint = _db.SaveTransactionPoint();

            DeletedItems deletedItems;

            try
            {
                // Update the existing items
                timeTracker = TimeTracker.Start();
                _db.UpdateAll(existingDataItems, false); // Don't run in its own transaction
                timeTracker.End(3, $"CommitChanges UpdateAll, {existingDataItems.Count} existingDataItems");

                // Add the new items
                timeTracker = TimeTracker.Start();
                _db.InsertAll(newDataItems, false); // Don't run in its own transaction
                timeTracker.End(3, $"CommitChanges InsertAll, {newDataItems.Count} newDataItems");

                // And delete the deleted items
                timeTracker = TimeTracker.Start();
                deletedItems = RecursiveDelete(dataChanges.IdentifiersToDelete.ToArray());
                timeTracker.End(3, $"CommitChanges RecursiveDelete, {dataChanges.IdentifiersToDelete.Count()} identifiers to delete");

                // Save the properties that have been changed so we can upload them later
                if (changedItems != null)
                    await changedItems.Save();

                _db.Release(savePoint);
            }
            catch (Exception)
            {
                _db.RollbackTo(savePoint);
                throw;
            }


            DataChangedEvent dataChangedEvent = new DataChangedEvent(LocalAccountId, newDataItems, existingDataItems, deletedItems, wasLocalChanges: processType == ProcessType.Local, originalChanges: dataChanges);

            // Queue the Appointments to be updated (this saves the account so that it's flagged as Appointments not updated, and then does remaining work on separate thread)
            bool needsSave = false;

            // Only update appointments if we're not in a multi-part sync
            if (processType != ProcessType.OnlineMultiPart)
            {
                if (AppointmentsExtension.Current != null)
                {
                    timeTracker = TimeTracker.Start();
                    var updateResponse = AppointmentsExtension.Current.Update(account, this, dataChangedEvent);
                    timeTracker.End(3, "CommitChanges AppointmentsExtension Update");
                    needsSave = needsSave || updateResponse.NeedsAccountToBeSaved;
                }
            }

            // send out changed event
            if (DataChangedEvent != null)
            {
                timeTracker = TimeTracker.Start();

                // Don't let errors in UI break our data code
                try
                {
                    DataChangedEvent(this, dataChangedEvent);
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                timeTracker.End(3, "CommitChanges sending out DataChangedEvent");
            }

            NotifyDataChangedEventHandlers(dataChangedEvent);

            return new CommitChangesResponse()
            {
                NeedsAccountToBeSaved = needsSave
            };

            // TODO - update tiles and toasts (should be done on a separate thread since not data-lock critical, but if another change comes in we should cancel the previous thread and simply complete the update on the new thread)
        }

        private DeletedItems RecursiveDelete(Guid[] identifiersToDelete)
        {
            DeletedItems into = new DeletedItems();

            RecursiveDeleteHelper(identifiersToDelete, into);

            return into;
        }

        private void RecursiveDeleteHelper(Guid[] identifiersToDelete, DeletedItems into)
        {
            // If too many, we need to batch it
            if (identifiersToDelete.Length > 500)
            {
                foreach (Guid[] batched in identifiersToDelete.BatchAsArrays(500))
                    RecursiveDeleteHelper(batched, into);

                return;
            }

            if (identifiersToDelete.Length == 0)
                return;

            // Delete the current level items
            Delete(identifiersToDelete, into);

            // And add those to deleted identifiers
            into.DeletedIdentifiers.AddRange(identifiersToDelete);

            // And now we need to find all children and then recursively delete those
            Guid[] childrenToDelete = FindAllIdentifiersThatAreChildren(identifiersToDelete);
            RecursiveDeleteHelper(childrenToDelete, into);
        }

        /// <summary>
        /// Doesn't enumerate all tables unless necessary. Enumerates item by item so all of them aren't loaded at once.
        /// </summary>
        /// <param name="identifiersToLookFor"></param>
        /// <returns></returns>
        private IEnumerable<BaseDataItem> FindAll(Guid[] identifiersToLookFor)
        {
            int countFound = 0;

            foreach (var i in FindAll(identifiersToLookFor, ActualTableMegaItems))
            {
                yield return i;
                countFound++;
            }

            if (countFound == identifiersToLookFor.Length)
                yield break;



            foreach (var i in FindAll(identifiersToLookFor, ActualTableGrades))
            {
                yield return i;
                countFound++;
            }

            if (countFound == identifiersToLookFor.Length)
                yield break;



            foreach (var i in FindAll(identifiersToLookFor, ActualTableClasses))
            {
                yield return i;
                countFound++;
            }

            if (countFound == identifiersToLookFor.Length)
                yield break;



            foreach (var i in FindAll(identifiersToLookFor, ActualTableSchedules))
            {
                yield return i;
                countFound++;
            }

            if (countFound == identifiersToLookFor.Length)
                yield break;



            foreach (var i in FindAll(identifiersToLookFor, ActualTableSemesters))
            {
                yield return i;
                countFound++;
            }

            if (countFound == identifiersToLookFor.Length)
                yield break;



            foreach (var i in FindAll(identifiersToLookFor, ActualTableYears))
            {
                yield return i;
                countFound++;
            }

            if (countFound == identifiersToLookFor.Length)
                yield break;



            foreach (var i in FindAll(identifiersToLookFor, ActualTableWeightCategories))
            {
                yield return i;
                countFound++;
            }

            if (countFound == identifiersToLookFor.Length)
                yield break;
        }

        private IEnumerable<BaseDataItem> FindAll<T>(Guid[] identifiersToLookFor, TableQuery<T> table) where T : BaseDataItem
        {
            return table.Where(i => identifiersToLookFor.Contains(i.Identifier));
        }

        private Guid[] FindAllIdentifiersThatAreChildren(Guid[] parentIdentifiers)
        {
            List<Guid> children = new List<Guid>();

            children.AddRange(FindIdentifiersThatAreChildren(parentIdentifiers, ActualTableMegaItems));

            children.AddRange(FindIdentifiersThatAreChildren(parentIdentifiers, ActualTableGrades));

            children.AddRange(FindIdentifiersThatAreChildren(parentIdentifiers, ActualTableSchedules));

            children.AddRange(FindIdentifiersThatAreChildren(parentIdentifiers, ActualTableClasses));

            children.AddRange(FindIdentifiersThatAreChildren(parentIdentifiers, ActualTableSemesters));

            children.AddRange(FindIdentifiersThatAreChildren(parentIdentifiers, ActualTableWeightCategories));
            
            // Years don't count since they can't be children

            return children.ToArray();
        }

        private IEnumerable<Guid> FindIdentifiersThatAreChildren<T>(Guid[] parentIdentifiers, TableQuery<T> table) where T : BaseDataItemUnderOne
        {
            // Have to convert to a class first, since directly selecting identifier doesn't work (it tries to input properties into a Guid class...)

            var items = table.Where(i => parentIdentifiers.Contains(i.UpperIdentifier)).ToArray().Select(i => i.Identifier).ToArray();
            
            return items;
        }

        private IEnumerable<Guid> FindIdentifiersThatAreChildrenOfEitherParent<T>(Guid[] parentIdentifiers, TableQuery<T> table) where T : BaseDataItemUnderTwo
        {
            // Parameters gets doubled since we're checking two params, so we'll make our limit be 800 total to stay below the 999 SQL limit
            if (parentIdentifiers.Length > 400)
            {
                List<Guid> answer = new List<Guid>();

                foreach (Guid[] batched in parentIdentifiers.BatchAsArrays(400))
                    answer.AddRange(FindIdentifiersThatAreChildrenOfEitherParent(batched, table));

                return answer;
            }

            // Have to convert to a class first, since directly selecting identifier doesn't work (it tries to input properties into a Guid class...)
            return table.Where(i => parentIdentifiers.Contains(i.UpperIdentifier) || parentIdentifiers.Contains(i.SecondUpperIdentifier)).Select(i => i.Identifier).ToArray();
        }

        private int DeleteMegaItems(Guid[] identifiersToDelete, DeletedItems into)
        {
            int before = into.DeletedHomeworkExamAppointments.Count;

            // Add the appointment LocalId's of the items we're going to delete
            // Mono has an exception when selecting the string, so first have to create the whole object...
            into.DeletedHomeworkExamAppointments.AddRange(ActualTableMegaItems.Where(i =>
                (i.MegaItemType != MegaItemType.Holiday)
                && identifiersToDelete.Contains(i.Identifier))
                .ToArray().Select(i => i.AppointmentLocalId));

            int after = into.DeletedHomeworkExamAppointments.Count;

            // If there possibly was a deleted holiday
            if (identifiersToDelete.Length != (after - before) && !into.DidDeleteHoliday)
            {
                into.DidDeleteHoliday = TableMegaItems.Any(i => i.MegaItemType == MegaItemType.Holiday && identifiersToDelete.Contains(i.Identifier));
            }

            // If there were none to delete
            if (before == after && !into.DidDeleteHoliday)
                return 0;

            // And then actually delete them, returning the count of deleted
            return ActualTableMegaItems.Delete(i => identifiersToDelete.Contains(i.Identifier));
        }

        private int DeleteSchedules(Guid[] identifiersToDelete, DeletedItems into)
        {
            int before = into.DeletedScheduleAppointments.Count;

            // Add the appointment LocalId's of the items we're going to delete
            // Mono has an exception when selecting the string, so first have to create the whole object...
            into.DeletedScheduleAppointments.AddRange(ActualTableSchedules.Where(i => identifiersToDelete.Contains(i.Identifier)).ToArray().Select(i => i.AppointmentLocalId));

            int after = into.DeletedScheduleAppointments.Count;

            // If there were none to delete
            if (before == after)
                return 0;

            // And then actually delete them, returning the count of deleted
            return ActualTableSchedules.Delete(i => identifiersToDelete.Contains(i.Identifier));
        }

        private void Delete(Guid[] identifiersToDelete, DeletedItems into)
        {
            int countDeleted = 0;

            countDeleted += DeleteMegaItems(identifiersToDelete, into);

            if (countDeleted >= identifiersToDelete.Length)
                return;

            countDeleted += ActualTableGrades.Delete(i => identifiersToDelete.Contains(i.Identifier));

            if (countDeleted >= identifiersToDelete.Length)
                return;

            countDeleted += DeleteSchedules(identifiersToDelete, into);

            if (countDeleted >= identifiersToDelete.Length)
                return;

            countDeleted += ActualTableClasses.Delete(i => identifiersToDelete.Contains(i.Identifier));

            if (countDeleted >= identifiersToDelete.Length)
                return;

            countDeleted += ActualTableSemesters.Delete(i => identifiersToDelete.Contains(i.Identifier));

            if (countDeleted >= identifiersToDelete.Length)
                return;

            countDeleted += ActualTableWeightCategories.Delete(i => identifiersToDelete.Contains(i.Identifier));

            if (countDeleted >= identifiersToDelete.Length)
                return;

            countDeleted += ActualTableYears.Delete(i => identifiersToDelete.Contains(i.Identifier));

            if (countDeleted >= identifiersToDelete.Length)
                return;
        }

        private List<BaseDataItem> GetExistingItems(IEnumerable<BaseDataItem> itemsToMatch)
        {
            List<BaseDataItem> existingItems = new List<BaseDataItem>();

            AddExistingItemsOfType<DataItemClass>(itemsToMatch, existingItems);
            AddExistingItemsOfType<DataItemGrade>(itemsToMatch, existingItems);
            AddExistingItemsOfType<DataItemMegaItem>(itemsToMatch, existingItems);
            AddExistingItemsOfType<DataItemSchedule>(itemsToMatch, existingItems);
            AddExistingItemsOfType<DataItemSemester>(itemsToMatch, existingItems);
            AddExistingItemsOfType<DataItemWeightCategory>(itemsToMatch, existingItems);
            AddExistingItemsOfType<DataItemYear>(itemsToMatch, existingItems);

            return existingItems;
        }

        private void AddExistingItemsOfType<T>(IEnumerable allItemsToMatch, List<BaseDataItem> listToAddTo) where T : BaseDataItem, new()
        {
            // Only take 500 at once since SQL can only have max of 999 parameters (and might as well stay further below that limit)
            foreach (Guid[] identifiersBatchGroup in allItemsToMatch.OfType<T>().Select(i => i.Identifier).BatchAsArrays(500))
            {
                if (identifiersBatchGroup.Length == 0)
                    return;

                listToAddTo.AddRange(_db.Table<T>().Where(i => identifiersBatchGroup.Contains(i.Identifier)));
            }
        }

        private List<BaseDataItem> GetExistingItems(UpdatedItems updatedItems)
        {
            List<BaseDataItem> existingItems = new List<BaseDataItem>();

            if (updatedItems.Classes.Count > 0)
                existingItems.AddRange(TableClasses.Where(i => updatedItems.Classes.Select(x => x.Identifier).Contains(i.Identifier)));

            if (updatedItems.MegaItems.Count > 0)
                existingItems.AddRange(TableMegaItems.Where(i => updatedItems.MegaItems.Select(x => x.Identifier).Contains(i.Identifier)));

            if (updatedItems.Grades.Count > 0)
                existingItems.AddRange(TableGrades.Where(i => updatedItems.Grades.Select(x => x.Identifier).Contains(i.Identifier)));

            if (updatedItems.Schedules.Count > 0)
                existingItems.AddRange(TableSchedules.Where(i => updatedItems.Schedules.Select(x => x.Identifier).Contains(i.Identifier)));

            if (updatedItems.Semesters.Count > 0)
                existingItems.AddRange(TableSemesters.Where(i => updatedItems.Semesters.Select(x => x.Identifier).Contains(i.Identifier)));

            if (updatedItems.WeightCategories.Count > 0)
                existingItems.AddRange(TableWeightCategories.Where(i => updatedItems.WeightCategories.Select(x => x.Identifier).Contains(i.Identifier)));

            if (updatedItems.Years.Count > 0)
                existingItems.AddRange(TableYears.Where(i => updatedItems.Years.Select(x => x.Identifier).Contains(i.Identifier)));

            return existingItems;
        }

        public DataItemYear[] GetYears()
        {
            return TableYears.ToArray();
        }

        public DataItemSemester[] GetSemesters()
        {
            return TableSemesters.ToArray();
        }

        public DataItemSemester GetSemester(Guid identifier)
        {
            return TableSemesters.FirstOrDefault(i => i.Identifier == identifier);
        }


#region GetClasses

        public Guid[] GetClassIdentifiersUnderSemester(Guid semesterIdentifier)
        {
            return _db.Query<JustIdentifier>($"select {nameof(DataItemClass.Identifier)} from {ActualTableClasses.Table.TableName} where {nameof(DataItemClass.UpperIdentifier)} = ?", semesterIdentifier)
                .Select(i => i.Identifier)
                .ToArray();
        }

        private class JustIdentifier
        {
            public Guid Identifier { get; set; }
        }

#endregion


        public static BaseDataItem[] GenerateNewDefaultClass(Guid semesterId, string name, byte[] rawColor)
        {
            DataItemClass c = new DataItemClass()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = semesterId,
                Credits = Grade.NO_CREDITS,
                DoesRoundGradesUp = true,
                Name = name,
                RawColor = rawColor
            };
            
            c.SetGradeScales(GradeScale.GenerateDefaultScaleWithoutLetters());

            DataItemWeightCategory weight = new DataItemWeightCategory()
            {
                Identifier = Guid.NewGuid(),
                Name = "All Grades",
                WeightValue = 100,
                UpperIdentifier = c.Identifier
            };

            return new BaseDataItem[] { c, weight };
        }

        public static DataItemWeightCategory CreateDefaultWeightCategory(Guid classIdentifier)
        {
            return new DataItemWeightCategory()
            {
                Identifier = Guid.NewGuid(),
                Name = "All Grades",
                WeightValue = 100,
                UpperIdentifier = classIdentifier
            };
        }

        /// <summary>
        /// Returns Guid.Empty if class not found
        /// </summary>
        /// <param name="classId"></param>
        /// <returns></returns>
        public async Task<Guid> GetSemesterIdForClassAsync(Guid classId)
        {
            return await System.Threading.Tasks.Task.Run(delegate
            {
                return GetSemesterIdForClassBlocking(classId);
            });
        }

        private async Task<Guid> GetSemesterIdForClassBlocking(Guid classId)
        {
            using (await Locks.LockDataForReadAsync())
            {
                return TableClasses.Where(i => i.Identifier == classId).Select(i => i.UpperIdentifier).FirstOrDefault();
            }
        }

#if ANDROID
        public async System.Threading.Tasks.Task MarkAndroidRemindersSent(Guid[] homeworkIds, Guid[] examIds)
        {
            await System.Threading.Tasks.Task.Run(async delegate
            {
                using (await Locks.LockDataForWriteAsync())
                {
                    _db.BeginTransaction();

                    try
                    {
                        MarkAndroidRemindersSent(ActualTableMegaItems.Table.TableName, homeworkIds.Concat(examIds).ToArray());
                    }

                    finally
                    {
                        _db.Commit();
                    }
                }
            });
        }

        private void MarkAndroidRemindersSent(string table, Guid[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return;
            }

            foreach (var id in ids)
            {
                _db.Execute($"update {table} set HasSentReminder = 1 where Identifier = ?", id);
            }
        }
#endif
    }
}
