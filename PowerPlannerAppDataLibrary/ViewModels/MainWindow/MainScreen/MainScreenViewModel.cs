using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Agenda;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Classes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Schedule;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Years;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using BareMvvm.Core.ViewModels;
using static PowerPlannerAppDataLibrary.NavigationManager;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.SyncLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Holiday;
using PowerPlannerAppDataLibrary.Exceptions;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public class MainScreenViewModel : PagedViewModelWithPopups
    {
        public enum SyncStates
        {
            Syncing,
            UploadingImages,
            Done
        }

        private SyncStates _syncState = SyncStates.Done;
        public SyncStates SyncState
        {
            get { return _syncState; }
            set
            {
                if (value == SyncStates.Done && UploadImageProgress < 1 && UploadImageProgress > 0)
                {
                    value = SyncStates.UploadingImages;
                }

                SetProperty(ref _syncState, value, nameof(SyncState));
            }
        }

        public Guid CurrentLocalAccountId
        {
            get
            {
                if (CurrentAccount == null)
                    return Guid.Empty;

                return CurrentAccount.LocalAccountId;
            }
        }


        public AccountDataItem CurrentAccount { get; private set; }

        public Guid CurrentSemesterId { get; private set; }

        public ViewItemSemester CurrentSemester
        {
            get
            {
                if (ScheduleViewItemsGroup == null)
                    return null;

                return ScheduleViewItemsGroup.Semester;
            }
        }

        /// <summary>
        /// Required to be a valid semester ID
        /// </summary>
        /// <param name="semesterId"></param>
        /// <returns></returns>
        public async Task SetCurrentSemester(Guid semesterId, bool alwaysNavigate = false)
        {
            if (CurrentSemesterId == semesterId)
            {
                if (alwaysNavigate)
                {
                    // This is only called in case where we have a semester, so there'll always be available items
                    if (SelectedItem != AvailableItems.First())
                    {
                        SelectedItem = AvailableItems.First();
                    }

                    if (PowerPlannerApp.DoNotShowYearsInTabItems)
                    {
                        // We need to clear the Years page
                        Popups.Clear();
                    }
                }
                return;
            }
            
            await CurrentAccount.SetCurrentSemesterAsync(semesterId);

            CurrentSemesterId = semesterId;
            await OnSemesterChanged(); // Loads the classes
            UpdateAvailableItemsAndTriggerUpdateDisplay();

            if (AvailableItems.Any())
            {
                SelectedItem = AvailableItems.First();
            }
            else
            {
                SelectedItem = null;
            }

            if (PowerPlannerApp.DoNotShowYearsInTabItems && AvailableItems.Any())
            {
                // We need to clear the Years page
                Popups.Clear();
            }
        }

        private MainScreenViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            CurrentAccount = account;

            AccountDataStore.DataChangedEvent += new WeakEventHandler<DataChangedEvent>(AccountDataStore_DataChangedEvent).Handler;
            Sync.SyncQueued += new WeakEventHandler<SyncQueuedEventArgs>(Sync_SyncQueued).Handler;
            Sync.UploadImageProgress += new WeakEventHandler<UploadImageProgressEventArgs>(Sync_UploadImageProgress).Handler;

            base.PropertyChanged += MainScreenViewModel_PropertyChanged;
        }

        private double _uploadImageProgress;
        public double UploadImageProgress
        {
            get { return _uploadImageProgress; }
            set { SetProperty(ref _uploadImageProgress, value, nameof(UploadImageProgress)); }
        }

        private void Sync_UploadImageProgress(object sender, UploadImageProgressEventArgs e)
        {
            if (CurrentAccount == null)
            {
                return;
            }

            if (e.Account.LocalAccountId == CurrentAccount.LocalAccountId)
            {
                if (e.Progress >= 1)
                {
                    if (SyncState != SyncStates.Syncing)
                    {
                        DispatchAndUpdateSyncStateProperties(delegate
                        {
                            UploadImageProgress = 1;
                            SyncState = SyncStates.Done;
                        });
                    }
                    else
                    {
                        // Silently set upload image progress... a new sync operation is currently happening which takes priority
                        _uploadImageProgress = 1;
                    }
                }

                else
                {
                    if (SyncState != SyncStates.Syncing)
                    {
                        DispatchAndUpdateSyncStateProperties(delegate
                        {
                            SyncState = SyncStates.UploadingImages;
                            UploadImageProgress = e.Progress;
                        });
                    }
                    else
                    {
                        // Silently set image progress... a new sync operation is currently happening which takes priority
                        _uploadImageProgress = e.Progress;
                    }
                }
            }
        }

        private int _updateSyncStatesPropertiesOperation = 0;
        private void DispatchAndUpdateSyncStateProperties(Action action)
        {
            try
            {
                _updateSyncStatesPropertiesOperation++;
                int thisOperation = _updateSyncStatesPropertiesOperation;
                PortableDispatcher.GetCurrentDispatcher().Run(delegate
                {
                    try
                    {
                        if (_updateSyncStatesPropertiesOperation != thisOperation)
                        {
                            return;
                        }

                        action();
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

        private List<LoggedError> _unreadSyncErrors = new List<LoggedError>();

        private bool _hasSyncErrors;
        public bool HasSyncErrors
        {
            get { return _hasSyncErrors; }
            private set { SetProperty(ref _hasSyncErrors, value, nameof(HasSyncErrors)); }
        }

        private bool _isOffline;
        public bool IsOffline
        {
            get { return _isOffline; }
            private set { SetProperty(ref _isOffline, value, nameof(IsOffline)); }
        }

        private void SetSyncErrors(IEnumerable<LoggedError> errors)
        {
            HasSyncErrors = true;
            _unreadSyncErrors = new List<DataLayer.LoggedError>(errors);
        }

        private void ClearSyncErrors()
        {
            _unreadSyncErrors.Clear();
            HasSyncErrors = false;
        }

        public void ViewSyncErrors()
        {
            if (_unreadSyncErrors.Count == 0)
            {
                return;
            }

            ShowPopup(new SyncErrorsViewModel(this, _unreadSyncErrors.ToArray()));
        }

        public void SyncCurrentAccount()
        {
            if (CurrentAccount != null)
            {
                try
                {
                    var dontWait = PowerPlannerAppDataLibrary.SyncLayer.Sync.SyncAccountAsync(CurrentAccount);
                }

                catch { }
            }
        }

        private Task<SyncResult> _currSyncResultTask;
        private async void Sync_SyncQueued(object sender, SyncQueuedEventArgs e)
        {
            try
            {
                await Dispatcher.RunOrFallbackToCurrentThreadAsync(async delegate
                {
                    try
                    {
                        if (CurrentAccount != null && CurrentAccount.LocalAccountId == e.Account.LocalAccountId)
                        {
                            SyncState = SyncStates.Syncing;

                            _currSyncResultTask = e.ResultTask;

                            SyncResult result;

                            try
                            {
                                result = await e.ResultTask;
                            }

                            catch (OperationCanceledException) { result = null; }

                            // If this is still the task we're considering for intermediate, then we clear intermediate (if another was queued, it wouldn't match task)
                            if (_currSyncResultTask == e.ResultTask)
                            {
                                if (UploadImageProgress > 0 && UploadImageProgress < 1)
                                {
                                    SyncState = SyncStates.UploadingImages;
                                }
                                else
                                {
                                    SyncState = SyncStates.Done;
                                }
                            }
                            else
                                return;

                            // Canceled
                            if (result == null)
                                return;

                            else if (result.Error != null)
                            {
                                if (result.Error.Equals("Offline."))
                                {
                                    IsOffline = true;
                                    ClearSyncErrors();
                                }

                                else if (result.Error.Equals(PowerPlannerSending.SyncResponse.NO_ACCOUNT))
                                {
                                    IsOffline = false;
                                    SetSyncErrors(new LoggedError[] {
                                        new LoggedError("Online account deleted", "Your online account was deleted and no longer exists.")
                                    });
                                }

                                else
                                {
                                    IsOffline = false;
                                    SetSyncErrors(new LoggedError[] {
                                        new LoggedError("Sync Error", result.Error)
                                    });
                                }

                                if (result.Error.Equals(PowerPlannerSending.SyncResponse.INCORRECT_PASSWORD) || result.Error.Equals(PowerPlannerSending.SyncResponse.USERNAME_CHANGED) || result.Error.Equals(PowerPlannerSending.SyncResponse.DEVICE_NOT_FOUND))
                                {
                                    ShowPopupUpdateCredentials(CurrentAccount);
                                    return;
                                }

                                else if (result.Error.Equals(PowerPlannerSending.SyncResponse.DEVICE_NOT_FOUND) || result.Error.Equals(PowerPlannerSending.SyncResponse.NO_DEVICE))
                                {
                                    ShowPopupUpdateCredentials(CurrentAccount, UpdateCredentialsViewModel.UpdateTypes.NoDevice);
                                }
                            }

                            else if (result.UpdateErrors != null && result.UpdateErrors.Count > 0)
                            {
                                IsOffline = false;
                                SetSyncErrors(result.UpdateErrors.Select(i => new LoggedError("Sync Item Error", i.Name + ": " + i.Message)));
                            }

                            else
                            {
                                // All good!
                                IsOffline = false;
                                ClearSyncErrors();
                            }
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

        private void ShowPopupUpdateCredentials(AccountDataItem account, UpdateCredentialsViewModel.UpdateTypes updateType = UpdateCredentialsViewModel.UpdateTypes.Normal)
        {
            ShowPopup(UpdateCredentialsViewModel.Create(this, account, updateType));
        }

        private void MainScreenViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Content):
                    OnContentChanged();
                    break;
            }
        }

        private void OnContentChanged()
        {
            if (Content != null)
            {
                var selection = GetCurrentSelectionBasedOnContent();

                if (Content is ClassViewModel || Content is ClassesViewModel)
                {
                    UpdateSelectedClass();
                }

                if (selection != _selectedItem)
                {
                    _selectedItem = selection;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        private void UpdateSelectedClass()
        {
            if (Content is ClassViewModel)
            {
                ClassViewModel viewModel = Content as ClassViewModel;

                SelectedClass = Classes.FirstOrDefault(i => i.Identifier == viewModel.ClassId);
            }

            else if (Content is ClassesViewModel)
            {
                SelectedClass = null;
            }
        }

        private static Dictionary<Type, MainMenuSelections> ContentTypesToMenuSelections = new Dictionary<Type, MainMenuSelections>()
        {
            { typeof(CalendarViewModel), MainMenuSelections.Calendar },
            { typeof(DayViewModel), MainMenuSelections.Day },
            { typeof(AgendaViewModel), MainMenuSelections.Agenda },
            { typeof(ScheduleViewModel), MainMenuSelections.Schedule },
            { typeof(ClassViewModel), MainMenuSelections.Classes },
            { typeof(ClassesViewModel), MainMenuSelections.Classes },
            { typeof(YearsViewModel), MainMenuSelections.Years },
            { typeof(SettingsViewModel), MainMenuSelections.Settings },
            { typeof(ClassWhatIfViewModel), MainMenuSelections.Classes }
        };

        private MainMenuSelections GetCurrentSelectionBasedOnContent()
        {
            if (Content == null)
                throw new NullReferenceException("Content was null");

            if (!ContentTypesToMenuSelections.ContainsKey(Content.GetType()))
            {
                throw new KeyNotFoundException("Please register this content type for menu item selection");
            }

            return ContentTypesToMenuSelections[Content.GetType()];
        }

        public static async Task<MainScreenViewModel> LoadAsync(BaseViewModel parent, AccountDataItem account, bool syncAccount = true)
        {
            if (account == null)
                throw new ArgumentNullException("account");

            MainScreenViewModel model = new MainScreenViewModel(parent, account);

            if (account.CurrentSemesterId != Guid.Empty)
            {
                // Check that semester is valid
                if (await CheckThatSemesterIsValidAsync(account.LocalAccountId, account.CurrentSemesterId))
                {
                    model.CurrentSemesterId = account.CurrentSemesterId;
                    await model.OnSemesterChanged(); // Loads the classes
                }
            }

            model.updateAvailableItems();

            MainMenuSelections? selectedItem = null;

            if (model.AvailableItems.Contains(NavigationManager.MainMenuSelection))
                selectedItem = NavigationManager.MainMenuSelection;
            else if (model.AvailableItems.Any())
            {
                selectedItem = model.AvailableItems.First();
            }

            if (!PowerPlannerApp.ShowClassesAsPopups && selectedItem.GetValueOrDefault() == NavigationManager.MainMenuSelections.Classes && model.Classes != null)
            {
                var c = model.Classes.FirstOrDefault(i => NavigationManager.ClassSelection == i.Identifier);
                if (c != null)
                {
                    await model.SelectClass(c.Identifier);
                }
                else
                {
                    model.SelectedItem = selectedItem;
                }
            }
            else
            {
                model.SelectedItem = selectedItem;
            }

            try
            {
                if (syncAccount)
                {
                    var dontWait = Sync.SyncAccountAsync(account);
                }
            }

            catch { }

            return model;
        }

        private static async Task<bool> CheckThatSemesterIsValidAsync(Guid localAccountId, Guid semesterId)
        {
            return await Task.Run(async delegate
            {
                return await CheckThatSemesterIsValidBlocking(localAccountId, semesterId);
            });
        }

        private static async Task<bool> CheckThatSemesterIsValidBlocking(Guid localAccountId, Guid semesterId)
        {
            var dataStore = await AccountDataStore.Get(localAccountId);

            using (await Locks.LockDataForReadAsync())
            {
                return dataStore.TableSemesters.Count(i => i.Identifier == semesterId) > 0;
            }
        }

        private async void AccountDataStore_DataChangedEvent(object sender, DataChangedEvent e)
        {
            lock (_changedItemListeners)
            {
                for (int i = 0; i < _changedItemListeners.Count; i++)
                {
                    IDataChangeListener listener;
                    _changedItemListeners[i].TryGetTarget(out listener);

                    if (listener == null)
                    {
                        _changedItemListeners.RemoveAt(i);
                        i--;
                    }

                    else
                    {
                        listener.HandleDataChangedEvent(e);
                    }
                }
            }

            // If there's no semester right now, nothing needs changing
            if (CurrentSemesterId == Guid.Empty)
                return;

            // If the changes are for this account
            if (e.LocalAccountId == CurrentLocalAccountId)
            {
                try
                {
                    // We fall back to current thread since the view model should remain correct even
                    // if the view is disconnected.
                    await Dispatcher.RunOrFallbackToCurrentThreadAsync(async delegate
                    {
                        try
                        {
                            // If this semester was deleted
                            if (e.DeletedItems.Contains(CurrentSemesterId))
                            {
                                await SetCurrentSemester(Guid.Empty);
                                return;
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
        }

        private List<WeakReference<IDataChangeListener>> _changedItemListeners = new List<WeakReference<IDataChangeListener>>();

        public ChangedItemListener ListenToItem(Guid identifier)
        {
            var listener = new ChangedItemListener(CurrentLocalAccountId, identifier, Dispatcher);
            _changedItemListeners.Add(new WeakReference<IDataChangeListener>(listener));
            return listener;
        }

        public ItemsEditedLocallyListener<T> ListenToLocalEditsFor<T>() where T : BaseDataItem
        {
            var listener = new ItemsEditedLocallyListener<T>(CurrentLocalAccountId);
            _changedItemListeners.Add(new WeakReference<IDataChangeListener>(listener));
            return listener;
        }

        public interface IDataChangeListener
        {
            void HandleDataChangedEvent(DataChangedEvent e);
        }

        public class ItemsEditedLocallyListener<T> : IDataChangeListener where T : BaseDataItem
        {
            /// <summary>
            /// Note that this does NOT run on the UI thread
            /// </summary>
            public event EventHandler<Guid[]> ChangedItems;
            private Guid _localAccountId;

            public ItemsEditedLocallyListener(Guid accountId)
            {
                _localAccountId = accountId;
            }

            public void HandleDataChangedEvent(DataChangedEvent e)
            {
                if (e.LocalAccountId == _localAccountId && e.WasLocalChanges)
                {
                    var changedItems = e.NewItems.Union(e.EditedItems).OfType<T>().Select(i => i.Identifier).ToArray();
                    if (changedItems.Length > 0 && ChangedItems != null)
                    {
                        ChangedItems.Invoke(this, changedItems);
                    }
                }
            }
        }

        public class ChangedItemListener : IDataChangeListener
        {
            public event EventHandler Deleted;
            private Guid _localAccountId;
            private Guid _identifier;
            private PortableDispatcher _dispatcher;

            public ChangedItemListener(Guid accountId, Guid identifier, PortableDispatcher dispatcher)
            {
                _localAccountId = accountId;
                _identifier = identifier;
                _dispatcher = dispatcher;
            }

            public void HandleDataChangedEvent(DataChangedEvent e)
            {
                if (e.LocalAccountId == _localAccountId)
                {
                    if (e.DeletedItems.Contains(_identifier))
                    {
                        _dispatcher.Run(delegate
                        {
                            Deleted?.Invoke(this, new EventArgs());
                        });
                    }
                }
            }
        }

        private void UpdateAvailableItemsAndTriggerUpdateDisplay()
        {
            updateAvailableItems();
        }

        //void Data_OnChangesDone(object sender, EventArgs e)
        //{
        //    updateAvailableItems();

        //    //if there's currently a selected class, we'll make sure it's valid
        //    if (SelectedClass != null)
        //    {
        //        //if the class is no longer in the active semester, be sure to clear it (might have been deleted)
        //        if (Store.Data.ActiveSemester == null || !Store.Data.ActiveSemester.Classes.Contains(SelectedClass))
        //            SelectedClass = null;
        //    }
        //}

        private NavigationManager.MainMenuSelections? _selectedItem;
        /// <summary>
        /// Will log the user out if LogIn is selected. Will set active semester to null if Years is selected.
        /// </summary>
        public NavigationManager.MainMenuSelections? SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                // The actual property will be written when the page content changes
                if (value != null)
                    setSelectedItem(value.Value);
            }
        }

        private ViewItemClass _selectedClass;
        public ViewItemClass SelectedClass
        {
            get
            {
                if (Classes.Contains(_selectedClass))
                    return _selectedClass;

                return null;
            }

            private set { SetProperty(ref _selectedClass, value, "SelectedClass"); }
        }

        /// <summary>
        /// Selects the class, or if not found, searches all the semesters and switches to that semester in order to select the class
        /// </summary>
        /// <param name="classId"></param>
        /// <returns></returns>
        public async Task<bool> SelectClass(Guid classId)
        {
            if (classId == Guid.Empty)
                return false;

            // If there's no classes, or none matched, we need to check other semesters
            if (Classes == null || !Classes.Any(i => i.Identifier == classId))
            {
                var data = await AccountDataStore.Get(CurrentLocalAccountId);

                // Otherwise we have to see what semester this class might be in...
                Guid semesterId = await data.GetSemesterIdForClassAsync(classId);

                if (semesterId == Guid.Empty)
                    return false;

                await SetCurrentSemester(semesterId);
            }


            // Now try selecting the class
            if (Classes != null)
            {
                return SelectClassWithoutLoading(classId, false);
            }

            return false;
        }

        public void KeepBackStack()
        {
            _shouldKeepBackStack = true;
        }

        private bool _shouldKeepBackStack;

        /// <summary>
        /// Call this once when navigating. Calling this resets it to false.
        /// </summary>
        /// <returns></returns>
        public bool ShouldKeepBackStack()
        {
            if (_shouldKeepBackStack)
            {
                _shouldKeepBackStack = false;
                return true;
            }

            return false;
        }

        private bool SelectClassWithoutLoading(Guid classId, bool allowGoingBack)
        {
            if (Classes == null)
                return false;

            ViewItemClass c;

            if (classId == Guid.Empty)
                c = null;
            else
            {
                c = Classes.FirstOrDefault(i => i.Identifier == classId);
                if (c == null)
                    return false;
            }
            
            NavigationManager.ClassSelection = classId;

            if (c != null)
            {
                SetContent(new ClassViewModel(this, CurrentLocalAccountId, classId, DateTime.Today, CurrentSemester), preserveBack: allowGoingBack);
            }
            else
            {
                SetContent(new ClassesViewModel(this));
            }
            return true;
        }

        public void SelectClassWithinSemester(ViewItemClass classToSelect, bool allowGoingBack = false)
        {
            if (classToSelect == null)
                SelectClassWithoutLoading(Guid.Empty, allowGoingBack);
            else
                SelectClassWithoutLoading(classToSelect.Identifier, allowGoingBack);
        }

        private void setSelectedItem(NavigationManager.MainMenuSelections value)
        {
            if (!AvailableItems.Contains(value))
            {
                return;
            }

            // If already selected, do nothing
            if (value == SelectedItem)
                return;
            
            NavigationManager.MainMenuSelection = value;

            updateAvailableItems();

            switch (value)
            {
                case NavigationManager.MainMenuSelections.Calendar:
                    SetContent(new CalendarViewModel(this, CurrentLocalAccountId, CurrentSemester));
                    break;

                case NavigationManager.MainMenuSelections.Day:
                    SetContent(new DayViewModel(this, CurrentLocalAccountId, CurrentSemester));
                    break;

                case NavigationManager.MainMenuSelections.Agenda:
                    SetContent(new AgendaViewModel(this, CurrentLocalAccountId, CurrentSemester, DateTime.Today));
                    break;

                case MainMenuSelections.Schedule:
                    SetContent(new ScheduleViewModel(this));
                    break;

                case MainMenuSelections.Classes:

                    if (this.SelectedClass != null)
                        SetContent(new ClassViewModel(this, CurrentLocalAccountId, SelectedClass.Identifier, DateTime.Today, CurrentSemester));
                    else
                        SetContent(new ClassesViewModel(this));

                    break;

                case MainMenuSelections.Years:
                    SetContent(new YearsViewModel(this));
                    break;

                case MainMenuSelections.Settings:
                    SetContent(new SettingsViewModel(this));
                    break;
            }
        }

        private EventHandler<DataChangedEvent> _scheduleChangesOccurredHandler;
        private async Task OnSemesterChanged()
        {
            // Null this out so that when we set the item for the new semester, it loads
            _selectedItem = null;

            // Restore the default stored items
            NavigationManager.RestoreDefaultMemoryItems();

            // Disconnect the previous
            if (ScheduleViewItemsGroup != null)
            {
                Classes.EndMakeThisACopyOf(ScheduleViewItemsGroup.Classes);
                if (_scheduleChangesOccurredHandler != null)
                {
                    ScheduleViewItemsGroup.OnChangesOccurred -= _scheduleChangesOccurredHandler;
                }
                ScheduleViewItemsGroup = null;
            }

            // Clear the current classes
            Classes.Clear();

            // If there's believed to be a semester (although that semester may not exist, this simply means Guid isn't empty)
            if (!hasNoSemester())
            {
                // Load the classes/schedules
                try
                {
                    ScheduleViewItemsGroup = await ScheduleViewItemsGroup.LoadAsync(this.CurrentLocalAccountId, this.CurrentSemesterId);
                }
                catch (SemesterNotFoundException)
                {
                    // Semester didn't actually exist
                    CurrentSemesterId = Guid.Empty;
                    return;
                }

                // Change the default date

                // If semester has already ended
                if (!PowerPlannerSending.DateValues.IsUnassigned(CurrentSemester.End) && DateTime.SpecifyKind(CurrentSemester.End, DateTimeKind.Local).Date < DateTime.Today)
                {
                    NavigationManager.SetDisplayMonth(DateTime.SpecifyKind(CurrentSemester.End, DateTimeKind.Local), preserveForever: true);
                    NavigationManager.SetSelectedDate(DateTime.SpecifyKind(CurrentSemester.End, DateTimeKind.Local), preserveForever: true);
                }

                // If semester hasn't started yet
                else if (!PowerPlannerSending.DateValues.IsUnassigned(CurrentSemester.Start) && DateTime.SpecifyKind(CurrentSemester.Start, DateTimeKind.Local).Date > DateTime.Today)
                {
                    NavigationManager.SetDisplayMonth(DateTime.SpecifyKind(CurrentSemester.Start, DateTimeKind.Local), preserveForever: true);
                    NavigationManager.SetSelectedDate(DateTime.SpecifyKind(CurrentSemester.Start, DateTimeKind.Local), preserveForever: true);
                }

                // Make this a copy of the Classes list
                Classes.MakeThisACopyOf(ScheduleViewItemsGroup.Classes);

                if (_scheduleChangesOccurredHandler == null)
                {
                    _scheduleChangesOccurredHandler = new WeakEventHandler<DataChangedEvent>(ViewModelSchedule_OnChangesOccurred).Handler;
                }
                ScheduleViewItemsGroup.OnChangesOccurred += _scheduleChangesOccurredHandler;
            }
        }

        private void ViewModelSchedule_OnChangesOccurred(object sender, DataChangedEvent e)
        {
            UpdateAvailableItemsAndTriggerUpdateDisplay();
        }

        #region ItemLists

        private static readonly NavigationManager.MainMenuSelections[] DEFAULT_ITEMS = new NavigationManager.MainMenuSelections[]
        {
            NavigationManager.MainMenuSelections.Calendar,
            NavigationManager.MainMenuSelections.Day,
            NavigationManager.MainMenuSelections.Agenda,
            NavigationManager.MainMenuSelections.Schedule,
            NavigationManager.MainMenuSelections.Classes,
            NavigationManager.MainMenuSelections.Years,
            NavigationManager.MainMenuSelections.Settings
        };

        private static MainMenuSelections[] GetDefaultItems()
        {
            return DEFAULT_ITEMS;
        }

        private static readonly NavigationManager.MainMenuSelections[] SELECTING_SEMESTER_ITEMS = new NavigationManager.MainMenuSelections[]
        {
            NavigationManager.MainMenuSelections.Years,
            NavigationManager.MainMenuSelections.Settings
        };

        private static readonly NavigationManager.MainMenuSelections[] NO_CLASSES_ITEMS = new NavigationManager.MainMenuSelections[]
        {
            NavigationManager.MainMenuSelections.Schedule,
            NavigationManager.MainMenuSelections.Classes,
            NavigationManager.MainMenuSelections.Years,
            NavigationManager.MainMenuSelections.Settings
        };

        #endregion

        private bool hasNoSemester()
        {
            return CurrentSemesterId == Guid.Empty;
        }

        private bool hasNoClasses()
        {
            return hasNoSemester() || Classes.Count == 0;
        }

        private ObservableCollection<NavigationManager.MainMenuSelections> _availableItems = new ObservableCollection<NavigationManager.MainMenuSelections>(GetDefaultItems());
        public ObservableCollection<NavigationManager.MainMenuSelections> AvailableItems
        {
            get { return _availableItems; }
        }

        public ScheduleViewItemsGroup ScheduleViewItemsGroup { get; private set; }

        public MyObservableList<ViewItemClass> Classes { get; private set; } = new MyObservableList<ViewItemClass>();

        /// <summary>
        /// Restores the dates to today, etc.
        /// </summary>
        private void restoreDefaultMemoryItems()
        {
            NavigationManager.RestoreDefaultMemoryItems();
        }

        /// <summary>
        /// Does not modify the current SelectedItem
        /// </summary>
        /// <returns></returns>
        private bool updateAvailableItems()
        {
            // if they haven't picked a semester, we MUST display selecting semester options
            if (hasNoSemester())
            {
                _selectedClass = null;

                if (makeAvailableItemsLike(SELECTING_SEMESTER_ITEMS))
                    restoreDefaultMemoryItems();
            }

            else if (hasNoClasses())
            {
                _selectedClass = null;

                if (makeAvailableItemsLike(NO_CLASSES_ITEMS))
                    restoreDefaultMemoryItems();
            }

            else
                makeAvailableItemsLike(GetDefaultItems());
            



            // make sure the class is valid too
            //if (SelectedItem == NavigationManager.MainMenuSelections.Classes)
            //{
            //    if (updateSelectedClass())
            //        return true;
            //}

            return false;
        }

        /// <summary>
        /// Returns true if changes were made
        /// </summary>
        /// <param name="desired"></param>
        /// <returns></returns>
        private bool makeAvailableItemsLike(params NavigationManager.MainMenuSelections[] desired)
        {
            var desiredList = new List<MainMenuSelections>(desired);

            if (PowerPlannerApp.UseUnifiedCalendarDayTabItem)
            {
                desiredList.Remove(MainMenuSelections.Day);
            }

            if (PowerPlannerApp.DoNotShowYearsInTabItems)
            {
                desiredList.Remove(MainMenuSelections.Years);
            }

            if (PowerPlannerApp.DoNotShowSettingsInTabItems)
            {
                desiredList.Remove(MainMenuSelections.Settings);
            }

            bool answer = IListExtensions.MakeListLike(_availableItems, desiredList);

            if (PowerPlannerApp.DoNotShowYearsInTabItems && !AvailableItems.Any() && Popups.Count == 0)
            {
                OpenYears();
            }

            return answer;
        }

        public void SetContent(BaseViewModel viewModel, bool preserveBack = false)
        {
            if (preserveBack)
                base.Navigate(viewModel);
            else
            {
                base.ClearBackStack();
                base.Replace(viewModel);
            }
        }

        public void AddClass(bool navigateToClassAfterAdd = false, Action<DataLayer.DataItems.DataItemClass> onClassAddedAction = null)
        {
            if (Classes == null)
            {
                throw new InvalidOperationException("Classes list was null");
            }

            if (CurrentSemesterId == Guid.Empty)
            {
                throw new InvalidOperationException("CurrentSemesterId was empty");
            }

            ShowPopup(AddClassViewModel.CreateForAdd(this, new AddClassViewModel.AddParameter()
            {
                Classes = Classes.ToArray(),
                SemesterIdentifier = CurrentSemesterId,
                NavigateToClassAfterAdd = navigateToClassAfterAdd,
                OnClassAddedAction = onClassAddedAction
            }));
        }

        public void EditClass(ViewItemClass c)
        {
            ShowPopup(AddClassViewModel.CreateForEdit(this, c));
        }

        public void ShowItem(ViewItemTaskOrEvent item)
        {
            ShowPopup(ViewTaskOrEventViewModel.Create(this, item));
        }

        public void EditTaskOrEvent(ViewItemTaskOrEvent item)
        {
            ShowPopup(AddTaskOrEventViewModel.CreateForEdit(this, new AddTaskOrEventViewModel.EditParameter()
            {
                Item = item
            }));
        }

        public void ConvertTaskOrEventType(ViewItemTaskOrEvent item)
        {
            ViewTaskOrEventViewModel.Create(this, item).ConvertType();
        }

        public void SetTaskOrEventPercentComplete(ViewItemTaskOrEvent item, double percentComplete)
        {
            ViewTaskOrEventViewModel.Create(this, item).SetPercentComplete(percentComplete);
        }

        public void EditGrade(BaseViewItemMegaItem grade, bool whatIf = false)
        {
            ShowPopup(AddGradeViewModel.CreateForEdit(this, new AddGradeViewModel.EditParameter()
            {
                Item = grade,
                IsInWhatIfMode = whatIf
            }));
        }

        public Task DeleteItem(BaseViewItem item)
        {
            return DeleteItem(item.Identifier);
        }

        public async Task DeleteItem(Guid identifier)
        {
            DataChanges changes = new DataChanges();
            changes.DeleteItem(identifier);

            await PowerPlannerApp.Current.SaveChanges(changes);
        }

        public void ViewHoliday(ViewItemHoliday holiday)
        {
            this.ShowPopup(AddHolidayViewModel.CreateForEdit(this, holiday));
        }

        public void OpenYears()
        {
            try
            {
                if (!PowerPlannerApp.DoNotShowYearsInTabItems)
                {
                    throw new InvalidOperationException("If you're using this, you should have set DoNotShowYearsInTabItems to true");
                }

                ShowPopup(new YearsViewModel(this));
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void OpenSettings()
        {
            try
            {
                if (!PowerPlannerApp.DoNotShowSettingsInTabItems)
                {
                    throw new InvalidOperationException("If you're using this, you should have set DoNotShowSettingsInTabItems to true");
                }

                ShowPopup(new SettingsViewModel(this));
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void ViewClass(ViewItemClass c, ClassViewModel.ClassPages? initialPage = null)
        {
            if (PowerPlannerApp.ShowClassesAsPopups)
            {
                OpenClassAsPopup(c, initialPage);
            }
            else
            {
                Navigate(new ClassViewModel(this, CurrentLocalAccountId, c.Identifier, DateTime.Today, CurrentSemester)
                {
                    InitialPage = initialPage
                });
            }
        }

        private void OpenClassAsPopup(ViewItemClass c, ClassViewModel.ClassPages? initialPage = null)
        {
            try
            {
                if (!PowerPlannerApp.ShowClassesAsPopups)
                {
                    throw new InvalidOperationException("If you're using this, you should have set ShowClassesAsPopups to true");
                }

                if (c == null)
                {
                    throw new ArgumentNullException(nameof(c));
                }

                if (CurrentSemester == null)
                {
                    throw new InvalidOperationException("CurrentSemester was null");
                }

                ShowPopup(new ClassViewModel(this, CurrentLocalAccountId, c.Identifier, DateTime.Today, CurrentSemester)
                {
                    InitialPage = initialPage
                });
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
