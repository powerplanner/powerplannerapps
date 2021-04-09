using PowerPlannerApp.DataLayer;
using PowerPlannerApp.Extensions;
using PowerPlannerApp.Pages;
using PowerPlannerApp.Pages.WelcomePages;
using PowerPlannerApp.SyncLayer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using Xamarin.Forms;

namespace PowerPlannerApp.App
{
    public class PowerPlannerVxApp : VxApp
    {
        public static PowerPlannerVxApp Current { get; private set; }

        private VxState<bool> _isLoaded = new VxState<bool>(false);
        private VxState<AccountDataItem> _currentAccount = new VxState<AccountDataItem>(null);

        public PowerPlannerVxApp()
        {
            Current = this;
        }

        protected override async void Initialize()
        {
            base.Initialize();

            // Restore previous login
            AccountDataItem lastAccount = await AccountsManager.GetLastLogin();

            if (lastAccount == null && (await AccountsManager.GetAllAccounts()).Count == 0)
            {
                // If no accounts, we create the default account
                try
                {
                    var account = await AccountsManager.CreateAndInitializeAccountAsync(AccountsManager.DefaultOfflineAccountUsername, "", null, 0, 0);

                    if (account != null)
                    {
                        lastAccount = account;
                        TelemetryExtension.Current?.TrackEvent("CreatedDefaultOfflineAccount");
                    }
                    else
                    {
                        TelemetryExtension.Current?.TrackException(new Exception("Tried creating default offline account, but it returned null"));
                    }
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }

            if (lastAccount != null && lastAccount.IsAutoLoginPossible && lastAccount.AutoLogin)
            {
                await this.SetCurrentAccount(lastAccount);
                //PromoRegistrations.StartPromoLogic(lastAccount);
                //LoggedInFromNormalActivation?.Invoke(this, lastAccount);
            }

            else
                await this.SetCurrentAccount(null);

            _isLoaded.Value = true;
        }

        public AccountDataItem GetCurrentAccount()
        {
            return _currentAccount.Value;
        }

        public async Task SetCurrentAccount(AccountDataItem account, bool syncAccount = true)
        {
            ClearPopups();

            _currentAccount.Value = account;

            if (account != null && !account.NeedsInitialSync && syncAccount)
            {
                Sync.StartSyncAccount(account);
            }
        }

        public void MarkInitialSyncCompleted()
        {
            MarkDirty();
        }

        protected override View Render()
        {
            if (!_isLoaded.Value)
            {
                return null;
            }

            if (_currentAccount.Value == null)
            {
                return new WelcomePage();
            }

            if (_currentAccount.Value.NeedsInitialSync)
            {
                return new InitialSyncPage(_currentAccount.Value);
            }
            else
            {
                return new MainScreenPage();
            }
        }

        public Task SaveChanges(DataChanges changes)
        {
            return PowerPlannerCoreApp.SaveChanges(_currentAccount.Value, changes);
        }
    }
}
