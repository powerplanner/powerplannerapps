using PowerPlannerApp.App;
using PowerPlannerApp.DataLayer;
using PowerPlannerApp.Extensions;
using PowerPlannerApp.SyncLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Vx.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PowerPlannerApp.Pages.WelcomePages
{
    public class InitialSyncPage : VxPage
    {
        public readonly AccountDataItem Account;
        private EventHandler<SyncQueuedEventArgs> _syncQueuedEventHandler;

        private VxState<bool> _isSyncing = new VxState<bool>(true);
        private VxState<string> _error = new VxState<string>(null);

        public InitialSyncPage(AccountDataItem account) : base()
        {
            Account = account;
        }

        protected override void Initialize()
        {
            base.Initialize();

            _syncQueuedEventHandler = new WeakEventHandler<SyncQueuedEventArgs>(Sync_SyncQueued).Handler;
            Sync.SyncQueued += _syncQueuedEventHandler;

            Sync.StartSyncAccount(Account);

            try
            {
                // Perfect time to ask for permission to send notifications
                RemindersExtension.Current?.RequestReminderPermission();
            }
            catch { }
        }

        protected override View Render()
        {
            return new Grid
            {
                BackgroundColor = PowerPlannerColors.PowerPlannerBlue,
                Children =
                {
                    new ScrollView
                    {
                        Content = new StackLayout
                        {
                            Margin = new Thickness(24),
                            VerticalOptions = LayoutOptions.Center,
                            Children =
                            {
                                new Image
                                {
                                    Source = new FileImageSource() { File = "Assets/Logo.png" },
                                    Aspect = Aspect.AspectFit,
                                    WidthRequest = 80
                                },

                                new ProgressBar
                                {
                                    ProgressColor = Color.White,
                                    WidthRequest = 40,
                                    HeightRequest = 40,
                                    Margin = new Thickness(0, 24, 0, 0),
                                    IsVisible = _isSyncing.Value
                                },

                                new Label
                                {
                                    Text = _isSyncing.Value ? PowerPlannerResources.GetString("LoginPage_String_SyncingAccount") : PowerPlannerResources.GetString("String_SyncError"),
                                    TextColor = Color.White,
                                    HorizontalTextAlignment = TextAlignment.Center,
                                    Margin = new Thickness(0, 12, 0, 0)
                                },

                                new Button
                                {
                                    Text = "Try again",
                                    Command = CreateCommand(TryAgain),
                                    HorizontalOptions = LayoutOptions.Center,
                                    MinimumWidthRequest = 120,
                                    Margin = new Thickness(0, 24, 0, 0),
                                    IsVisible = !_isSyncing.Value
                                },

                                new Label
                                {
                                    Text = _error.Value,
                                    TextColor = Color.White,
                                    Margin = new Thickness(0, 24, 0, 0),
                                    IsVisible = _error.Value != null
                                }
                            }
                        }
                    },

                    new Button
                    {
                        Text = "Settings",
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.Start,
                        IsVisible = !_isSyncing.Value
                    }
                }
            };
        }

        public void TryAgain()
        {
            _isSyncing.Value = true;
            _error.Value = null;

            Sync.StartSyncAccount(Account);
        }

        public void OpenSettings(bool asPopup = false)
        {
            // TODO
            //var mainWindowViewModel = this.FindAncestor<MainWindowViewModel>();

            //if (asPopup)
            //{
            //    mainWindowViewModel.ShowPopup(new SettingsViewModel(mainWindowViewModel));
            //}
            //else
            //{
            //    mainWindowViewModel.Navigate(new SettingsViewModel(mainWindowViewModel));
            //}
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
                await MainThread.InvokeOnMainThreadAsync(async delegate
                {
                    try
                    {
                        if (Account.LocalAccountId == e.Account.LocalAccountId)
                        {
                            _isSyncing.Value = true;

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
                                _isSyncing.Value = false;
                                return;
                            }

                            else if (result.Error != null)
                            {
                                _isSyncing.Value = false;

                                TelemetryExtension.Current?.TrackEvent("FailedInitialSync", new Dictionary<string, string>()
                                {
                                    { "Error", result.Error }
                                });

                                if (result.Error.Equals("Offline."))
                                {
                                    _error.Value = PowerPlannerResources.GetString("String_OfflineExplanation");
                                }

                                else if (result.Error.Equals(PowerPlannerSending.SyncResponse.NO_ACCOUNT))
                                {
                                    _error.Value = "Your online account was deleted and no longer exists.";
                                }

                                else
                                {
                                    _error.Value = result.Error;
                                }

                                if (result.Error.Equals(PowerPlannerSending.SyncResponse.INCORRECT_PASSWORD) || result.Error.Equals(PowerPlannerSending.SyncResponse.USERNAME_CHANGED) || result.Error.Equals(PowerPlannerSending.SyncResponse.DEVICE_NOT_FOUND))
                                {
                                    //ShowPopupUpdateCredentials(Account);
                                }

                                else if (result.Error.Equals(PowerPlannerSending.SyncResponse.DEVICE_NOT_FOUND) || result.Error.Equals(PowerPlannerSending.SyncResponse.NO_DEVICE))
                                {
                                    //ShowPopupUpdateCredentials(Account, UpdateCredentialsViewModel.UpdateTypes.NoDevice);
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
                                PowerPlannerVxApp.Current.MarkInitialSyncCompleted();

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
                        _error.Value = ex.ToString();
                        _isSyncing.Value = false;
                    }
                });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                _error.Value = ex.ToString();
                _isSyncing.Value = false;
            }
        }

        //private void ShowPopupUpdateCredentials(AccountDataItem account, UpdateCredentialsViewModel.UpdateTypes updateType = UpdateCredentialsViewModel.UpdateTypes.Normal)
        //{
        //    ShowPopup(UpdateCredentialsViewModel.Create(this, account, updateType));
        //}
    }
}
