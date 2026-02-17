using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using Microsoft.EntityFrameworkCore;
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
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System.ComponentModel.DataAnnotations;

/*  ClassAttribute
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
*/

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public class DeletedItems : IEnumerable<Guid>
    {
        public List<Guid> DeletedIdentifiers = new List<Guid>();

        /// <summary>
        /// A list of the AppointmentLocalId's of tasks or events that were deleted
        /// </summary>
        public List<string> DeletedTaskEventAppointments = new List<string>();

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

            DeletedTaskEventAppointments.AddRange(newer.DeletedTaskEventAppointments);
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

        public System.Threading.Tasks.Task UpdateClassRemindersTask { get; internal set; }

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

            if (UpdateClassRemindersTask != null)
            {
                try
                {
                    await UpdateClassRemindersTask;
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

        /// <summary>
        /// A collection of ids of items that are purely for editing (if item doesn't exist, don't commit any changes).
        /// </summary>
        private HashSet<Guid> _onlyEditItems;

        private bool DoesGuidExist(Guid id)
        {
            return _storage.ContainsKey(id);
        }

        /// <summary>
        /// Adds a new or edited item
        /// </summary>
        /// <param name="item"></param>
        public void Add(BaseDataItem item, bool throwIfExists = true, bool onlyEdit = false)
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

            if (onlyEdit)
            {
                if (_onlyEditItems == null)
                {
                    _onlyEditItems = new HashSet<Guid>();
                }

                _onlyEditItems.Add(item.Identifier);
            }
        }

        public bool ShouldCreateNewItem(Guid identifier)
        {
            return _onlyEditItems == null || !_onlyEditItems.Contains(identifier);
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
            private IQueryable<T> _queryable;

            public AccountApplier(AccountDataItem account, IQueryable<T> queryable)
            {
                _account = account;
                _queryable = queryable;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _queryable.AsEnumerable().Select(i => ApplyAccount(i)).GetEnumerator();
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

            public int Count() { return _queryable.Count(); }
            public int Count(Expression<Func<T, bool>> predExpr) { return _queryable.Count(predExpr); }
            public T ElementAt(int index) { return ApplyAccount(_queryable.AsEnumerable().ElementAt(index)); }
            public T First() { return ApplyAccount(_queryable.First()); }
            public T First(Expression<Func<T, bool>> predExpr) { return ApplyAccount(_queryable.First(predExpr)); }
            public T FirstOrDefault() { return ApplyAccount(_queryable.FirstOrDefault()); }
            public T FirstOrDefault(Expression<Func<T, bool>> predExpr) { return ApplyAccount(_queryable.FirstOrDefault(predExpr)); }
            public AccountApplier<T> Where(Expression<Func<T, bool>> predExpr) { return new AccountApplier<T>(_account, _queryable.Where(predExpr)); }
        }

        public AccountDataItem Account { get; private set; }

        private ChangedItems _loadedChangedItems;

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

            public void MarkAllDeletesSent()
            {
                foreach (var pair in _items.Where(i => i.Value.Type == ChangedPropertiesOfDataItem.ChangeType.Deleted))
                {
                    pair.Value.MarkSent();
                }
            }

            public void MarkUpdatesSent(IEnumerable<Guid> identifiers)
            {
                foreach (var pair in _items.Where(i => identifiers.Contains(i.Key)))
                {
                    pair.Value.MarkSent();
                }
            }
        }

        public Guid LocalAccountId
        {
            get { return Account.LocalAccountId; }
        }
        public PowerPlannerDbContext _db;

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
                    catch (Exception ex) when (!(ex is OutOfMemoryException))
                    {
                        // Database corrupted, delete and re-create
                        TelemetryExtension.Current?.TrackException(ex);

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
                        existing._db.Dispose();
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
        private AccountDataStore(AccountDataItem account)
        {
            Account = account;
        }

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
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);

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
            _db = new PowerPlannerDbContext(DatabaseFilePath, Account);

            // EnsureCreated creates all tables if the database doesn't exist,
            // and is a no-op if the database already exists (it does NOT update schema).
            // This matches the old CreateTable behavior which was also a no-op for existing tables.
            _db.Database.EnsureCreated();
            timeTracker.End(3, "AccountDataStore.InitializeDatabase create DbContext and EnsureCreated");

            // Handle upgrading data
            timeTracker = TimeTracker.Start();
            var dataInfo = _db.DataInfos.FirstOrDefault();
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
                // The old Homework/Exam tables have been migrated to MegaItem in a previous release.
                // For databases still on version < 3, we use raw SQL to read from the legacy tables
                // and insert into the MegaItem table, then drop the legacy tables.
                await UpgradeFromVersion2Async(version);
            }
            if (version < 4)
            {
                // Added StartDate/EndDate to classes
                _db.Database.ExecuteSqlRaw("update DataItemClass set StartDate = {0}, EndDate = {1}", SqlDate.MinValue, SqlDate.MinValue);
            }
            if (version < 5)
            {
                // Added PassingGrade to classes, which needs to be set to 60% by default
                _db.Database.ExecuteSqlRaw("update DataItemClass set PassingGrade = {0}", Class.DefaultPassingGrade);
            }
            if (version < 6 && version == 5)
            {
                // In previous version 5, I accidently was saving a class with the identifier of a semester, wiping out semesters
                try
                {
                    int countDeleted = _db.Database.ExecuteSqlRaw("delete from DataItemClass where exists (select * from DataItemSemester where DataItemClass.Identifier = DataItemSemester.Identifier and DataItemClass.UpperIdentifier = {0})", Guid.Empty);

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
                            string name = _db.Semesters
                                .Where(s => s.Identifier == semesterToReUpload)
                                .Select(s => s.Name)
                                .FirstOrDefault();
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

                // Upsert: check if tracked or in DB, then add or update
                var existingInDb = _db.DataInfos.Find(dataInfo.Key);
                if (existingInDb != null)
                {
                    existingInDb.Version = dataInfo.Version;
                }
                else
                {
                    _db.DataInfos.Add(dataInfo);
                }
                _db.SaveChanges();
            }
            timeTracker.End(3, "AccountDataStore.InitializeDatabase handle upgrade");

            if (needsAppointmentReset)
            {
                AppointmentsExtension.Current?.ResetAll(Account, this);
            }
        }

        /// <summary>
        /// Handles the v1/v2 to v3 upgrade: migrating legacy Homework/Exam tables to MegaItem.
        /// Uses raw SQL since the legacy entity types are no longer mapped in EF Core.
        /// </summary>
        private async System.Threading.Tasks.Task UpgradeFromVersion2Async(int version)
        {
            // Check if the old DataItemHomework table exists
            var connection = _db.Database.GetDbConnection();
            await connection.OpenAsync();

            bool homeworkTableExists = false;
            try
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='DataItemHomework'";
                    var result = cmd.ExecuteScalar();
                    homeworkTableExists = Convert.ToInt64(result) > 0;
                }
            }
            catch
            {
                // If we can't check, assume the table doesn't exist
            }

            if (!homeworkTableExists)
                return;

            var classes = TableClasses.ToArray();

            // Read all homework items using raw SQL and insert as MegaItems
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT Identifier, DateCreated, Updated, UpperIdentifier, Name, Details, ImageNames, Date, GradeReceived, GradeTotal, IsDropped, IndividualWeight, EndTime, Reminder, WeightCategoryIdentifier, AppointmentLocalId, PercentComplete"
#if ANDROID
                    + ", HasSentReminder"
