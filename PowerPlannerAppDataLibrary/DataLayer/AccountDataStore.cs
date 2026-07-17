using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using Dapper;
using Microsoft.Data.Sqlite;
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
using System.Reflection;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.Serialization;

[module: DapperAot]

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
    public partial class AccountDataStore
    {
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
                        writer.Write(PowerPlannerJson.Serialize(_items));
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

                    Dictionary<Guid, ChangedPropertiesOfDataItem> answer;

                    try
                    {
                        timeTracker = TimeTracker.Start();
                        using (StreamReader reader = new StreamReader(s))
                        {
                            answer = PowerPlannerJson.Deserialize<Dictionary<Guid, ChangedPropertiesOfDataItem>>(reader.ReadToEnd());
                        }
                        timeTracker.End(3, "ChangedItems.Load read and deserialize");
                    }

                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                        answer = new Dictionary<Guid, ChangedPropertiesOfDataItem>();
                    }

                    if (answer == null)
                    {
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
        public SqliteConnection _db;

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
            _db = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = DatabaseFilePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            }.ToString());
            await _db.OpenAsync();
            CreateSchema();
            timeTracker.End(3, "AccountDataStore.InitializeDatabase open connection and create schema");

            // Handle upgrading data
            timeTracker = TimeTracker.Start();
            var dataInfo = _db.QueryFirstOrDefault<DataInfo>("SELECT Key, Version FROM DataInfo LIMIT 1");
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
                _db.Execute("update DataItemClass set StartDate = @StartDate, EndDate = @EndDate", new { StartDate = SqlDate.MinValue.Ticks, EndDate = SqlDate.MinValue.Ticks });
            }
            if (version < 5)
            {
                // Added PassingGrade to classes, which needs to be set to 60% by default
                _db.Execute("update DataItemClass set PassingGrade = @PassingGrade", new { PassingGrade = Class.DefaultPassingGrade });
            }
            if (version < 6 && version == 5)
            {
                // In previous version 5, I accidently was saving a class with the identifier of a semester, wiping out semesters
                try
                {
                    int countDeleted = _db.Execute("delete from DataItemClass where exists (select * from DataItemSemester where DataItemClass.Identifier = DataItemSemester.Identifier and DataItemClass.UpperIdentifier = @EmptyIdentifier)", new { EmptyIdentifier = Guid.Empty.ToString() });

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
                            string name = _db.QueryFirstOrDefault<string>("SELECT Name FROM DataItemSemester WHERE Identifier = @Identifier", new { Identifier = semesterToReUpload.ToString() });
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
            if (version < 8)
            {
                // This fixes a VERY OLD bug that I never fixed, introduced around version 5.4.62.0...
                // GpaType was added to DataItemClass a LONG time ago. However, unlike StartDate/EndDate (v4)
                // and PassingGrade (v5), it was never backfilled, so class rows created before the column
                // existed still hold NULL in this non-nullable column. This throws:
                //   "The data is NULL at ordinal N. This method can't be called on NULL values."
                // Only touch NULL rows so we don't clobber valid PassFail values on newer databases.
                _db.Execute("update DataItemClass set GpaType = @GpaType where GpaType is NULL", new { GpaType = (int)GpaType.Standard });
            }
            if (version < DataInfo.LATEST_VERSION)
            {
                _db.Execute("INSERT OR REPLACE INTO DataInfo (Key, Version) VALUES (@Key, @Version)", new { dataInfo.Key, Version = DataInfo.LATEST_VERSION });
            }
            timeTracker.End(3, "AccountDataStore.InitializeDatabase handle upgrade");

            if (needsAppointmentReset)
            {
                AppointmentsExtension.Current?.ResetAll(Account, this);
            }
        }

        /// <summary>
        /// Handles the v1/v2 to v3 upgrade: migrating legacy Homework/Exam tables to MegaItem.
        /// Reads the legacy tables directly and writes their rows into DataItemMegaItem.
        /// </summary>
        private async System.Threading.Tasks.Task UpgradeFromVersion2Async(int version)
        {
            // Check if the old DataItemHomework table exists
            bool homeworkTableExists = false;
            try
            {
                using (var cmd = _db.CreateCommand())
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

            var classes = LoadClasses();
            var migratedItems = new List<DataItemMegaItem>();

            // Read all homework items using raw SQL and insert as MegaItems
            using (var cmd = _db.CreateCommand())
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
                        var upperIdentifier = ReadGuid(reader.GetString(3));
                        var c = classes.FirstOrDefault(i => upperIdentifier == i.Identifier);
                        if (c == null)
                            continue;

                        var megaItem = new DataItemMegaItem()
                        {
                            MegaItemType = MegaItemType.Homework,
                            Identifier = ReadGuid(reader.GetString(0)),
                            DateCreated = ReadDateTime(reader.GetInt64(1)),
                            Updated = ReadDateTime(reader.GetInt64(2)),
                            UpperIdentifier = c.Identifier,
                            Name = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Details = reader.IsDBNull(5) ? null : reader.GetString(5),
                            RawImageNames = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Date = ReadDateTime(reader.GetInt64(7)),
                            GradeReceived = reader.GetDouble(8),
                            GradeTotal = reader.GetDouble(9),
                            IsDropped = reader.GetInt64(10) != 0,
                            IndividualWeight = reader.GetDouble(11),
                            EndTime = ReadDateTime(reader.GetInt64(12)),
                            Reminder = ReadDateTime(reader.GetInt64(13)),
                            WeightCategoryIdentifier = ReadGuid(reader.GetString(14)),
                            AppointmentLocalId = reader.IsDBNull(15) ? null : reader.GetString(15),
                            PercentComplete = reader.GetDouble(16),
#if ANDROID
                            HasSentReminder = reader.GetInt64(17) != 0,
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

                        migratedItems.Add(megaItem);
                    }
                }
            }

            // Read all exam items using raw SQL and insert as MegaItems
            using (var cmd = _db.CreateCommand())
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
                        var upperIdentifier = ReadGuid(reader.GetString(3));
                        var c = classes.FirstOrDefault(i => upperIdentifier == i.Identifier);
                        if (c == null)
                            continue;

                        var megaItem = new DataItemMegaItem()
                        {
                            MegaItemType = MegaItemType.Exam,
                            Identifier = ReadGuid(reader.GetString(0)),
                            DateCreated = ReadDateTime(reader.GetInt64(1)),
                            Updated = ReadDateTime(reader.GetInt64(2)),
                            UpperIdentifier = c.Identifier,
                            Name = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Details = reader.IsDBNull(5) ? null : reader.GetString(5),
                            RawImageNames = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Date = ReadDateTime(reader.GetInt64(7)),
                            GradeReceived = reader.GetDouble(8),
                            GradeTotal = reader.GetDouble(9),
                            IsDropped = reader.GetInt64(10) != 0,
                            IndividualWeight = reader.GetDouble(11),
                            EndTime = ReadDateTime(reader.GetInt64(12)),
                            Reminder = ReadDateTime(reader.GetInt64(13)),
                            WeightCategoryIdentifier = ReadGuid(reader.GetString(14)),
                            AppointmentLocalId = reader.IsDBNull(15) ? null : reader.GetString(15),
#if ANDROID
                            HasSentReminder = reader.GetInt64(16) != 0,
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

                        migratedItems.Add(megaItem);
                    }
                }
            }

            using (var transaction = _db.BeginTransaction())
            {
                foreach (var item in migratedItems)
                    UpsertItem(item, transaction);
                transaction.Commit();
            }

            // Drop the legacy tables
            _db.Execute("DROP TABLE IF EXISTS DataItemHomework; DROP TABLE IF EXISTS DataItemExam;");
        }

        public class DataInfo
        {
            public const int LATEST_VERSION = 8;

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
                    _db.Execute("INSERT OR IGNORE INTO ImageToUpload (FileName) VALUES (@FileName)", new { FileName = u });
                }
            }
        }

        /// <summary>
        /// Uses DataLock
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetNextImageToUploadAsync()
        {
            using (await Locks.LockDataForReadAsync())
            {
                return GetNextImageToUploadBlocking();
            }
        }

        /// <summary>
        /// Caller must establish data lock
        /// </summary>
        /// <returns></returns>
        private string GetNextImageToUploadBlocking()
        {
            return _db.QueryFirstOrDefault<string>("SELECT FileName FROM ImageToUpload LIMIT 1");
        }

        public async System.Threading.Tasks.Task MarkImageUploadedAsync(string fileName)
        {
            using (await Locks.LockDataForWriteAsync())
            {
                MarkImageUploadedBlocking(fileName);
            }
        }

        /// <summary>
        /// Caller must establish data lock
        /// </summary>
        /// <param name="fileName"></param>
        private void MarkImageUploadedBlocking(string fileName)
        {
            _db.Execute("DELETE FROM ImageToUpload WHERE FileName = @FileName", new { FileName = fileName });
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
            using (var transaction = _db.BeginTransaction())
            {
                UpsertItems(items, transaction);
                transaction.Commit();
            }
        }

        private void UpsertItems(IEnumerable<BaseDataItem> items, SqliteTransaction transaction)
        {
            foreach (var item in items)
                UpsertItem(item, transaction);
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

            using (var transaction = _db.BeginTransaction())
            {
                try
                {
                    timeTracker = TimeTracker.Start();
                    UpsertItems(existingDataItems, transaction);
                    timeTracker.End(3, $"CommitChanges update existing items, {existingDataItems.Count} existingDataItems");

                    // Add the new items
                    timeTracker = TimeTracker.Start();
                    UpsertItems(newDataItems, transaction);
                    timeTracker.End(3, $"CommitChanges insert new items, {newDataItems.Count} newDataItems");

                    // And delete the deleted items
                    timeTracker = TimeTracker.Start();
                    deletedItems = RecursiveDelete(dataChanges.IdentifiersToDelete.ToArray(), transaction);
                    timeTracker.End(3, $"CommitChanges RecursiveDelete, {dataChanges.IdentifiersToDelete.Count()} identifiers to delete");

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

        private DeletedItems RecursiveDelete(Guid[] identifiersToDelete, SqliteTransaction transaction)
        {
            DeletedItems into = new DeletedItems();

            RecursiveDeleteHelper(identifiersToDelete, into, transaction);

            return into;
        }

        private void RecursiveDeleteHelper(Guid[] identifiersToDelete, DeletedItems into, SqliteTransaction transaction)
        {
            if (identifiersToDelete.Length > 500)
            {
                foreach (Guid[] batched in identifiersToDelete.BatchAsArrays(500))
                    RecursiveDeleteHelper(batched, into, transaction);

                return;
            }

            if (identifiersToDelete.Length == 0)
                return;

            Delete(identifiersToDelete, into, transaction);
            into.DeletedIdentifiers.AddRange(identifiersToDelete);

            Guid[] childrenToDelete = FindAllIdentifiersThatAreChildren(identifiersToDelete, transaction);
            RecursiveDeleteHelper(childrenToDelete, into, transaction);
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

            foreach (var i in FindMegaItems(identifiersToLookFor))
            { yield return found(i); }
            if (done()) yield break;

            foreach (var i in FindGrades(identifiersToLookFor))
            { yield return found(i); }
            if (done()) yield break;

            foreach (var i in FindClasses(identifiersToLookFor))
            { yield return found(i); }
            if (done()) yield break;

            foreach (var i in FindSchedules(identifiersToLookFor))
            { yield return found(i); }
            if (done()) yield break;

            foreach (var i in FindSemesters(identifiersToLookFor))
            { yield return found(i); }
            if (done()) yield break;

            foreach (var i in FindYears(identifiersToLookFor))
            { yield return found(i); }
            if (done()) yield break;

            foreach (var i in FindWeightCategories(identifiersToLookFor))
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

            foreach (var i in FindYears(identifiersToLookFor))
            { yield return found(i); if (done()) yield break; }

            foreach (var i in FindSemesters(identifiersToLookFor))
            { yield return found(i); if (done()) yield break; }

            foreach (var i in FindClasses(identifiersToLookFor))
            { yield return found(i); if (done()) yield break; }

            foreach (var i in FindWeightCategories(identifiersToLookFor))
            { yield return found(i); if (done()) yield break; }

            foreach (var i in FindMegaItems(identifiersToLookFor))
            { yield return found(i); if (done()) yield break; }

            foreach (var i in FindGrades(identifiersToLookFor))
            { yield return found(i); if (done()) yield break; }

            foreach (var i in FindSchedules(identifiersToLookFor))
            { yield return found(i); if (done()) yield break; }
        }

        private DataItemMegaItem[] FindMegaItems(Guid[] identifiers)
        {
            List<Guid> identifierList = identifiers.ToList();
            return LoadMegaItems().Where(i => identifierList.Contains(i.Identifier)).ToArray();
        }

        private DataItemGrade[] FindGrades(Guid[] identifiers)
        {
            List<Guid> identifierList = identifiers.ToList();
            return LoadGrades().Where(i => identifierList.Contains(i.Identifier)).ToArray();
        }

        private DataItemClass[] FindClasses(Guid[] identifiers)
        {
            List<Guid> identifierList = identifiers.ToList();
            return LoadClasses().Where(i => identifierList.Contains(i.Identifier)).ToArray();
        }

        private DataItemSchedule[] FindSchedules(Guid[] identifiers)
        {
            List<Guid> identifierList = identifiers.ToList();
            return LoadSchedules().Where(i => identifierList.Contains(i.Identifier)).ToArray();
        }

        private DataItemSemester[] FindSemesters(Guid[] identifiers)
        {
            List<Guid> identifierList = identifiers.ToList();
            return LoadSemesters().Where(i => identifierList.Contains(i.Identifier)).ToArray();
        }

        private DataItemYear[] FindYears(Guid[] identifiers)
        {
            List<Guid> identifierList = identifiers.ToList();
            return LoadYears().Where(i => identifierList.Contains(i.Identifier)).ToArray();
        }

        private DataItemWeightCategory[] FindWeightCategories(Guid[] identifiers)
        {
            List<Guid> identifierList = identifiers.ToList();
            return LoadWeightCategories().Where(i => identifierList.Contains(i.Identifier)).ToArray();
        }

        private Guid[] FindAllIdentifiersThatAreChildren(Guid[] parentIdentifiers, SqliteTransaction transaction)
        {
            List<Guid> children = new List<Guid>();

            List<Guid> identifiers = parentIdentifiers.ToList();
            children.AddRange(LoadMegaItems(transaction).Where(i => identifiers.Contains(i.UpperIdentifier)).Select(i => i.Identifier));
            children.AddRange(LoadGrades(transaction).Where(i => identifiers.Contains(i.UpperIdentifier)).Select(i => i.Identifier));
            children.AddRange(LoadSchedules(transaction).Where(i => identifiers.Contains(i.UpperIdentifier)).Select(i => i.Identifier));
            children.AddRange(LoadClasses(transaction).Where(i => identifiers.Contains(i.UpperIdentifier)).Select(i => i.Identifier));
            children.AddRange(LoadSemesters(transaction).Where(i => identifiers.Contains(i.UpperIdentifier)).Select(i => i.Identifier));
            children.AddRange(LoadWeightCategories(transaction).Where(i => identifiers.Contains(i.UpperIdentifier)).Select(i => i.Identifier));

            return children.ToArray();
        }

        private int DeleteMegaItems(Guid[] identifiersToDelete, DeletedItems into, SqliteTransaction transaction)
        {
            List<Guid> identifiers = identifiersToDelete.ToList();
            var toDelete = LoadMegaItems(transaction).Where(i => identifiers.Contains(i.Identifier)).ToArray();
            into.DeletedTaskEventAppointments.AddRange(toDelete.Where(i => i.MegaItemType != MegaItemType.Holiday).Select(i => i.AppointmentLocalId));
            into.DidDeleteHoliday = into.DidDeleteHoliday || toDelete.Any(i => i.MegaItemType == MegaItemType.Holiday);
            if (toDelete.Length == 0)
                return 0;
            _db.Execute("DELETE FROM DataItemMegaItem WHERE instr(@Identifiers, '|' || Identifier || '|') > 0", new { Identifiers = CreateIdentifierSet(toDelete.Select(i => i.Identifier)) }, transaction);
            return toDelete.Length;
        }

        private int DeleteSchedules(Guid[] identifiersToDelete, DeletedItems into, SqliteTransaction transaction)
        {
            List<Guid> identifiers = identifiersToDelete.ToList();
            var toDelete = LoadSchedules(transaction).Where(i => identifiers.Contains(i.Identifier)).ToArray();
            into.DeletedScheduleAppointments.AddRange(toDelete.Select(i => i.AppointmentLocalId));
            if (toDelete.Length == 0)
                return 0;
            _db.Execute("DELETE FROM DataItemSchedule WHERE instr(@Identifiers, '|' || Identifier || '|') > 0", new { Identifiers = CreateIdentifierSet(toDelete.Select(i => i.Identifier)) }, transaction);
            return toDelete.Length;
        }

        private void Delete(Guid[] identifiersToDelete, DeletedItems into, SqliteTransaction transaction)
        {
            List<Guid> identifiers = identifiersToDelete.ToList();
            int countDeleted = 0;

            countDeleted += DeleteMegaItems(identifiersToDelete, into, transaction);
            if (countDeleted >= identifiersToDelete.Length) return;

            var grades = LoadGrades(transaction).Where(i => identifiers.Contains(i.Identifier)).ToArray();
            countDeleted += _db.Execute("DELETE FROM DataItemGrade WHERE instr(@Identifiers, '|' || Identifier || '|') > 0", new { Identifiers = CreateIdentifierSet(grades.Select(i => i.Identifier)) }, transaction);
            if (countDeleted >= identifiersToDelete.Length) return;

            countDeleted += DeleteSchedules(identifiersToDelete, into, transaction);
            if (countDeleted >= identifiersToDelete.Length) return;

            var classes = LoadClasses(transaction).Where(i => identifiers.Contains(i.Identifier)).ToArray();
            countDeleted += _db.Execute("DELETE FROM DataItemClass WHERE instr(@Identifiers, '|' || Identifier || '|') > 0", new { Identifiers = CreateIdentifierSet(classes.Select(i => i.Identifier)) }, transaction);
            if (countDeleted >= identifiersToDelete.Length) return;

            var semesters = LoadSemesters(transaction).Where(i => identifiers.Contains(i.Identifier)).ToArray();
            countDeleted += _db.Execute("DELETE FROM DataItemSemester WHERE instr(@Identifiers, '|' || Identifier || '|') > 0", new { Identifiers = CreateIdentifierSet(semesters.Select(i => i.Identifier)) }, transaction);
            if (countDeleted >= identifiersToDelete.Length) return;

            var weightCategories = LoadWeightCategories(transaction).Where(i => identifiers.Contains(i.Identifier)).ToArray();
            countDeleted += _db.Execute("DELETE FROM DataItemWeightCategory WHERE instr(@Identifiers, '|' || Identifier || '|') > 0", new { Identifiers = CreateIdentifierSet(weightCategories.Select(i => i.Identifier)) }, transaction);
            if (countDeleted >= identifiersToDelete.Length) return;

            var years = LoadYears(transaction).Where(i => identifiers.Contains(i.Identifier)).ToArray();
            _db.Execute("DELETE FROM DataItemYear WHERE instr(@Identifiers, '|' || Identifier || '|') > 0", new { Identifiers = CreateIdentifierSet(years.Select(i => i.Identifier)) }, transaction);
        }

        private List<BaseDataItem> GetExistingItems(IEnumerable<BaseDataItem> itemsToMatch)
        {
            List<BaseDataItem> existingItems = new List<BaseDataItem>();

            List<Guid> classIdentifiers = itemsToMatch.OfType<DataItemClass>().Select(i => i.Identifier).ToList();
            existingItems.AddRange(LoadClasses().Where(i => classIdentifiers.Contains(i.Identifier)));

            List<Guid> gradeIdentifiers = itemsToMatch.OfType<DataItemGrade>().Select(i => i.Identifier).ToList();
            existingItems.AddRange(LoadGrades().Where(i => gradeIdentifiers.Contains(i.Identifier)));

            List<Guid> megaItemIdentifiers = itemsToMatch.OfType<DataItemMegaItem>().Select(i => i.Identifier).ToList();
            existingItems.AddRange(LoadMegaItems().Where(i => megaItemIdentifiers.Contains(i.Identifier)));

            List<Guid> scheduleIdentifiers = itemsToMatch.OfType<DataItemSchedule>().Select(i => i.Identifier).ToList();
            existingItems.AddRange(LoadSchedules().Where(i => scheduleIdentifiers.Contains(i.Identifier)));

            List<Guid> semesterIdentifiers = itemsToMatch.OfType<DataItemSemester>().Select(i => i.Identifier).ToList();
            existingItems.AddRange(LoadSemesters().Where(i => semesterIdentifiers.Contains(i.Identifier)));

            List<Guid> weightCategoryIdentifiers = itemsToMatch.OfType<DataItemWeightCategory>().Select(i => i.Identifier).ToList();
            existingItems.AddRange(LoadWeightCategories().Where(i => weightCategoryIdentifiers.Contains(i.Identifier)));

            List<Guid> yearIdentifiers = itemsToMatch.OfType<DataItemYear>().Select(i => i.Identifier).ToList();
            existingItems.AddRange(LoadYears().Where(i => yearIdentifiers.Contains(i.Identifier)));

            return existingItems;
        }

        public DataItemYear[] GetYears()
        {
            return LoadYears();
        }

        public int GetYearCount()
        {
            return LoadYears().Length;
        }

        public DataItemSemester[] GetSemesters()
        {
            return LoadSemesters();
        }

        public int GetSemesterCount()
        {
            return LoadSemesters().Length;
        }

        public DataItemClass[] GetClasses()
        {
            return LoadClasses();
        }

        public int GetClassCount()
        {
            return LoadClasses().Length;
        }

        public DataItemSchedule[] GetSchedules()
        {
            return LoadSchedules();
        }

        public DataItemWeightCategory[] GetWeightCategories()
        {
            return LoadWeightCategories();
        }

        public DataItemGrade[] GetGrades()
        {
            return LoadGrades();
        }

        public DataItemSemester GetSemester(Guid identifier)
        {
            Guid semesterIdentifier = identifier;
            return LoadSemesters().FirstOrDefault(i => i.Identifier == semesterIdentifier);
        }

        public bool SemesterExists(Guid identifier)
        {
            Guid semesterIdentifier = identifier;
            return LoadSemesters().Any(i => i.Identifier == semesterIdentifier);
        }

        public DataItemClass GetClass(Guid identifier)
        {
            Guid classIdentifier = identifier;
            return LoadClasses().FirstOrDefault(i => i.Identifier == classIdentifier);
        }

        public DataItemClass[] GetClassesUnderSemester(Guid identifier)
        {
            Guid semesterIdentifier = identifier;
            return LoadClasses().Where(i => i.UpperIdentifier == semesterIdentifier).ToArray();
        }

        public DataItemSchedule[] GetSchedulesUnderClasses(Guid[] identifiers)
        {
            List<Guid> classIdentifiers = identifiers.ToList();
            return LoadSchedules().Where(i => classIdentifiers.Contains(i.UpperIdentifier)).ToArray();
        }

        public DataItemWeightCategory[] GetWeightCategoriesUnderClasses(Guid[] identifiers)
        {
            List<Guid> classIdentifiers = identifiers.ToList();
            return LoadWeightCategories().Where(i => classIdentifiers.Contains(i.UpperIdentifier)).ToArray();
        }

        public DataItemGrade[] GetGradesUnderWeightCategories(Guid[] identifiers)
        {
            List<Guid> weightCategoryIdentifiers = identifiers.ToList();
            return LoadGrades().Where(i => weightCategoryIdentifiers.Contains(i.UpperIdentifier)).ToArray();
        }

        public DataItemMegaItem GetMegaItem(Guid identifier)
        {
            Guid megaItemIdentifier = identifier;
            return LoadMegaItems().FirstOrDefault(i => i.Identifier == megaItemIdentifier);
        }

        public DataItemMegaItem GetHoliday(Guid identifier)
        {
            Guid holidayIdentifier = identifier;
            return LoadMegaItems().FirstOrDefault(i =>
                i.MegaItemType == MegaItemType.Holiday
                && i.Identifier == holidayIdentifier);
        }

        public DataItemMegaItem[] GetAgendaItems(Guid[] identifiers, DateTime date)
        {
            List<Guid> classIdentifiers = identifiers.ToList();
            DateTime todayAsUtc = date;
            return LoadMegaItems().Where(i =>
                (((i.MegaItemType == MegaItemType.Homework && i.PercentComplete < 1.0)
                    || (i.MegaItemType == MegaItemType.Exam && i.Date >= todayAsUtc))
                    && classIdentifiers.Contains(i.UpperIdentifier))
                || (i.MegaItemType == MegaItemType.Task && i.PercentComplete < 1.0)
                || (i.MegaItemType == MegaItemType.Event && i.Date >= todayAsUtc)).ToArray();
        }

        public DataItemMegaItem[] GetCalendarItems(Guid[] identifiers, DateTime startDate, DateTime endDate)
        {
            List<Guid> classIdentifiers = identifiers.ToList();
            DateTime start = startDate;
            DateTime end = endDate;
            return LoadMegaItems().Where(i =>
                (i.MegaItemType == MegaItemType.Homework
                    || i.MegaItemType == MegaItemType.Exam
                    || i.MegaItemType == MegaItemType.Task
                    || i.MegaItemType == MegaItemType.Event)
                && classIdentifiers.Contains(i.UpperIdentifier)
                && i.Date >= start
                && i.Date <= end).ToArray();
        }

        public DataItemMegaItem[] GetSemesterItems(Guid identifier, Guid[] identifiers)
        {
            Guid semesterIdentifier = identifier;
            List<Guid> classIdentifiers = identifiers.ToList();
            return LoadMegaItems().Where(i =>
                ((i.MegaItemType == MegaItemType.Holiday
                    || i.MegaItemType == MegaItemType.Task
                    || i.MegaItemType == MegaItemType.Event)
                    && i.UpperIdentifier == semesterIdentifier)
                || ((i.MegaItemType == MegaItemType.Homework || i.MegaItemType == MegaItemType.Exam)
                    && classIdentifiers.Contains(i.UpperIdentifier))).ToArray();
        }

        public DataItemMegaItem[] GetCurrentClassItems(Guid identifier, DateTime date)
        {
            Guid classIdentifier = identifier;
            DateTime todayAsUtc = date;
            return LoadMegaItems().Where(i =>
                (i.MegaItemType == MegaItemType.Homework
                    && i.UpperIdentifier == classIdentifier
                    && (i.PercentComplete < 1.0 || i.Date >= todayAsUtc))
                || (i.MegaItemType == MegaItemType.Exam
                    && i.UpperIdentifier == classIdentifier
                    && i.Date >= todayAsUtc)).ToArray();
        }

        public bool HasPastCompletedTasks(Guid identifier, DateTime date)
        {
            Guid classIdentifier = identifier;
            DateTime todayAsUtc = date;
            return LoadMegaItems().Any(i =>
                i.MegaItemType == MegaItemType.Homework
                && i.UpperIdentifier == classIdentifier
                && i.PercentComplete >= 1.0
                && i.Date < todayAsUtc);
        }

        public bool HasPastCompletedEvents(Guid identifier, DateTime date)
        {
            Guid classIdentifier = identifier;
            DateTime nextDay = date.Date.AddDays(1);
            return LoadMegaItems().Any(i =>
                i.MegaItemType == MegaItemType.Exam
                && i.UpperIdentifier == classIdentifier
                && i.Date < nextDay);
        }

        public DataItemMegaItem[] GetPastCompletedTasksAndEvents(Guid identifier, DateTime date)
        {
            Guid classIdentifier = identifier;
            DateTime todayAsUtc = date;
            DateTime nextDay = date.Date.AddDays(1);
            return LoadMegaItems().Where(i =>
                (i.MegaItemType == MegaItemType.Homework
                    && i.UpperIdentifier == classIdentifier
                    && i.PercentComplete >= 1.0
                    && i.Date < todayAsUtc)
                || (i.MegaItemType == MegaItemType.Exam
                    && i.UpperIdentifier == classIdentifier
                    && i.Date < nextDay)).ToArray();
        }

        public DataItemMegaItem[] GetHolidays(Guid identifier, DateTime startDate, DateTime endDate)
        {
            Guid semesterIdentifier = identifier;
            DateTime startAsUtc = startDate;
            DateTime endAsUtc = endDate;
            return LoadMegaItems().Where(i =>
                i.MegaItemType == MegaItemType.Holiday
                && i.UpperIdentifier == semesterIdentifier
                && ((i.Date <= startAsUtc && i.EndTime >= startAsUtc)
                    || (i.Date >= startAsUtc && i.Date <= endAsUtc))).ToArray();
        }

        public DataItemMegaItem[] GetGradedMegaItems()
        {
            return LoadMegaItems().Where(i =>
                (i.MegaItemType == MegaItemType.Homework || i.MegaItemType == MegaItemType.Exam)
                && i.WeightCategoryIdentifier != BaseHomeworkExam.WEIGHT_CATEGORY_EXCLUDED).ToArray();
        }

        public DataItemMegaItem[] GetGradedMegaItemsUnderClass(Guid identifier)
        {
            Guid classIdentifier = identifier;
            return LoadMegaItems().Where(i =>
                (i.MegaItemType == MegaItemType.Exam || i.MegaItemType == MegaItemType.Homework)
                && i.UpperIdentifier == classIdentifier
                && i.WeightCategoryIdentifier != BaseHomeworkExam.WEIGHT_CATEGORY_EXCLUDED).ToArray();
        }

        public bool HasManyOldMegaItems()
        {
            var megaItems = LoadMegaItems();
            if (megaItems.Length <= 30)
                return false;

            DateTime cutoff = DateTime.Today.AddDays(-60);
            return megaItems.Any(i => i.DateCreated < cutoff);
        }

#region GetClasses

        public Guid[] GetClassIdentifiersUnderSemester(Guid semesterIdentifier)
        {
            Guid identifier = semesterIdentifier;
            return LoadClasses()
                .Where(c => c.UpperIdentifier == identifier)
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
                Guid classIdentifier = classId;
                return LoadClasses().Where(i => i.Identifier == classIdentifier).Select(i => i.UpperIdentifier).FirstOrDefault();
            }
        }

#if ANDROID
        public async System.Threading.Tasks.Task MarkAndroidRemindersSent(Guid[] homeworkIds, Guid[] examIds)
        {
            await System.Threading.Tasks.Task.Run(async delegate
            {
                using (await Locks.LockDataForWriteAsync())
                {
                    var allIds = CreateIdentifierSet(homeworkIds.Concat(examIds));
                    _db.Execute("update DataItemMegaItem set HasSentReminder = 1 where instr(@Identifiers, '|' || Identifier || '|') > 0", new { Identifiers = allIds });
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
