using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public partial class InitialSyncViewModel : BaseViewModel
    {
        public readonly AccountDataItem Account;
        private EventHandler<SyncQueuedEventArgs> _syncQueuedEventHandler;

        public InitialSyncViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Account = account;

            _syncQueuedEventHandler = new WeakEventHandler<SyncQueuedEventArgs>(Sync_SyncQueued).Handler;
            Sync.SyncQueued += _syncQueuedEventHandler;

            Sync.StartSyncAccount(account);

            try
            {
                // Perfect time to ask for permission to send notifications
                RemindersExtension.Current?.RequestReminderPermission();
            }
            catch { }
        }

        protected override View Render()
        {
            var icon = new ImageView
            {
                Source = new UriImageSource
                {
                    UwpUri = Theme.Current.IsDarkTheme ? "ms-appx:///Assets/Logo.png" : "ms-appx:///Assets/LogoDark.png",
                    IosBundleName = Theme.Current.IsDarkTheme ? "PowerPlannerIcon" : "PowerPlannerIconDark",
                    AndroidResourceName = Theme.Current.IsDarkTheme ? "logo" : "logodark"
                },
                Height = 100,
                Width = 100,
                Margin = new Thickness(0, IsSyncing ? 0 : 24, 0, 0)
            };

            return new Border
            {
                BackgroundColor = Theme.Current.BackgroundColor,
                Content = new FrameLayout
                {
                    Children =
                    {
                        new LinearLayout
                        {
                            Children =
                            {
                                icon,

                                IsSyncing ? new ProgressBar
                                {
                                    IsIndeterminate = true,
                                    Margin = new Thickness(0, 12, 0, 0),
                                    Width = 100,
                                    Color = Theme.Current.ForegroundColor
                                } : null,

                                new TextBlock
                                {
                                    Text = IsSyncing ? R.S("LoginPage_String_SyncingAccount") : R.S("String_SyncError"),
                                    Margin = new Thickness(0, 6, 0, 0),
                                    FontSize = Theme.Current.SubtitleFontSize,
                                    HorizontalAlignment = HorizontalAlignment.Center
                                },

                                IsSyncing ? null : new AccentButton
                                {
                                    Text = "Try again",
                                    Click = TryAgain,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Margin = new Thickness(0, 6, 0, 0)
                                },

                                IsSyncing || string.IsNullOrWhiteSpace(Error) ? null : new ScrollView
                                {
                                    Content = new TextBlock
                                    {
                                        Text = Error,
                                        WrapText = true,
                                        IsTextSelectionEnabled = true
                                    },
                                    Height = 100,
                                    Margin = new Thickness(0, 12, 0, 0)
                                }
                            },
                            Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, 0, Theme.Current.PageMargin + NookInsets.Right, 0),
                            VerticalAlignment = VerticalAlignment.Center
                        },

                        IsSyncing ? null : new TransparentContentButton
                        {
                            Margin = new Thickness(Theme.Current.PageMargin - Theme.Current.SmallPageMargin).Combine(NookInsets),
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Click = () => OpenSettings(),
                            Content = new LinearLayout
                            {
                                Orientation = Orientation.Horizontal,
                                Margin = new Thickness(Theme.Current.SmallPageMargin),
                                Children =
                                {
                                    new FontIcon
                                    {
                                        Glyph = MaterialDesign.MaterialDesignIcons.Settings,
                                        FontSize = 20
                                    },

                                    new TextBlock
                                    {
                                        Text = R.S("MainMenuItem_Settings"),
                                        WrapText = false,
                                        VerticalAlignment = VerticalAlignment.Center,
                                        Margin = new Thickness(6, 0, 0, 0)
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public void TryAgain()
        {
            IsSyncing = true;
            Error = null;

            Sync.StartSyncAccount(Account);
        }

        public void OpenSettings()
        {
            ShowPopup(new MinimalSettingsViewModel(Parent, Account));
        }

        private bool _isSyncing = true;
        public bool IsSyncing
        {
            get => _isSyncing;
            set => SetProperty(ref _isSyncing, value, nameof(IsSyncing));
        }

        private string _error;
        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value, nameof(Error));
        }

        private Task<SyncResult> _currSyncResultTask;
        private async void Sync_SyncQueued(object sender, SyncQueuedEventArgs e)
        {
            // We already synced and left this page, we should be good
            if (!Account.NeedsInitialSync)
            {
                return;
            }

            try
            {
                await Dispatcher.RunOrFallbackToCurrentThreadAsync(async delegate
                {
                    try
                    {
                        if (Account.LocalAccountId == e.Account.LocalAccountId)
                        {
                            IsSyncing = true;

                            _currSyncResultTask = e.ResultTask;

                            SyncResult result;

                            try
                            {
                                result = await e.ResultTask;
                            }

                            catch (OperationCanceledException) { result = null; }

                            // If this is not the task we're considering for intermediate, then we ignore
                            if (_currSyncResultTask != e.ResultTask)
                            {
                                return;
                            }

                            // Canceled
                            if (result == null)
                            {
                                IsSyncing = false;
                                return;
                            }

                            else if (result.Error != null)
                            {
                                IsSyncing = false;

                                TelemetryExtension.Current?.TrackEvent("FailedInitialSync", new Dictionary<string, string>()
                                {
                                    { "Error", result.Error }
                                });

                                if (result.Error.Equals("Offline."))
                                {
                                    Error = PowerPlannerResources.GetString("String_OfflineExplanation");
                                }

                                else if (result.Error.Equals(PowerPlannerSending.SyncResponse.NO_ACCOUNT))
                                {
                                    Error = "Your online account was deleted and no longer exists.";
                                }

                                else
                                {
                                    Error = result.Error;
                                }

                                if (result.Error.Equals(PowerPlannerSending.SyncResponse.INCORRECT_PASSWORD) || result.Error.Equals(PowerPlannerSending.SyncResponse.USERNAME_CHANGED) || result.Error.Equals(PowerPlannerSending.SyncResponse.DEVICE_NOT_FOUND))
                                {
                                    ShowPopupUpdateCredentials(Account);
                                }

                                else if (result.Error.Equals(PowerPlannerSending.SyncResponse.DEVICE_NOT_FOUND) || result.Error.Equals(PowerPlannerSending.SyncResponse.NO_DEVICE))
                                {
                                    ShowPopupUpdateCredentials(Account, UpdateCredentialsViewModel.UpdateTypes.NoDevice);
                                }
                            }

                            else
                            {
                                Account.NeedsInitialSync = false;
                                await AccountsManager.Save(Account);

                                // All good!
                                if (result.SelectedSemesterId != null && result.SelectedSemesterId.Value != Guid.Empty)
                                {
                                    await Account.SetCurrentSemesterAsync(result.SelectedSemesterId.Value, uploadSettings: false);
                                }

                                // Navigate to main 
                                var mainWindow = FindAncestor<MainWindowViewModel>();
                                mainWindow.Replace(await MainScreenViewModel.LoadAsync(mainWindow, Account, syncAccount: false));

                                // No need to set IsSyncing to false since we replaced this view

                                // Remove the event handler
                                Sync.SyncQueued -= _syncQueuedEventHandler;

                                TelemetryExtension.Current?.TrackEvent("SuccessfullyFinishedInitialSync");
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                        Error = ex.ToString();
                        IsSyncing = false;
                    }
                });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                Error = ex.ToString();
                IsSyncing = false;
            }
        }

        private void ShowPopupUpdateCredentials(AccountDataItem account, UpdateCredentialsViewModel.UpdateTypes updateType = UpdateCredentialsViewModel.UpdateTypes.Normal)
        {
            ShowPopup(UpdateCredentialsViewModel.Create(this, account, updateType));
        }
    }
}
