using StorageEverywhere;
using PowerPlannerAppDataLibrary.DataLayer.TileSettings;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Extensions.Telemetry;
using PowerPlannerAppDataLibrary.SyncLayer;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerUWPLibrary.DataLayer")]
    public class AccountDataItem : BindableBaseWithPortableDispatcher
    {
        public const int CURRENT_ACCOUNT_DATA_VERSION = 5;
        public const int CURRENT_SYNCED_DATA_VERSION = 5;

        /// <summary>
        /// Initializes a new account
        /// </summary>
        /// <param name="localAccountId"></param>
        public AccountDataItem(Guid localAccountId)
        {
            LocalAccountId = localAccountId;
        }

        public Guid LocalAccountId { get; internal set; }

        [DataMember]
        public Version Version = Variables.VERSION;

        [DataMember]
        public int AccountDataVersion = CURRENT_ACCOUNT_DATA_VERSION;

        /// <summary>
        /// SyncedDataVersion is used so that when we make drastic changes, we can perform needed actions upon upgrade.
        /// For example, when we added grades linked to tasks/events, we need down-level clients to re-sync
        /// all tasks/events, so that they pick up the new grade values.
        /// </summary>
        [DataMember]
        public int SyncedDataVersion { get; set; } = CURRENT_SYNCED_DATA_VERSION;

        private int _currentChangeNumber;
        [DataMember]
        public int CurrentChangeNumber
        {
            get { return _currentChangeNumber; }
            set { SetProperty(ref _currentChangeNumber, value, "CurrentChangeNumber"); }
        }

        /// <summary>
        /// The last current online DefaultGradeScaleIndex that this client saw. Initially starts as 0 (null), same as a newly created account.
        /// </summary>
        [DataMember]
        public long CurrentDefaultGradeScaleIndex { get; set; }

        [DataMember]
        public bool NeedsInitialSync { get; set; }

        #region Settings

        private bool _needsToSyncSettings;
        /// <summary>
        /// In case user exits or is offline when they modify settings, I'll remember that I need to sync them
        /// </summary>
        [DataMember]
        public bool NeedsToSyncSettings
        {
            get { return _needsToSyncSettings; }
            set { SetProperty(ref _needsToSyncSettings, value, "NeedsToSyncSettings"); }
        }

        private bool _isAppointmentsUpToDate;
        /// <summary>
        /// Gets and sets a value indicating whether the Appointments calendar has been updated. If false, an entire re-write of the Appointments data will be performed. If true, incremental updates of Appointments data can be made.
        /// </summary>
        [DataMember]
        public bool IsAppointmentsUpToDate
        {
            get { return _isAppointmentsUpToDate; }
            set { SetProperty(ref _isAppointmentsUpToDate, value, "IsAppointmentsUpToDate"); }
        }

        private bool _isTasksCalendarIntegrationDisabled;
        /// <summary>
        /// Gets and sets a value indicating whether the Tasks calendar integration with system calendar has been disabled.
        /// </summary>
        [DataMember]
        public bool IsTasksCalendarIntegrationDisabled
        {
            get { return _isTasksCalendarIntegrationDisabled; }
            set { SetProperty(ref _isTasksCalendarIntegrationDisabled, value, "IsTasksCalendarIntegrationDisabled"); }
        }

        private bool _isClassesCalendarIntegrationDisabled;
        /// <summary>
        /// Gets and sets a value indicating whether the Classes calendar integration with system calendar has been disabled.
        /// </summary>
        [DataMember]
        public bool IsClassesCalendarIntegrationDisabled
        {
            get { return _isClassesCalendarIntegrationDisabled; }
            set { SetProperty(ref _isClassesCalendarIntegrationDisabled, value, "IsClassesCalendarIntegrationDisabled"); }
        }

        /// <summary>
        /// Gets a value indicating whether all calendar integration has been disabled.
        /// </summary>
        public bool IsAllCalendarIntegrationDisabled()
        {
            return IsTasksCalendarIntegrationDisabled && IsClassesCalendarIntegrationDisabled;
        }

        [DataMember]
        private string _serializedSchoolTimeZone { get; set; }

        public event EventHandler OnSchoolTimeZoneChanged;
        private bool _initializedSchoolTimeZone;
        private TimeZoneInfo _schoolTimeZone;
        public TimeZoneInfo SchoolTimeZone
        {
            get
            {
                if (!_initializedSchoolTimeZone)
                {
                    if (_serializedSchoolTimeZone != null)
                    {
                        try
                        {
                            _schoolTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_serializedSchoolTimeZone);
                        }
                        catch { }
                    }
                    _initializedSchoolTimeZone = true;
                }

                return _schoolTimeZone;
            }
            set
            {
                if (object.Equals(SchoolTimeZone, value))
                {
                    return;
                }

                if (value == null)
                {
                    _serializedSchoolTimeZone = null;
                }
                else
                {
                    _serializedSchoolTimeZone = value.Id;
                }

                _schoolTimeZone = value;
                UpdateIsInDifferentTimeZone();
                OnPropertyChanged(nameof(SchoolTimeZone));
                OnSchoolTimeZoneChanged?.Invoke(this, null);
            }
        }

        private bool? _isInDifferentTimeZone;
        /// <summary>
        /// Returns true if local time is actually different than school time (hours or minutes are different).
        /// </summary>
        public bool IsInDifferentTimeZone
        {
            get
            {
                if (_isInDifferentTimeZone == null)
                {
                    UpdateIsInDifferentTimeZone();
                }

                return _isInDifferentTimeZone.Value;
            }
        }

        private void UpdateIsInDifferentTimeZone()
        {
            if (SchoolTimeZone == null)
            {
                _isInDifferentTimeZone = false;
                return;
            }

            DateTime now = DateTime.Now;

            if (SchoolTimeZone.GetUtcOffset(now) != TimeZoneInfo.Local.GetUtcOffset(now))
            {
                _isInDifferentTimeZone = true;
            }
            else
            {
                _isInDifferentTimeZone = false;
            }
        }

        private GpaOptions _gpaOption;
        [DataMember]
        public GpaOptions GpaOption
        {
            get { return _gpaOption; }
            set { SetProperty(ref _gpaOption, value, "GpaOption"); }
        }

        private DateTime _weekOneStartsOn = DateTools.Last(DayOfWeek.Sunday, DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc));
        [DataMember]
        public DateTime WeekOneStartsOn
        {
            get { return _weekOneStartsOn; }
            set { SetProperties(ref _weekOneStartsOn, DateTime.SpecifyKind(value.Date, DateTimeKind.Utc), "WeekOneStartsOn", "CurrentWeek", "WeekChangesOn"); }
        }

        public DayOfWeek WeekChangesOn
        {
            get { return WeekOneStartsOn.DayOfWeek; }
        }

        /// <summary>
        /// Automatically sets WeekOneStartsOn
        /// </summary>
        public Schedule.Week CurrentWeek
        {
            get
            {
                if (MyMath.Mod((int)(DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc) - WeekOneStartsOn.Date).TotalDays, 14) < 7)
                    return Schedule.Week.WeekOne;

                return Schedule.Week.WeekTwo;
            }
        }

        /// <summary>
        /// Automatically updates display, resets reminders, starts settings sync, and submits changes
        /// </summary>
        /// <param name="startsOn"></param>
        /// <param name="currentWeek"></param>
        public async System.Threading.Tasks.Task SetWeek(DayOfWeek startsOn, Schedule.Week currentWeek)
        {
            if (SetWeekSimple(startsOn, currentWeek))
            {
                // Clear cached schedules on day since they don't subscribe to these changes.
                // Ideally I would have the lists subscribe to the account, but this will do for now.
                ViewLists.SchedulesOnDay.ClearCached();
                ViewLists.DayScheduleItemsArranger.ClearCached();

                NeedsToSyncSettings = true;

                // Save
                await SaveOnThread();

                //make upcoming update
                OnPropertyChanged("ShowSchedule");

                AccountDataStore data = await AccountDataStore.Get(this.LocalAccountId);

                _ = ClassRemindersExtension.Current?.ResetAllRemindersAsync(this);
                _ = RemindersExtension.Current?.ResetReminders(this, data);

                _ = System.Threading.Tasks.Task.Run(delegate
                {
                    // Update schedule tile
                    _ = ScheduleTileExtension.Current?.UpdateScheduleTile(this, data);
                });

                _ = Sync.SyncSettings(this, Sync.ChangedSetting.WeekOneStartsOn);
            }
        }

        /// <summary>
        /// Saves changes, then triggers updates to reminders/tiles, and triggers a settings sync
        /// </summary>
        /// <param name="schoolTimeZone"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task SetSchoolTimeZone(TimeZoneInfo schoolTimeZone)
        {
            SchoolTimeZone = schoolTimeZone;
            NeedsToSyncSettings = true;

            // Save
            await SaveOnThread();

            AccountDataStore data = await AccountDataStore.Get(this.LocalAccountId);

            _ = ClassRemindersExtension.Current?.ResetAllRemindersAsync(this);
            _ = RemindersExtension.Current?.ResetReminders(this, data);

            _ = System.Threading.Tasks.Task.Run(delegate
            {
                // Update schedule tile
                _ = ScheduleTileExtension.Current?.UpdateScheduleTile(this, data);
            });

            _ = Sync.SyncSettings(this, Sync.ChangedSetting.SchoolTimeZone);
        }

        /// <summary>
        /// Doesn't do any saving or setting of any other dependent properties
        /// </summary>
        /// <param name="startsOn"></param>
        /// <param name="currentWeek"></param>
        public bool SetWeekSimple(DayOfWeek startsOn, Schedule.Week currentWeek)
        {
            DateTime today = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);

            if (currentWeek == Schedule.Week.WeekTwo)
                today = today.AddDays(-7);

            DateTime answer = DateTools.Last(startsOn, today);
            if (answer != WeekOneStartsOn)
            {
                WeekOneStartsOn = answer;
                return true;
            }

            return false;
        }

        private async System.Threading.Tasks.Task SaveOnThread()
        {
            await AccountsManager.Save(this);
        }

        private Schedule.Week toggleWeek()
        {
            if (CurrentWeek == Schedule.Week.WeekOne)
                return Schedule.Week.WeekTwo;
            else
                return Schedule.Week.WeekOne;
        }

        /// <summary>
        /// Incoming parameter can be in either local or UTC time, it'll be specified as UTC for the comparison with the stored UTC value
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public Schedule.Week GetWeekOnDifferentDate(DateTime date)
        {
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            return PowerPlannerSending.Helpers.WeekHelper.GetWeekOnDifferentDate(WeekOneStartsOn, date);
        }

        private bool _showPastCompleteItemsOnCalendar = false;
        [DataMember(Name = "ShowPastCompleteItemsOnFullCalendar")] // For legacy purposes, it used to be called this
        public bool ShowPastCompleteItemsOnCalendar
        {
            get => _showPastCompleteItemsOnCalendar;
            set => SetProperty(ref _showPastCompleteItemsOnCalendar, value, nameof(ShowPastCompleteItemsOnCalendar));
        }

        private bool showSchedule = true;
        [DataMember]
        public bool ShowSchedule
        {
            get { return showSchedule; }
            set { SetProperty(ref showSchedule, value, "ShowSchedule"); }
        }

        private bool showBackground = true;
        [DataMember]
        public bool ShowBackground
        {
            get { return showBackground; }
            set { SetProperty(ref showBackground, value, "ShowBackground"); }
        }

        private bool _reviewed;
        /// <summary>
        /// If the user hasn't reviewed the app, this should be false.
        /// </summary>
        [DataMember]
        public bool Reviewed
        {
            get { return _reviewed; }
            set { SetProperty(ref _reviewed, value, "Reviewed"); }
        }

        private bool _hasAddedRepeating = false;
        [DataMember]
        public bool HasAddedRepeating
        {
            get { return _hasAddedRepeating; }
            set { _hasAddedRepeating = value; }
        }

        public bool AreClassRemindersEnabled()
        {
            return ClassRemindersTimeSpan != null;
        }

        public static TimeSpan DefaultClassRemindersTimeSpan = TimeSpan.FromMinutes(10);

        private TimeSpan? _classRemindersTimeSpan = DefaultClassRemindersTimeSpan;
        /// <summary>
        /// The time span to remind before class. If null, reminders are disabled.
        /// </summary>
        [DataMember]
        public TimeSpan? ClassRemindersTimeSpan
        {
            get => _classRemindersTimeSpan;
            set => SetProperty(ref _classRemindersTimeSpan, value, nameof(ClassRemindersTimeSpan));
        }

        private bool _remindersDayBefore = true;
        [DataMember]
        public bool RemindersDayBefore
        {
            get { return _remindersDayBefore; }
            set { SetProperty(ref _remindersDayBefore, value, "RemindersDayBefore"); }
        }

        private bool _remindersDayOf = true;
        [DataMember]
        public bool RemindersDayOf
        {
            get { return _remindersDayOf; }
            set { SetProperty(ref _remindersDayOf, value, "RemindersDayOf"); }
        }

        private ImageUploadOptions _imageUploadOption = ImageUploadOptions.WifiOnly;
        [DataMember]
        public ImageUploadOptions ImageUploadOption
        {
            get { return _imageUploadOption; }
            set { SetProperty(ref _imageUploadOption, value, "ImageUploadOption"); }
        }

        private bool _isPushDisabled = false;
        [DataMember]
        public bool IsPushDisabled
        {
            get { return _isPushDisabled; }
            set { SetProperty(ref _isPushDisabled, value, "IsPushDisabled"); }
        }

        [DataMember]
        private GradeScale[] _defaultGradeScale;
        public GradeScale[] DefaultGradeScale
        {
            get
            {
                if (_defaultGradeScale == null || _defaultGradeScale.Length == 0)
                {
                    _defaultGradeScale = GradeScale.GenerateDefaultScaleWithoutLetters();
                }

                return _defaultGradeScale;
            }
            internal set => _defaultGradeScale = value;
        }

        private bool _defaultDoesAverageGradeTotals = false;
        [DataMember]
        public bool DefaultDoesAverageGradeTotals
        {
            get => _defaultDoesAverageGradeTotals;
            set => SetProperty(ref _defaultDoesAverageGradeTotals, value, nameof(DefaultDoesAverageGradeTotals));
        }

        private bool _defaultDoesRoundGradesUp = true;
        [DataMember]
        public bool DefaultDoesRoundGradesUp
        {
            get => _defaultDoesRoundGradesUp;
            set => SetProperty(ref _defaultDoesRoundGradesUp, value, nameof(DefaultDoesRoundGradesUp));
        }

        /// <summary>
        /// Negating since this was added later and will default to false for upgraded accounts.
        /// </summary>
        [DataMember]
        private bool _isSoundEffectsDisabled;

        /// <summary>
        /// Gets or sets whether sound effects are enabled. Does NOT auto-save. True by default.
        /// </summary>
        public bool IsSoundEffectsEnabled
        {
            get => !_isSoundEffectsDisabled;
            set => SetProperty(ref _isSoundEffectsDisabled, !value, nameof(IsSoundEffectsEnabled));
        }

        public async System.Threading.Tasks.Task SaveDefaultGradeScale(GradeScale[] defaultGradeScale)
        {
            _defaultGradeScale = defaultGradeScale;
            NeedsToSyncSettings = true;
            await SaveOnThread();
        }

        [DataMember]
        private MainTileSettings _mainTileSettings;
        public MainTileSettings MainTileSettings
        {
            get
            {
                if (_mainTileSettings == null)
                    _mainTileSettings = new MainTileSettings();

                return _mainTileSettings;
            }
        }

        /// <summary>
        /// Always guaranteed to return an initialized object (if class doesn't have settings, returns default with the SkipItems inherited from primary)
        /// </summary>
        /// <param name="classId"></param>
        /// <returns></returns>
        public async Task<ClassTileSettings> GetClassTileSettings(Guid classId)
        {
            try
            {
                IFile file = await FileSystem.Current.LocalStorage.GetFileByPathAsync(FileNames.ACCOUNT_CLASS_TILES_SETTINGS_PATH(LocalAccountId), classId.ToString() + ".dat");

                if (file != null)
                {
                    using (Stream s = await file.OpenAsync(StorageEverywhere.FileAccess.Read))
                    {
                        ClassTileSettings answer = (ClassTileSettings)GetClassTileSettingsSerializer().ReadObject(s);
                        return answer;
                    }
                }
            }

            catch { }

            // Otherwise, return default object, which inherits the SkipItems option from primary tile
            return new ClassTileSettings()
            {
                SkipItemsOlderThan = MainTileSettings.SkipItemsOlderThan
            };
        }

        public async System.Threading.Tasks.Task SaveClassTileSettings(Guid classId, ClassTileSettings settings)
        {
            // Get the actual destination folder
            IFolder destination = await FileHelper.GetOrCreateClassTilesSettingsFolder(LocalAccountId);

            // Create a temp file to write to
            IFile temp = await destination.CreateFileAsync("Temp" + classId + ".dat", CreationCollisionOption.ReplaceExisting);

            // Write the data to the temp file
            using (Stream s = await temp.OpenAsync(StorageEverywhere.FileAccess.ReadAndWrite))
            {
                GetClassTileSettingsSerializer().WriteObject(s, settings);
            }

            // Move the temp file to the actual file
            await temp.MoveAsync(destination.Path + "/" + classId.ToString() + ".dat", NameCollisionOption.ReplaceExisting);
        }

        public bool ApplySyncedSettings(SyncedSettings settings, long? defaultGradeScaleIndex)
        {
            bool accountChanged = false;

            if (settings.GpaOption != null && this.GpaOption != settings.GpaOption.Value)
            {
                this.GpaOption = settings.GpaOption.Value;
                accountChanged = true;
            }

            if (settings.WeekOneStartsOn != null && this.WeekOneStartsOn != settings.WeekOneStartsOn.Value)
            {
                this.WeekOneStartsOn = settings.WeekOneStartsOn.Value;
                accountChanged = true;
            }

            if (settings.SchoolTimeZone != null)
            {
                try
                {
                    // Sometimes TryGetTimeZoneInfo still throws
                    if (TimeZoneConverter.TZConvert.TryGetTimeZoneInfo(settings.SchoolTimeZone, out TimeZoneInfo serverSchoolTimeZone))
                    {
                        if (!serverSchoolTimeZone.Equals(this.SchoolTimeZone))
                        {
                            this.SchoolTimeZone = serverSchoolTimeZone;
                            accountChanged = true;
                        }
                    }
                }
                catch
                {
                    TelemetryExtension.Current?.TrackEvent("FailedParseOnlineTimeZone", new Dictionary<string, string>()
                    {
                        { "OnlineTimeZone", settings.SchoolTimeZone }
                    });
                }
            }

            if (settings.DefaultGradeScale != null && defaultGradeScaleIndex != null)
            {
                this.CurrentDefaultGradeScaleIndex = defaultGradeScaleIndex.Value;
                this.DefaultGradeScale = settings.DefaultGradeScale;
                accountChanged = true;
            }

            if (settings.DefaultDoesAverageGradeTotals != null && this.DefaultDoesAverageGradeTotals != settings.DefaultDoesAverageGradeTotals.Value)
            {
                this.DefaultDoesAverageGradeTotals = settings.DefaultDoesAverageGradeTotals.Value;
                accountChanged = true;
            }

            if (settings.DefaultDoesRoundGradesUp != null && this.DefaultDoesRoundGradesUp != settings.DefaultDoesRoundGradesUp.Value)
            {
                this.DefaultDoesRoundGradesUp = settings.DefaultDoesRoundGradesUp.Value;
                accountChanged = true;
            }

            return accountChanged;
        }

        private static DataContractSerializer GetClassTileSettingsSerializer()
        {
            return new DataContractSerializer(typeof(ClassTileSettings));
        }


        #endregion


        private int _deviceId;
        [DataMember]
        public int DeviceId
        {
            get { return _deviceId; }
            set { SetProperties(ref _deviceId, value, "DeviceId", "IsOnlineAccount"); }
        }

        private long _accountId;
        [DataMember]
        public long AccountId
        {
            get { return _accountId; }
            set { SetProperties(ref _accountId, value, "AccountId", "IsOnlineAccount"); }
        }

        private string _username;
        [DataMember]
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value, "Username"); }
        }

        private string _localToken;
        [DataMember(Name = "Token")] // Named "Token" for legacy upgrading purposes
        public string LocalToken
        {
            get { return _localToken; }
            set { SetProperty(ref _localToken, value, nameof(LocalToken)); }
        }

        /// <summary>
        /// The online token
        /// </summary>
        [DataMember(Name = "OnlineToken")]
        public string Token { get; set; }

        [DataMember]
        [Obsolete("Use Token instead")]
        public string Password
        {
            // Legacy up-convert
            get { return null; }
            set { if (value != null) LocalToken = value; }
        }

        private bool _rememberUsername;
        [DataMember]
        public bool RememberUsername
        {
            get { return _rememberUsername; }
            set
            {
                SetProperties(ref _rememberUsername, value, "RememberUsername", "IsRememberPasswordPossible", "IsAutoLoginPossible");

                if (value == false)
                    AutoLogin = false;
            }
        }

        private bool _rememberPassword;
        [DataMember]
        public bool RememberPassword
        {
            get { return _rememberPassword; }
            set
            {
                SetProperties(ref _rememberPassword, value, "RememberPassword", "IsAutoLoginPossible");

                if (value == false)
                    AutoLogin = false;
            }
        }

        private bool _autoLogin;
        [DataMember]
        public bool AutoLogin
        {
            get { return _autoLogin; }
            set { SetProperty(ref _autoLogin, value, "AutoLogin"); }
        }

        public bool IsRememberPasswordPossible
        {
            get { return RememberUsername; }
        }

        public bool IsAutoLoginPossible
        {
            get { return RememberUsername && RememberPassword; }
        }

        public bool IsOnlineAccount => CachedComputation(delegate
        {
            return AccountId != 0 && DeviceId != 0;
        }, new string[] { nameof(AccountId), nameof(DeviceId) });

        public bool IsDefaultOfflineAccount => CachedComputation(delegate
        {
            return !IsOnlineAccount && Username == AccountsManager.DefaultOfflineAccountUsername && LocalToken == "";
        }, new string[] { nameof(IsOnlineAccount), nameof(Username), nameof(LocalToken) });

        public string GetTelemetryUserId()
        {
            if (IsDefaultOfflineAccount)
            {
                return "default-" + LocalAccountId;
            }
            else if (IsOnlineAccount)
            {
                return AccountId.ToString();
            }
            else
            {
                return "offline-" + LocalAccountId;
            }
        }

        private System.Threading.Tasks.Task<string> _refreshOnlineTokenTask;
        private System.Threading.Tasks.Task<string> RefreshOnlineTokenAsync()
        {
            if (_refreshOnlineTokenTask == null || _refreshOnlineTokenTask.IsCompleted)
            {
                _refreshOnlineTokenTask = RefreshOnlineTokenHelperAsync();
            }

            return _refreshOnlineTokenTask;
        }

        private async System.Threading.Tasks.Task<string> RefreshOnlineTokenHelperAsync()
        {
            var resp = await PowerPlannerAppAuthLibrary.PowerPlannerAuth.RefreshOnlineTokenAsync(AccountId, Username, LocalToken);
            if (resp.Error == null)
            {
                Token = resp.Token;
                await AccountsManager.Save(this);
                return null;
            }
            else
            {
                return resp.Error;
            }
        }

        public async Task<T> PostAuthenticatedAsync<K, T>(string url, K postData, System.Threading.CancellationToken? cancellationToken = null)
            where K : PartialLoginRequest
            where T : PlainResponse
        {
            if (Token == null)
            {
                // Get the token (this happens from clients upgrading from older versions)
                string error = await RefreshOnlineTokenAsync();
                if (error != null)
                {
                    var answer = (T)Activator.CreateInstance(typeof(T));
                    answer.Error = error;
                    return answer;
                }
            }

            if (Token == null)
            {
                // This theoretically should never get hit
                var answer = (T)Activator.CreateInstance(typeof(T));
                answer.Error = SyncResponse.INCORRECT_CREDENTIALS;
                return answer;
            }

            postData.Login = new LoginCredentials()
            {
                AccountId = AccountId,
                Username = Username,
                Token = Token
            };

            return await WebHelper.Download<K, T>(url, postData, Website.ApiKey, cancellationToken ?? System.Threading.CancellationToken.None);
        }

        public async Task<PlainResponse> RecoverDeletedItemAsync(PastDeletedItem item)
        {
            try
            {
                var resp = await PostAuthenticatedAsync<UndeleteItemRequest, PlainResponse>(
                    Website.ClientApiUrl + "undeleteitemandchildren",
                    new UndeleteItemRequest
                    {
                        Identifier = item.Identifier
                    });

                if (resp.Error == null)
                {
                    try
                    {
                        await Sync.SyncAccountAsync(this);
                    }
                    catch { }
                }

                return resp;
            }
            catch (Exception ex)
            {
                if (ExceptionHelper.IsHttpWebIssue(ex))
                {
                    return new PlainResponse
                    {
                        Error = R.S("String_OfflineExplanation")
                    };
                }

                return new PlainResponse
                {
                    Error = ex.Message
                };
            }
        }

        // This is only needed on Android, but I accidentally was compiling it into the iOS app too, and now the iOS DataContractSerializer won't deserialize
        // legacy accounts that have this property in its saved XML unless this property is present (it's bizzare). So I'm making it show up for all platforms, but
        // marking it obselete on those other platforms. I could have an #if IOS, but that would require adding another target framework and it'd just be for this, not worth it.
        [DataMember]