#endif
                    + " FROM DataItemHomework";

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var upperIdentifier = reader.GetGuid(3);
                        var c = classes.FirstOrDefault(i => upperIdentifier == i.Identifier);
                        if (c == null)
                            continue;

                        var megaItem = new DataItemMegaItem()
                        {
                            MegaItemType = MegaItemType.Homework,
                            Identifier = reader.GetGuid(0),
                            DateCreated = reader.GetDateTime(1),
                            Updated = reader.GetDateTime(2),
                            UpperIdentifier = c.Identifier,
                            Name = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Details = reader.IsDBNull(5) ? null : reader.GetString(5),
                            RawImageNames = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Date = reader.GetDateTime(7),
                            GradeReceived = reader.GetDouble(8),
                            GradeTotal = reader.GetDouble(9),
                            IsDropped = reader.GetBoolean(10),
                            IndividualWeight = reader.GetDouble(11),
                            EndTime = reader.GetDateTime(12),
                            Reminder = reader.GetDateTime(13),
                            WeightCategoryIdentifier = reader.GetGuid(14),
                            AppointmentLocalId = reader.IsDBNull(15) ? null : reader.GetString(15),
                            PercentComplete = reader.GetDouble(16),
#if ANDROID
                            HasSentReminder = reader.GetBoolean(17),
#endif
                        };

                        if (version < 2)
                        {
                            megaItem.GradeReceived = Grade.UNGRADED;
                            megaItem.GradeTotal = 100;
                            megaItem.IsDropped = false;
                            megaItem.IndividualWeight = 1;
                            megaItem.WeightCategoryIdentifier = Guid.Empty;
                        }

                        _db.MegaItems.Add(megaItem);
                    }
                }
            }

            // Read all exam items using raw SQL and insert as MegaItems
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT Identifier, DateCreated, Updated, UpperIdentifier, Name, Details, ImageNames, Date, GradeReceived, GradeTotal, IsDropped, IndividualWeight, EndTime, Reminder, WeightCategoryIdentifier, AppointmentLocalId"
#if ANDROID
                    + ", HasSentReminder"
