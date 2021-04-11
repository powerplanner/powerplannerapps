using PowerPlannerApp.DataLayer;
using PowerPlannerApp.Extensions;
using PowerPlannerApp.Pages;
using PowerPlannerApp.Pages.WelcomePages;
using PowerPlannerApp.SyncLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
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



        private bool _ownsInAppPurchase;

        /// <summary>
        /// Use IsFullVersionAsync to check either whether the account is premium or the in app purchase is owned.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> OwnsInAppPurchaseAsync()
        {
            if (_ownsInAppPurchase)
            {
                return true;
            }

            try
            {
                if (InAppPurchaseExtension.Current != null)
                {
                    _ownsInAppPurchase = await InAppPurchaseExtension.Current.OwnsInAppPurchaseAsync();
                    return _ownsInAppPurchase;
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            return false;
        }

        public async Task<bool> IsFullVersionAsync()
        {
#if DEBUG
            await System.Threading.Tasks.Task.Delay(800);
            return true;
#endif

            // Ignore unreachable code warning
#pragma warning disable 0162
            try
            {
                var currAccount = GetCurrentAccount();
                if (currAccount != null)
                {
                    if (currAccount.PremiumAccountExpiresOn > DateTime.UtcNow)
                    {
                        return true;
                    }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            return await OwnsInAppPurchaseAsync();

#pragma warning restore 0162
        }

        public async void PromptPurchase(string contextualMessage)
        {
            try
            {
                //ShowPopup((parent) => new PremiumVersionViewModel(parent, contextualMessage));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);

                try
                {
                    await new PortableMessageDialog("Failed to open upgrade to premium page. Your error has been reported to the developer.", "Error").ShowAsync();
                }
                catch { }
            }
        }



        public static async void TryStartDataOperationAndThenNavigate(Func<Task> dataOperation, Action navigateOperation, [CallerMemberName] string callerFunctionName = "", [CallerFilePath] string callerFilePath = "")
        {
            try
            {
                var task = dataOperation();

                navigateOperation();

                DateTime start = DateTime.Now;

                await task;

                TimeSpan duration = DateTime.Now - start;
                if (duration.TotalSeconds > 1.0)
                {
                    // If these are taking longer than a second, that gives us an idea that we need to
                    // implement UI to show that the data operation is occurring
                    TelemetryExtension.Current?.TrackEvent("SlowUIDataOperation", new Dictionary<string, string>()
                    {
                        { "CallerFilePath", callerFilePath.Split('\\').LastOrDefault() ?? "" },
                        { "Duration", duration.TotalSeconds.ToString("0.0") },
                        { "CallerFunction", callerFunctionName }
                    });
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);

                try
                {
                    await new PortableMessageDialog("Failed to save changes. Your error has been reported to the developer.", "Error").ShowAsync();
                }
                catch { }
            }
        }
    }
}
