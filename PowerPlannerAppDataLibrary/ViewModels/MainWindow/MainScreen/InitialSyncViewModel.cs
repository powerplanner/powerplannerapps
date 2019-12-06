using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen
{
    public class InitialSyncViewModel : PagedViewModelWithPopups
    {
        public readonly AccountDataItem Account;

        public InitialSyncViewModel(BaseViewModel parent, AccountDataItem account) : base(parent)
        {
            Account = account;

            Sync.SyncQueued += new WeakEventHandler<SyncQueuedEventArgs>(Sync_SyncQueued).Handler;

            Sync.StartSyncAccount(account);
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

                            // If this is still the task we're considering for intermediate, then we clear intermediate (if another was queued, it wouldn't match task)
                            if (_currSyncResultTask == e.ResultTask)
                            {
                                IsSyncing = false;
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
                                    return;
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
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                        Error = ex.ToString();
                    }
                });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                Error = ex.ToString();
            }
        }

        private void ShowPopupUpdateCredentials(AccountDataItem account, UpdateCredentialsViewModel.UpdateTypes updateType = UpdateCredentialsViewModel.UpdateTypes.Normal)
        {
            ShowPopup(UpdateCredentialsViewModel.Create(this, account, updateType));
        }
    }
}