#endif
                    + " FROM DataItemExam";

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var upperIdentifier = reader.GetGuid(3);
                        var c = classes.FirstOrDefault(i => upperIdentifier == i.Identifier);
                        if (c == null)
                            continue;

                        var megaItem = new DataItemMegaItem()
                        {
                            MegaItemType = MegaItemType.Exam,
                            Identifier = reader.GetGuid(0),
                            DateCreated = reader.GetDateTime(1),
                            Updated = reader.GetDateTime(2),
                            UpperIdentifier = c.Identifier,
                            Name = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Details = reader.IsDBNull(5) ? null : reader.GetString(5),
                            RawImageNames = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Date = reader.GetDateTime(7),
                            GradeReceived = reader.GetDouble(8),
                            GradeTotal = reader.GetDouble(9),
                            IsDropped = reader.GetBoolean(10),
                            IndividualWeight = reader.GetDouble(11),
                            EndTime = reader.GetDateTime(12),
                            Reminder = reader.GetDateTime(13),
                            WeightCategoryIdentifier = reader.GetGuid(14),
                            AppointmentLocalId = reader.IsDBNull(15) ? null : reader.GetString(15),
#if ANDROID
                            HasSentReminder = reader.GetBoolean(16),
#endif
                        };

                        if (version < 2)
                        {
                            megaItem.GradeReceived = Grade.UNGRADED;
                            megaItem.GradeTotal = 100;
                            megaItem.IsDropped = false;
                            megaItem.IndividualWeight = 1;
                            megaItem.WeightCategoryIdentifier = Guid.Empty;
                        }

                        _db.MegaItems.Add(megaItem);
                    }
                }
            }

            _db.SaveChanges();

            // Drop the legacy tables
            _db.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS DataItemHomework");
            _db.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS DataItemExam");
        }

        public class DataInfo
        {
            public const int LATEST_VERSION = 7;

            [Key]
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
                    _db.Dispose();
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }

                _db = null;
            }
        }

        public IQueryable<ImageToUpload> TableImagesToUpload
        {
            get { return _db.ImagesToUpload; }
        }

        public AccountApplier<DataItemClass> TableClasses
        {
            get { return new AccountApplier<DataItemClass>(Account, _db.Classes); }
        }

        public AccountApplier<DataItemMegaItem> TableMegaItems
        {
            get { return new AccountApplier<DataItemMegaItem>(Account, _db.MegaItems); }
        }

        public AccountApplier<DataItemGrade> TableGrades
        {
            get { return new AccountApplier<DataItemGrade>(Account, _db.Grades); }
        }

        public AccountApplier<DataItemSchedule> TableSchedules
        {
            get { return new AccountApplier<DataItemSchedule>(Account, _db.Schedules); }
        }

        public AccountApplier<DataItemSemester> TableSemesters
        {
            get { return new AccountApplier<DataItemSemester>(Account, _db.Semesters); }
        }

        public AccountApplier<DataItemWeightCategory> TableWeightCategories
        {
            get { return new AccountApplier<DataItemWeightCategory>(Account, _db.WeightCategories); }
        }

        public AccountApplier<DataItemYear> TableYears
        {
            get { return new AccountApplier<DataItemYear>(Account, _db.Years); }
        }

        public IQueryable<DataItemClass> ActualTableClasses
        {
            get { return _db.Classes; }
        }

        public IQueryable<DataItemMegaItem> ActualTableMegaItems
        {
            get { return _db.MegaItems; }
        }

        public IQueryable<DataItemGrade> ActualTableGrades
        {
            get { return _db.Grades; }
        }

        public IQueryable<DataItemSchedule> ActualTableSchedules
        {
            get { return _db.Schedules; }
        }

        public IQueryable<DataItemSemester> ActualTableSemesters
        {
            get { return _db.Semesters; }
        }

        public IQueryable<DataItemWeightCategory> ActualTableWeightCategories
        {
            get { return _db.WeightCategories; }
        }

        public IQueryable<DataItemYear> ActualTableYears
        {
            get { return _db.Years; }
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
                foreach (var u in toUpload)
                {
                    var existing = _db.ImagesToUpload.Find(u);
                    if (existing == null)
                    {
                        _db.ImagesToUpload.Add(new ImageToUpload()
                        {
                            FileName = u
                        });
                    }
                }
                _db.SaveChanges();
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
            var toUpload = _db.ImagesToUpload.FirstOrDefault();

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
            var toRemove = _db.ImagesToUpload.Find(fileName);
            if (toRemove != null)
            {
                _db.ImagesToUpload.Remove(toRemove);
                _db.SaveChanges();
            }
        }

        /// <summary>
        /// This establishes a data lock
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dictionary<string, object>>, Guid[], bool>> GetUpdatesAndDeletesAsync()
        {
            return await System.Threading.Tasks.Task.Run(async delegate
            {
                ChangedItems changedItems;
                Guid[] deletes;
                IEnumerable<Dictionary<string, object>> updates;
                bool isBatchingUpdates;

                using (await Locks.LockDataForReadAsync())
                {
                    changedItems = await ChangedItems.Load(this);

                    var newItemIdentifiers = changedItems.GetAllNew();
                    var editedItems = changedItems.GetAllEdited();
                    deletes = changedItems.GetAllDeleted();

                    // If nothing changed
                    if (newItemIdentifiers.Length == 0 && editedItems.Length == 0 && deletes.Length == 0)
                        return new Tuple<IEnumerable<Dictionary<string, object>>, Guid[], bool>(new Dictionary<string, object>[0], new Guid[0], false);


                    // grab all new/edited
                    var timeTracker = TimeTracker.Start();
                    updates = GetUpdatesBlocking(newItemIdentifiers, editedItems, out isBatchingUpdates);
                    timeTracker.End(3, $"AccountDataStore.GetUpdatesandDeletesAsync GetUpdatesBlocking, {newItemIdentifiers.Length} new items, {editedItems.Length} edited items");
                }

                using (await Locks.LockDataForWriteAsync())
                {
                    // Mark all the properties as sent, so that when sync completes, we can remove them
                    if (isBatchingUpdates)
                    {
                        changedItems.MarkUpdatesSent(updates.Select(i => (Guid)i["Identifier"]).ToArray());
                        changedItems.MarkAllDeletesSent();
                    }
                    else
                    {
                        changedItems.MarkAllSent();
                    }

                    // And then save that
                    await changedItems.Save();
                }
                

                return new Tuple<IEnumerable<Dictionary<string, object>>, Guid[], bool>(updates, deletes, isBatchingUpdates);
            });
        }

        /// <summary>
        /// Caller should establish data lock
        /// </summary>
        private IEnumerable<Dictionary<string, object>> GetUpdatesBlocking(Guid[] newItemIdentifiers, Tuple<Guid, BaseDataItem.SyncPropertyNames[]>[] editedItems, out bool isBatchingUpdates)
        {
            List<Dictionary<string, object>> answer = new List<Dictionary<string, object>>();

            Guid[] newAndEditedIdentifiers = newItemIdentifiers.Union(editedItems.Select(i => i.Item1)).ToArray();
            IEnumerable<BaseDataItem> newAndEditedItems;
            const int MAX_ITEMS = 65; // Server starts batching after 65

            if (newAndEditedIdentifiers.Length > MAX_ITEMS)
            {
                newAndEditedItems = FindAllWithLimitAndSorted(newAndEditedIdentifiers, MAX_ITEMS);
                isBatchingUpdates = true;
            }
            else
            {
                newAndEditedItems = FindAll(newAndEditedIdentifiers);
                isBatchingUpdates = false;
            }

            foreach (var item in newAndEditedItems)
            {
                BaseDataItem.SyncPropertyNames[] changedProperties = editedItems.Where(i => i.Item1 == item.Identifier).Select(i => i.Item2).FirstOrDefault();

                // If edited, get properties selectively
                if (changedProperties != null)
                {
                    Dictionary<string, object> changes = new Dictionary<string, object>();

                    changes["Identifier"] = item.Identifier;
                    changes["Updated"] = item.Updated;
                    changes["ItemType"] = item.ItemType;

                    foreach (var p in changedProperties)
                    {
                        changes[p.ToString()] = item.GetPropertyValue(p);
                    }

                    answer.Add(changes);
                }
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

                try
                {
                    Debug.WriteLine("Updating class reminders");
                    pendingTasks.UpdateClassRemindersTask = ClassRemindersExtension.Current?.ResetAllRemindersAsync(account);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to update class reminders");
                    TelemetryExtension.Current?.TrackException(ex);
                }

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

        internal async System.Threading.Tasks.Task ImportItemsAsync(BaseDataItem[] items)
        {
            await System.Threading.Tasks.Task.Run(delegate
            {
                ImportItemsBlocking(items);
            });
        }

        private void ImportItemsBlocking(BaseDataItem[] items)
        {
            AddItemsToDbSets(items);
            _db.SaveChanges();
        }

        /// <summary>
        /// Adds items to the appropriate DbSet based on their type
        /// </summary>
        private void AddItemsToDbSets(IEnumerable<BaseDataItem> items)
        {
            foreach (var item in items)
            {
                switch (item)
                {
                    case DataItemClass c: _db.Classes.Add(c); break;
                    case DataItemMegaItem m: _db.MegaItems.Add(m); break;
                    case DataItemGrade g: _db.Grades.Add(g); break;
                    case DataItemSchedule s: _db.Schedules.Add(s); break;
                    case DataItemSemester sem: _db.Semesters.Add(sem); break;
                    case DataItemWeightCategory w: _db.WeightCategories.Add(w); break;
                    case DataItemYear y: _db.Years.Add(y); break;
                }
            }
        }

        private class CommitChangesResponse
        {
            public bool NeedsAccountToBeSaved { get; set; }
        }

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
                if (processType == ProcessType.Local)
                    edited.Updated = now;

                BaseDataItem existing = existingDataItems.FirstOrDefault(i => i.Identifier == edited.Identifier);

                if (existing == null)
                {
                    if (dataChanges.ShouldCreateNewItem(edited.Identifier))
                    {
                        BaseDataItem newItem = edited;

                        if (processType == ProcessType.Local)
                            newItem.DateCreated = now;

                        if (changedItems != null)
                            changedItems.AddNewItem(newItem.Identifier);

                        newDataItems.Add(newItem);
                    }
                }
                else
                {
                    existing.Account = Account;
                    var changedPropertyNames = existing.ImportChanges(edited);

#if ANDROID
                    if (existing is DataItemMegaItem)
                    {
                        (existing as DataItemMegaItem).HasSentReminder = false;
                    }
#endif

                    if (changedItems != null)
                        changedItems.AddEditedItem(existing.Identifier, changedPropertyNames);
                }

                now = now.AddTicks(1);
            }

            if (changedItems != null)
                foreach (Guid id in dataChanges.IdentifiersToDelete)
                    changedItems.AddDeletedItem(id);

            DeletedItems deletedItems;

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // Existing items are already tracked by EF Core from GetExistingItems,
                    // so changes will be detected and saved automatically
                    timeTracker = TimeTracker.Start();
                    timeTracker.End(3, $"CommitChanges existing items tracked, {existingDataItems.Count} existingDataItems");

                    // Add the new items
                    timeTracker = TimeTracker.Start();
                    AddItemsToDbSets(newDataItems);
                    timeTracker.End(3, $"CommitChanges AddItemsToDbSets, {newDataItems.Count} newDataItems");

                    // And delete the deleted items
                    timeTracker = TimeTracker.Start();
                    deletedItems = RecursiveDelete(dataChanges.IdentifiersToDelete.ToArray());
                    timeTracker.End(3, $"CommitChanges RecursiveDelete, {dataChanges.IdentifiersToDelete.Count()} identifiers to delete");

                    // Save all tracked changes to the database
                    _db.SaveChanges();

                    if (changedItems != null)
                        await changedItems.Save();

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            DataChangedEvent dataChangedEvent = new DataChangedEvent(LocalAccountId, newDataItems, existingDataItems, deletedItems, wasLocalChanges: processType == ProcessType.Local, originalChanges: dataChanges);

            bool needsSave = false;

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

            if (DataChangedEvent != null)
            {
                timeTracker = TimeTracker.Start();

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
        }

        private DeletedItems RecursiveDelete(Guid[] identifiersToDelete)
        {
            DeletedItems into = new DeletedItems();

            RecursiveDeleteHelper(identifiersToDelete, into);

            return into;
        }

        private void RecursiveDeleteHelper(Guid[] identifiersToDelete, DeletedItems into)
        {
            if (identifiersToDelete.Length > 500)
            {
                foreach (Guid[] batched in identifiersToDelete.BatchAsArrays(500))
                    RecursiveDeleteHelper(batched, into);

                return;
            }

            if (identifiersToDelete.Length == 0)
                return;

            Delete(identifiersToDelete, into);
            into.DeletedIdentifiers.AddRange(identifiersToDelete);

            Guid[] childrenToDelete = FindAllIdentifiersThatAreChildren(identifiersToDelete);
            RecursiveDeleteHelper(childrenToDelete, into);
        }

        private IEnumerable<BaseDataItem> FindAll(Guid[] identifiersToLookFor)
        {
            HashSet<Guid> remainingIdentifiersToLookFor = new HashSet<Guid>(identifiersToLookFor);

            Func<BaseDataItem, BaseDataItem> found = (BaseDataItem i) =>
            {
                remainingIdentifiersToLookFor.Remove(i.Identifier);
                return i;
            };

            Func<bool> done = () => { return remainingIdentifiersToLookFor.Count == 0; };

            foreach (var i in FindAll(identifiersToLookFor, ActualTableMegaItems))
            { yield return found(i); }
            if (done()) yield break;

            foreach (var i in FindAll(identifiersToLookFor, ActualTableGrades))
            { yield return found(i); }
            if (done()) yield break;

            foreach (var i in FindAll(identifiersToLookFor, ActualTableClasses))
            { yield return found(i); }
            if (done()) yield break;

            foreach (var i in FindAll(identifiersToLookFor, ActualTableSchedules))
            { yield return found(i); }
            if (done()) yield break;

            foreach (var i in FindAll(identifiersToLookFor, ActualTableSemesters))
            { yield return found(i); }
            if (done()) yield break;

            foreach (var i in FindAll(identifiersToLookFor, ActualTableYears))
            { yield return found(i); }
            if (done()) yield break;

            foreach (var i in FindAll(identifiersToLookFor, ActualTableWeightCategories))
            { yield return found(i); }
        }

        private IEnumerable<BaseDataItem> FindAllWithLimitAndSorted(Guid[] identifiersToLookFor, int maxItemsToReturn)
        {
            HashSet<Guid> remainingIdentifiersToLookFor = new HashSet<Guid>(identifiersToLookFor);
            int countFound = 0;

            Func<BaseDataItem, BaseDataItem> found = (BaseDataItem i) =>
            {
                countFound++;
                remainingIdentifiersToLookFor.Remove(i.Identifier);
                return i;
            };

            Func<bool> done = () => { return countFound >= maxItemsToReturn || remainingIdentifiersToLookFor.Count == 0; };

            foreach (var i in FindAll(identifiersToLookFor, ActualTableYears))
            { yield return found(i); if (done()) yield break; }

            foreach (var i in FindAll(identifiersToLookFor, ActualTableSemesters))
            { yield return found(i); if (done()) yield break; }

            foreach (var i in FindAll(identifiersToLookFor, ActualTableClasses))
            { yield return found(i); if (done()) yield break; }

            foreach (var i in FindAll(identifiersToLookFor, ActualTableWeightCategories))
            { yield return found(i); if (done()) yield break; }

            foreach (var i in FindAll(identifiersToLookFor, ActualTableMegaItems))
            { yield return found(i); if (done()) yield break; }

            foreach (var i in FindAll(identifiersToLookFor, ActualTableGrades))
            { yield return found(i); if (done()) yield break; }

            foreach (var i in FindAll(identifiersToLookFor, ActualTableSchedules))
            { yield return found(i); if (done()) yield break; }
        }

        private IEnumerable<BaseDataItem> FindAll<T>(Guid[] identifiersToLookFor, IQueryable<T> table) where T : BaseDataItem
        {
            const int max = 900;
            if (identifiersToLookFor.Length > max)
            {
                foreach (var grouped in identifiersToLookFor.BatchAsArrays(900))
                {
                    foreach (var item in table.Where(i => grouped.Contains(i.Identifier)).AsEnumerable())
                    {
                        yield return item;
                    }
                }
            }
            else
            {
                foreach (var item in table.Where(i => identifiersToLookFor.Contains(i.Identifier)).AsEnumerable())
                {
                    yield return item;
                }
            }
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

            return children.ToArray();
        }

        private IEnumerable<Guid> FindIdentifiersThatAreChildren<T>(Guid[] parentIdentifiers, IQueryable<T> table) where T : BaseDataItemUnderOne
        {
            var items = table.Where(i => parentIdentifiers.Contains(i.UpperIdentifier)).Select(i => i.Identifier).ToArray();
            return items;
        }

        private IEnumerable<Guid> FindIdentifiersThatAreChildrenOfEitherParent<T>(Guid[] parentIdentifiers, IQueryable<T> table) where T : BaseDataItemUnderTwo
        {
            if (parentIdentifiers.Length > 400)
            {
                List<Guid> answer = new List<Guid>();
                foreach (Guid[] batched in parentIdentifiers.BatchAsArrays(400))
                    answer.AddRange(FindIdentifiersThatAreChildrenOfEitherParent(batched, table));
                return answer;
            }

            return table.Where(i => parentIdentifiers.Contains(i.UpperIdentifier) || parentIdentifiers.Contains(i.SecondUpperIdentifier)).Select(i => i.Identifier).ToArray();
        }

        private int DeleteMegaItems(Guid[] identifiersToDelete, DeletedItems into)
        {
            int before = into.DeletedTaskEventAppointments.Count;

            into.DeletedTaskEventAppointments.AddRange(ActualTableMegaItems.Where(i =>
                (i.MegaItemType != MegaItemType.Holiday)
                && identifiersToDelete.Contains(i.Identifier))
                .AsEnumerable().Select(i => i.AppointmentLocalId));

            int after = into.DeletedTaskEventAppointments.Count;

            if (identifiersToDelete.Length != (after - before) && !into.DidDeleteHoliday)
            {
                into.DidDeleteHoliday = TableMegaItems.Any(i => i.MegaItemType == MegaItemType.Holiday && identifiersToDelete.Contains(i.Identifier));
            }

            if (before == after && !into.DidDeleteHoliday)
                return 0;

            var toDelete = _db.MegaItems.Where(i => identifiersToDelete.Contains(i.Identifier)).ToList();
            _db.MegaItems.RemoveRange(toDelete);
            return toDelete.Count;
        }

        private int DeleteSchedules(Guid[] identifiersToDelete, DeletedItems into)
        {
            int before = into.DeletedScheduleAppointments.Count;

            into.DeletedScheduleAppointments.AddRange(ActualTableSchedules.Where(i => identifiersToDelete.Contains(i.Identifier)).AsEnumerable().Select(i => i.AppointmentLocalId));

            int after = into.DeletedScheduleAppointments.Count;

            if (before == after)
                return 0;

            var toDelete = _db.Schedules.Where(i => identifiersToDelete.Contains(i.Identifier)).ToList();
            _db.Schedules.RemoveRange(toDelete);
            return toDelete.Count;
        }

        private int DeleteFromDbSet<T>(Guid[] identifiersToDelete, DbSet<T> dbSet) where T : BaseDataItem
        {
            var toDelete = dbSet.Where(i => identifiersToDelete.Contains(i.Identifier)).ToList();
            if (toDelete.Count > 0)
                dbSet.RemoveRange(toDelete);
            return toDelete.Count;
        }

        private void Delete(Guid[] identifiersToDelete, DeletedItems into)
        {
            int countDeleted = 0;

            countDeleted += DeleteMegaItems(identifiersToDelete, into);
            if (countDeleted >= identifiersToDelete.Length) return;

            countDeleted += DeleteFromDbSet(identifiersToDelete, _db.Grades);
            if (countDeleted >= identifiersToDelete.Length) return;

            countDeleted += DeleteSchedules(identifiersToDelete, into);
            if (countDeleted >= identifiersToDelete.Length) return;

            countDeleted += DeleteFromDbSet(identifiersToDelete, _db.Classes);
            if (countDeleted >= identifiersToDelete.Length) return;

            countDeleted += DeleteFromDbSet(identifiersToDelete, _db.Semesters);
            if (countDeleted >= identifiersToDelete.Length) return;

            countDeleted += DeleteFromDbSet(identifiersToDelete, _db.WeightCategories);
            if (countDeleted >= identifiersToDelete.Length) return;

            countDeleted += DeleteFromDbSet(identifiersToDelete, _db.Years);
        }

        private List<BaseDataItem> GetExistingItems(IEnumerable<BaseDataItem> itemsToMatch)
        {
            List<BaseDataItem> existingItems = new List<BaseDataItem>();

            AddExistingItemsOfType<DataItemClass>(itemsToMatch, existingItems, _db.Classes);
            AddExistingItemsOfType<DataItemGrade>(itemsToMatch, existingItems, _db.Grades);
            AddExistingItemsOfType<DataItemMegaItem>(itemsToMatch, existingItems, _db.MegaItems);
            AddExistingItemsOfType<DataItemSchedule>(itemsToMatch, existingItems, _db.Schedules);
            AddExistingItemsOfType<DataItemSemester>(itemsToMatch, existingItems, _db.Semesters);
            AddExistingItemsOfType<DataItemWeightCategory>(itemsToMatch, existingItems, _db.WeightCategories);
            AddExistingItemsOfType<DataItemYear>(itemsToMatch, existingItems, _db.Years);

            return existingItems;
        }

        private void AddExistingItemsOfType<T>(IEnumerable allItemsToMatch, List<BaseDataItem> listToAddTo, DbSet<T> dbSet) where T : BaseDataItem
        {
            foreach (Guid[] identifiersBatchGroup in allItemsToMatch.OfType<T>().Select(i => i.Identifier).BatchAsArrays(500))
            {
                if (identifiersBatchGroup.Length == 0)
                    return;

                listToAddTo.AddRange(dbSet.Where(i => identifiersBatchGroup.Contains(i.Identifier)));
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
            return _db.Classes
                .Where(c => c.UpperIdentifier == semesterIdentifier)
                .Select(c => c.Identifier)
                .ToArray();
        }

#endregion

        public static BaseDataItem[] GenerateNewDefaultClass(AccountDataItem account, Guid semesterId, string name, byte[] rawColor)
        {
            DataItemClass c = new DataItemClass()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = semesterId,
                Credits = Grade.NO_CREDITS,
                DoesRoundGradesUp = account.DefaultDoesRoundGradesUp,
                ShouldAverageGradeTotals = account.DefaultDoesAverageGradeTotals,
                Name = name,
                RawColor = rawColor
            };
            
            c.SetGradeScales(account.DefaultGradeScale);

            DataItemWeightCategory weight = CreateDefaultWeightCategory(c.Identifier);

            return new BaseDataItem[] { c, weight };
        }

        public static DataItemWeightCategory CreateDefaultWeightCategory(Guid classIdentifier)
        {
            return new DataItemWeightCategory()
            {
                Identifier = Guid.NewGuid(),
                Name = PowerPlannerResources.GetString("WeightCategory_AllGrades"),
                WeightValue = 100,
                UpperIdentifier = classIdentifier
            };
        }

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
                    var allIds = homeworkIds.Concat(examIds).ToArray();
                    foreach (var id in allIds)
                    {
                        _db.Database.ExecuteSqlRaw("update DataItemMegaItem set HasSentReminder = 1 where Identifier = {0}", id);
                    }
                }
            });
        }
