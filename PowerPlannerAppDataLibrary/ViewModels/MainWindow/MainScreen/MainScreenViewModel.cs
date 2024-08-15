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
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using BareMvvm.Core.Snackbar;
using Vx.Views;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.Components;
using System.Drawing;
using Vx;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public class MainScreenViewModel : PagedViewModelWithPopups
    {
        protected override View Render()
        {
            if (Size.Width == 0)
            {
                return null;
            }

            return new LinearLayout
            {
                Orientation = IsCompactMode ? Orientation.Vertical : Orientation.Horizontal,
                Children =
                {
                    IsCompactMode ? new FrameLayout() : RenderSidebar(),

                    new PagedViewModelPresenterView
                    {
                        ViewModel = this
                    }.LinearLayoutWeight(1),

                    IsCompactMode ? new BottomNavBar
                    {
                        SelectedItem = SelectedItem,
                        SetSelectedItem = i =>
                        {
                            if (i == MainMenuSelections.Classes && SelectedItem == MainMenuSelections.Classes && SelectedClass != null)
                            {
                                // Revert back to class picker
                                SelectClassWithinSemester(null);
                            }
                            else
                            {
                                SelectedItem = i;
                            }
                        },
                        IsOfflineOrHasSyncError = IsOffline || HasSyncErrors,
                        SyncState = SyncState,
                        UploadImageProgress = UploadImageProgress
                    } : null
                }
            };
        }

        protected override void Initialize()
        {
            base.Initialize();

            if (Size.Height > 0)
            {
                OnSizeChanged(Size, new SizeF());
            }
        }

        protected override void OnSizeChanged(SizeF size, SizeF previousSize)
        {
            if (previousSize.Height == 0 && size.Height != 0)
            {
                // Make sure it renders after getting size
                MarkDirty();
            }

            IsCompactMode = size.Width < 670;
        }

        private View RenderSyncProgressBar()
        {
            return RenderSyncProgressBar(SyncState, UploadImageProgress, VerticalAlignment.Top);
        }

        public static View RenderSyncProgressBar(SyncStates SyncState, double UploadImageProgress, VerticalAlignment verticalAlignment)
        {
            return new ProgressBar
            {
                Opacity = SyncState == SyncStates.Done ? 0 : 1,
                Color = System.Drawing.Color.White,
                IsIndeterminate = SyncState == SyncStates.Syncing ? true : false,
                Value = SyncState == SyncStates.UploadingImages ? UploadImageProgress : 0,
                MaxValue = 1,
                VerticalAlignment = verticalAlignment
            };
        }

        private View RenderOfflineOrErrorsView(VerticalAlignment verticalAlignment = VerticalAlignment.Top)
        {
            return IsOffline || HasSyncErrors ? new TransparentContentButton
            {
                VerticalAlignment = verticalAlignment,
                Content = new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(6),
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = MaterialDesign.MaterialDesignIcons.Error,
                            FontSize = 16,
                            Color = System.Drawing.Color.White,
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = HasSyncErrors ? PowerPlannerResources.GetString("String_SyncError") : IsOffline ? PowerPlannerResources.GetString("String_Offline") : "",
                            WrapText = false,
                            TextColor = System.Drawing.Color.White,
                            FontSize = 12,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(6,0,0,0)
                        }.LinearLayoutWeight(1)
                    }
                },
                Click = () =>
                {
                    if (HasSyncErrors)
                    {
                        ViewSyncErrors();
                    }
                    else
                    {
                        this.SyncCurrentAccount();
                    }
                }
            } : null;
        }

        private View RenderSidebar()
        {
            var menuItems = RenderMenuItems();

            return new FrameLayout
            {
                BackgroundColor = Theme.Current.ChromeColor,
                Width = 215,
                VerticalAlignment = VerticalAlignment.Stretch,
                Children =
                {
                    new LinearLayout
                    {
                        Orientation = Orientation.Vertical,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Children =
                        {
                            new ImageView
                            {
                                Source = new UriImageSource
                                {
                                    UwpUri = "ms-appx:///Assets/Logo.png"
                                },
                                Width = 58,
                                Margin = new Thickness(0, 24, 0, 24),
                                Tapped = SyncCurrentAccount
                            },

                            new ScrollView
                            {
                                Content = menuItems
                            }.LinearLayoutWeight(1),

                            RenderSettingsButton()
                        }
                    },

                    RenderSyncProgressBar(),

                    RenderOfflineOrErrorsView(),
                }
            };
        }

        private View RenderSettingsButton()
        {
            return new TransparentContentButton
            {
                Content = new Border
                {
                    BackgroundColor = SelectedItem == MainMenuSelections.Settings ? Theme.Current.AccentColor : System.Drawing.Color.Transparent,
                    Content = new TextBlock
                    {
                        Text = PowerPlannerResources.GetString("MainMenuItem_Settings"),
                        TextColor = System.Drawing.Color.White,
                        FontSize = 16,
                        Margin = new Thickness(20, 12, 0, 20),
                        FontWeight = FontWeights.SemiLight,
                        WrapText = false,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                },
                Click = () =>
                {
                    SelectedItem = MainMenuSelections.Settings;
                }
            };
        }

        public static string MainMenuItemToString(MainMenuSelections value)
        {
            switch (value)
            {
                case MainMenuSelections.Agenda:
                    return PowerPlannerResources.GetString("MainMenuItem_Agenda");

                case MainMenuSelections.Calendar:
                    return PowerPlannerResources.GetString("MainMenuItem_Calendar");

                case MainMenuSelections.Classes:
                    return PowerPlannerResources.GetString("MainMenuItem_Classes");

                case MainMenuSelections.Day:
                    return PowerPlannerResources.GetString("MainMenuItem_Day");

                case MainMenuSelections.Schedule:
                    return PowerPlannerResources.GetString("MainMenuItem_Schedule");

                case MainMenuSelections.Settings:
                    return PowerPlannerResources.GetString("MainMenuItem_Settings");

                case MainMenuSelections.Years:
                    return PowerPlannerResources.GetString("MainMenuItem_Years");

                default:
                    throw new NotImplementedException("Unknown MainMenuSelections enum value");
            }
        }

        private View RenderMenuItems()
        {
            var menuItems = new LinearLayout
            {
                Orientation = Orientation.Vertical
            };
            foreach (var i in AvailableItems)
            {
                if (i == MainMenuSelections.Settings)
                {
                    continue;
                }

                var tb = new TextBlock
                {
                    Text = MainMenuItemToString(i),
                    TextColor = System.Drawing.Color.White,
                    Margin = new Thickness(20, 8, 0, 8),
                    FontSize = 20,
                    FontWeight = FontWeights.SemiLight,
                    WrapText = false
                };

                menuItems.Children.Add(new Border
                {
                    Content = new TransparentContentButton
                    {
                        Content = i == MainMenuSelections.Classes ? (View)new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                tb.LinearLayoutWeight(1),
                                SelectedItem == MainMenuSelections.Classes ? new TransparentContentButton
                                {
                                    Content = new FontIcon
                                    {
                                        Glyph = MaterialDesign.MaterialDesignIcons.Add,
                                        FontSize = 16,
                                        Color = System.Drawing.Color.White,
                                        Margin = new Thickness(12,0,12,0)
                                    },
                                    Click = () =>
                                    {
                                        AddClass(navigateToClassAfterAdd: true);
                                    },
                                    TooltipText = R.S("SchedulePage_ButtonAddClass.Content")
                                } : null
                            }
                        } : (View)tb,
                        Click = () =>
                        {
                            SelectedItem = i;
                        }
                    },
                    BackgroundColor = SelectedItem == i ? Theme.Current.AccentColor : System.Drawing.Color.Transparent
                });

                if (i == MainMenuSelections.Classes && SelectedItem == MainMenuSelections.Classes)
                {
                    foreach (var c in Classes)
                    {
                        menuItems.Children.Add(new Border
                        {
                            Content = new TransparentContentButton
                            {
                                Content = new LinearLayout
                                {
                                    Orientation = Orientation.Horizontal,
                                    Margin = new Thickness(24, 10, 0, 10),
                                    Children =
                                    {
                                        new Border
                                        {
                                            BorderColor = System.Drawing.Color.Black,
                                            BorderThickness = new Thickness(1),
                                            BackgroundColor = ColorBytesHelper.ToColor(c.Color),
                                            Width = 14,
                                            Height = 14,
                                            VerticalAlignment = VerticalAlignment.Center
                                        },
                                        new TextBlock
                                        {
                                            Text = c.Name,
                                            TextColor = System.Drawing.Color.White,
                                            FontSize = 14,
                                            FontWeight = FontWeights.SemiLight,
                                            Margin = new Thickness(12,0,0,0),
                                            WrapText = false,
                                            VerticalAlignment = VerticalAlignment.Center
                                        }
                                    }
                                },
                                Click = () =>
                                {
                                    SelectClassWithinSemester(c);
                                }
                            },
                            BackgroundColor = SelectedClass == c ? System.Drawing.Color.FromArgb(65, 167, 240) : Theme.Current.AccentColor
                        });
                    }
                }
            }

            return menuItems;
        }

        private bool _isCompactMode = false;
        public bool IsCompactMode
        {
            get => _isCompactMode;
            set
            {
                if (_isCompactMode != value)
                {
                    SetProperty(ref _isCompactMode, value, nameof(IsCompactMode));
                }
            }
        }

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
        public async Task SetCurrentSemester(Guid semesterId, bool alwaysNavigate = false, bool closeYearsPopup = true)
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

                    if (PowerPlannerApp.DoNotShowYearsInTabItems && closeYearsPopup)
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

            if (PowerPlannerApp.DoNotShowYearsInTabItems && AvailableItems.Any() && closeYearsPopup)
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

            // On iOS we need to refresh the widgets on launch
            if (VxPlatform.Current == Platform.iOS && account != null)
            {
                UpdateTileNotifications(account);
            }
        }

        private async void UpdateTileNotifications(AccountDataItem account)
        {
            try {
                var data = await AccountDataStore.Get(account.LocalAccountId);
                TilesExtension.Current?.UpdateTileNotificationsForAccountAsync(account, data);
            }
            catch {}
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

#if DEBUG
            if (VxTestingGroundViewModel.ShowTestingGround)
            {
                selectedItem = MainMenuSelections.Settings;
            }
#endif

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
                if (PowerPlannerApp.ShowClassesAsPopups)
                {
                    ViewClass(c);
                }
                else
                {
                    SetContent(new ClassViewModel(this, CurrentLocalAccountId, classId, DateTime.Today, CurrentSemester), preserveBack: allowGoingBack);
                }
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

        public void DuplicateAndEditTaskOrEvent(ViewItemTaskOrEvent item)
        {
            ShowPopup(AddTaskOrEventViewModel.CreateForClone(this, new AddTaskOrEventViewModel.CloneParameter()
            {
                Item = item,
                Classes = Classes.ToArray()
            }));
        }

        /// /// <summary>
        /// Duplicates a Task or Event
        /// </summary>
        /// <param name="item">The task or event being duplicated</param>
        /// <param name="date">The date the task or event should be copied to. Defaults to same date as item</param>
        public void DuplicateTaskOrEvent(ViewItemTaskOrEvent item, DateTime? date = null)
        {
            DataChanges changes = new DataChanges();
            DateTime now = DateTime.UtcNow;

            DataItemMegaItem copiedDataItem = (DataItemMegaItem)item.DataItem.Clone();
            copiedDataItem.Identifier = Guid.NewGuid(); // Create new Guid
            copiedDataItem.DateCreated = now;   // The copy was created now
            copiedDataItem.Updated = now;       // The copy was created now
            copiedDataItem.Date = date ?? copiedDataItem.Date;  // If date is defined, set it

            changes.Add(copiedDataItem);

            PowerPlannerApp.Current.SaveChanges(changes);
        }

        public async void ConvertTaskOrEventType(ViewItemTaskOrEvent item, BaseViewModel viewModel = null)
        {
            await BaseMainScreenViewModelDescendant.TryHandleUserInteractionAsync(viewModel ?? this, "ChangeItemType", async (cancellationToken) =>
            {
                DataItemMegaItem dataItem = new DataItemMegaItem()
                {
                    Identifier = item.Identifier
                };

                PowerPlannerSending.MegaItemType newMegaItemType;

                switch ((item.DataItem as DataItemMegaItem).MegaItemType)
                {
                    case PowerPlannerSending.MegaItemType.Task:
                        newMegaItemType = PowerPlannerSending.MegaItemType.Event;
                        break;

                    case PowerPlannerSending.MegaItemType.Homework:
                        newMegaItemType = PowerPlannerSending.MegaItemType.Exam;
                        break;

                    case PowerPlannerSending.MegaItemType.Event:
                        newMegaItemType = PowerPlannerSending.MegaItemType.Task;
                        break;

                    case PowerPlannerSending.MegaItemType.Exam:
                        newMegaItemType = PowerPlannerSending.MegaItemType.Homework;
                        break;

                    default:
                        throw new NotImplementedException();
                }

                dataItem.MegaItemType = newMegaItemType;

                DataChanges editChanges = new DataChanges();
                editChanges.Add(dataItem);
                await PowerPlannerApp.Current.SaveChanges(editChanges);

                TelemetryExtension.Current?.TrackEvent("ConvertedItemType");

            }, "Failed to change item type. Your error has been reported.");
        }

        public void SetTaskPercentComplete(ViewItemTaskOrEvent task, double percentComplete)
        {
            if (task == null || task.PercentComplete == percentComplete || task.Type != TaskOrEventType.Task)
            {
                return;
            }

            BaseMainScreenViewModelDescendant.TryStartDataOperationAndThenNavigate(delegate
            {
                DataChanges changes = new DataChanges();

                changes.Add(new DataItemMegaItem()
                {
                    Identifier = task.Identifier,
                    PercentComplete = percentComplete
                });

                return PowerPlannerApp.Current.SaveChanges(changes);

            }, delegate
            {
                if (percentComplete == 1)
                {
                    if (CurrentAccount.IsSoundEffectsEnabled)
                    {
                        SoundsExtension.Current?.TryPlayTaskCompletedSound();
                    }

                    try
                    {
                        // Don't prompt for non-class tasks
                        if (!task.Class.IsNoClassClass)
                        {
                            BareSnackbar.Make(PowerPlannerResources.GetString("String_TaskCompleted"), PowerPlannerResources.GetString("String_AddGrade"), delegate { AddGradeAfterCompletingTask(task); }).Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                }
            });
        }

        private async void AddGradeAfterCompletingTask(ViewItemTaskOrEvent task)
        {
            try
            {
                TelemetryExtension.Current?.TrackEvent("ClickedSnackbarAddGrade");

                // We need to load the class with the weight categories
                var ViewItemsGroupClass = await ClassViewItemsGroup.LoadAsync(CurrentLocalAccountId, task.Class.Identifier, DateTime.Today, CurrentSemester);

                ViewItemsGroupClass.LoadTasksAndEvents();
                ViewItemsGroupClass.LoadGrades();
                await ViewItemsGroupClass.LoadTasksAndEventsTask;
                await ViewItemsGroupClass.LoadGradesTask;

                var loadedTask = ViewItemsGroupClass.Tasks.FirstOrDefault(i => i.Identifier == task.Identifier);
                if (loadedTask == null)
                {
                    ViewItemsGroupClass.ShowPastCompletedTasks();
                    await ViewItemsGroupClass.LoadPastCompleteTasksAndEventsTask;

                    loadedTask = ViewItemsGroupClass.PastCompletedTasks.FirstOrDefault(i => i.Identifier == task.Identifier);
                    if (loadedTask == null)
                    {
                        return;
                    }
                }

                var viewModel = ViewTaskOrEventViewModel.CreateForUnassigned(this, loadedTask);
                viewModel.AddGrade(showViewGradeSnackbarAfterSaving: SelectedItem != NavigationManager.MainMenuSelections.Classes); // Don't show view grades when already on class page
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
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

        /// <summary>
        /// Only should be called from devices that don't show Years in the tab items (Android/iOS)
        /// </summary>
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

        /// <summary>
        /// Works from all devices
        /// </summary>
        public void OpenOrShowYears()
        {
            if (PowerPlannerApp.DoNotShowYearsInTabItems)
            {
                OpenYears();
            }
            else
            {
                SelectedItem = MainMenuSelections.Years;
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