#if !ANDROID
        [Obsolete("Non-Android platforms should NOT use this property")]
#endif
        public DateTime DateLastDayBeforeReminderWasSent { get; set; }



        /// <summary>
        /// Submits changes.
        /// </summary>
        public async System.Threading.Tasks.Task ConvertToLocal()
        {
            AccountId = 0;
            DeviceId = 0;

            await SaveOnThread();

            //mark all items changed
            //Changes.ClearAll();

            //clear all changes
            throw new NotImplementedException();

            //foreach (BaseItemWin item in School.GetAllChildren())
            //    Changes.Add(item);
            
        }

        /// <summary>
        /// Places all the items into the changes storage so they'll be synced
        /// </summary>
        /// <returns></returns>
        public System.Threading.Tasks.Task ConvertToOnline()
        {
            throw new NotImplementedException();
            //partialChanges = new PartialChanges();

            //foreach (BaseItemWin item in School.GetAllChildren())
            //    partialChanges.New(item.Identifier);

            //await savePartialChanges();
        }

        [DataMember]
        public Dictionary<DayOfWeek, TimeSpan> CustomEndTimes { get; set; } = new Dictionary<DayOfWeek, TimeSpan>();

        /// <summary>
        /// Submits changes
        /// </summary>
        /// <param name="day"></param>
        /// <param name="value"></param>
        public async System.Threading.Tasks.Task SetCustomEndTime(DayOfWeek day, TimeSpan value)
        {
            CustomEndTimes[day] = value;
            
            await SaveOnThread();
        }

        /// <summary>
        /// Submits changes
        /// </summary>
        /// <param name="day"></param>
        public async System.Threading.Tasks.Task RemoveCustomEndTime(DayOfWeek day)
        {
            CustomEndTimes.Remove(day);

            await SaveOnThread();
        }
        
        

        private DateTime selectedDate;
        public DateTime SelectedDate { get { return selectedDate; } set { selectedDate = DateTime.SpecifyKind(value, DateTimeKind.Utc); } }

        private Guid _selectedClass;
        /// <summary>
        /// Not stored, temporary item
        /// </summary>
        public Guid SelectedClass
        {
            get
            {
                return _selectedClass;
            }

            set { _selectedClass = value; }
        }

        private Guid _currentSemesterId;
        /// <summary>
        /// Stored and saved to data. Not guaranteed to be a valid semester ID (for example semester might have been deleted)
        /// </summary>
        [DataMember]
        public Guid CurrentSemesterId
        {
            get { return _currentSemesterId; }
            internal set { _currentSemesterId = value; }
        }

        /// <summary>
        /// Sets current semester and saves changes, also updates primary tile
        /// </summary>
        public async System.Threading.Tasks.Task SetCurrentSemesterAsync(Guid currentSemesterId, bool uploadSettings = true)
        {
            if (CurrentSemesterId == currentSemesterId)
                return;

            // If semester is being cleared (going to Years page), ignore this change.
            // That's to allow easily being able to view overall GPA without losing curr semester.
            if (currentSemesterId == Guid.Empty)
            {
                return;
            }

            CurrentSemesterId = currentSemesterId;

            NeedsToSyncSettings = true;
            IsAppointmentsUpToDate = false;
#if ANDROID
            DateLastDayBeforeReminderWasSent = DateTime.MinValue;
#endif
            await AccountsManager.Save(this);

            // Upload their changed setting
            if (uploadSettings && IsOnlineAccount)
            {
                _ = Sync.SyncSettings(this, Sync.ChangedSetting.SelectedSemesterId);
            }

            var dataStore = await AccountDataStore.Get(this.LocalAccountId);

            AppointmentsExtension.Current?.ResetAll(this, dataStore);
            _ = ClassRemindersExtension.Current?.ResetAllRemindersAsync(this);
            _ = RemindersExtension.Current?.ResetReminders(this, dataStore);

            _ = TilesExtension.Current?.UpdateTileNotificationsForAccountAsync(this, dataStore);
        }

        /// <summary>
        /// Updates tiles and calendar, without awaiting
        /// </summary>
        public async void ExecuteOnLoginTasks()
        {
            try
            {
                var dataStore = await AccountDataStore.Get(this.LocalAccountId);

                // Update tiles
                if (TilesExtension.Current != null)
                {
                    await TilesExtension.Current.UpdateTileNotificationsForAccountAsync(this, dataStore);
                }

                // MainScreenViewModel will start the sync

                // Update calendar if needed
                AppointmentsExtension.Current?.ResetAllIfNeeded(this, dataStore);

            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }


        private Dictionary<Guid, Guid> selectedWeightedGrades;
        public Guid GetSelectedWeightedGrade(Guid classIdentifier)
        {
            Guid weightId;

            selectedWeightedGrades.TryGetValue(classIdentifier, out weightId);

            return weightId;
        }

        public void SetSelectedWeightCategory(Guid classIdentifier, Guid weightIdentifier)
        {
            selectedWeightedGrades[classIdentifier] = weightIdentifier;
        }

        public void ResetSelectedDateAndClass()
        {
            SelectedClass = Guid.Empty;
            SelectedDate = DateTime.Today;
            selectedWeightedGrades = new Dictionary<Guid, Guid>();
        }
        

        private DateTime _premiumAccountExpiresOn = DateTime.SpecifyKind(SqlDate.MinValue, DateTimeKind.Utc);
        /// <summary>
        /// Is always UTC time
        /// </summary>
        [DataMember]
        public DateTime PremiumAccountExpiresOn
        {
            get { return _premiumAccountExpiresOn; }
            set { SetProperty(ref _premiumAccountExpiresOn, value.ToUniversalTime(), "PremiumAccountExpiresOn"); }
        }

        public bool IsLifetimePremiumAccount
        {
            get { return PremiumAccountExpiresOn == PowerPlannerSending.DateValues.LIFETIME_PREMIUM_ACCOUNT; }
        }

        /// <summary>
        /// Saves locally
        /// </summary>
        /// <returns></returns>
        public async System.Threading.Tasks.Task SetAsLifetimePremiumAsync()
        {
            if (PremiumAccountExpiresOn != DateValues.LIFETIME_PREMIUM_ACCOUNT)
            {
                PremiumAccountExpiresOn = DateValues.LIFETIME_PREMIUM_ACCOUNT;

                await AccountsManager.Save(this);
            }
        }

        public DateTime LastSyncOn { get; internal set; }

        /// <summary>
        /// If no schedules on that day, returns null.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="classes"></param>
        /// <returns></returns>
        public DateTime? GetClassEndTime(DateTime date, IEnumerable<ViewItemClass> classes)
        {
            date = DateTime.SpecifyKind(date.Date, DateTimeKind.Local);

            //if they're using custom end times
            if (this.CustomEndTimes.ContainsKey(date.DayOfWeek))
                return date.Add(this.CustomEndTimes[date.DayOfWeek]);

            PowerPlannerSending.Schedule.Week week = this.GetWeekOnDifferentDate(date);

            //otherwise get all the schedules
            IEnumerable<ViewItemSchedule> schedules;
            
            schedules = classes.SelectMany(i => i.Schedules).Where(i => i.DayOfWeek == date.DayOfWeek && (i.ScheduleWeek == week || i.ScheduleWeek == PowerPlannerSending.Schedule.Week.BothWeeks)).ToArray();

            //if there aren't any schedules on that day
            if (!schedules.Any())
                return null;

            return date.Add(schedules.Max(i => i.EndTimeInLocalTime(date).TimeOfDay));
        }
    }
}