#endif

        public async Task<UpcomingItemsForWidgetResponse> GetAllUpcomingItemsForWidgetAsync(DateTime today, int maxItems, bool allowCachedAgenda = true)
        {
            var account = Account;

            if (account.MainTileSettings.IsDisabled())
            {
                return new UpcomingItemsForWidgetResponse
                {
                    IsDisabledInSettings = true
                };
            }

            var currSemesterId = account.CurrentSemesterId;
            if (currSemesterId == Guid.Empty)
            {
                return new UpcomingItemsForWidgetResponse
                {
                    NoSemester = true
                };
            }

            ScheduleViewItemsGroup scheduleViewGroup;
            try
            {
                scheduleViewGroup = await ScheduleViewItemsGroup.LoadAsync(account.LocalAccountId, account.CurrentSemesterId, trackChanges: true, includeWeightCategories: false);
            }
            catch
            {
                return new UpcomingItemsForWidgetResponse
                {
                    NoSemester = true
                };
            }

            DateTime dateToStartDisplayingFrom = account.MainTileSettings.GetDateToStartDisplayingOn(today);

            var agendaViewGroup = await AgendaViewItemsGroup.LoadAsync(account.LocalAccountId, scheduleViewGroup.Semester, today, trackChanges: allowCachedAgenda);

            var items = agendaViewGroup.Items.Where(
                i => i.Date.Date >= dateToStartDisplayingFrom
                && ((account.MainTileSettings.ShowTasks && i.Type == TaskOrEventType.Task) || (account.MainTileSettings.ShowEvents && i.Type == TaskOrEventType.Event))
                )
                .OrderBy(i => i)
                .Take(maxItems)
                .ToList();

            return new UpcomingItemsForWidgetResponse
            {
                Items = items
            };
        }

        public class UpcomingItemsForWidgetResponse
        {
            public bool NoSemester { get; set; }
            public List<ViewItemTaskOrEvent> Items { get; set; }
            public bool IsDisabledInSettings { get; set; }
        }
    }
}
