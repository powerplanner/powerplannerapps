using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Promos;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.Storage;

namespace PowerPlannerUWP.ViewModel.Promos
{
    public class PromoOtherPlatformsViewModel : BaseViewModel
    {
        public class Registration : PromoRegistration
        {
            public override BaseViewModel Create(AccountDataItem account, BaseViewModel parent)
            {
                return new PromoOtherPlatformsViewModel(parent);
            }

            public override void MarkShown(AccountDataItem account)
            {
                ApplicationData.Current.RoamingSettings.Values[SETTING_HAS_PROMOTED_ANDROID_AND_IOS] = true;
            }

            public override async Task<bool> ShouldShowAsync(AccountDataItem account)
            {
                // Don't show for offline accounts or for devices that aren't Desktop
                if (account == null || !account.IsOnlineAccount || InterfacesUWP.DeviceInfo.DeviceFamily != InterfacesUWP.DeviceFamily.Desktop)
                {
                    return false;
                }

                if (ApplicationData.Current.RoamingSettings.Values.ContainsKey(SETTING_HAS_PROMOTED_ANDROID_AND_IOS))
                {
                    return false;
                }

                var dataStore = await AccountDataStore.Get(account.LocalAccountId);

                // If they actually have some classes
                bool hasContent;
                using (await Locks.LockDataForReadAsync())
                {
                    hasContent = dataStore.TableClasses.Count() > 1;
                }

                if (hasContent)
                {
                    // Try downloading and then show
                    ShouldSuggestOtherPlatformsResponse response = await WebHelper.Download<ShouldSuggestOtherPlatformsRequest, ShouldSuggestOtherPlatformsResponse>(
                    Website.URL + "shouldsuggestotherplatforms",
                    new ShouldSuggestOtherPlatformsRequest()
                    {
                        Login = account.GenerateCredentials(),
                        CurrentPlatform = "Windows 10"
                    }, Website.ApiKey);

                    if (response.ShouldSuggest)
                    {
                        return true;
                    }

                    // No need to suggest in the future nor show now
                    MarkShown(account);
                    return false;
                }
                else
                {
                    // Not enough content to show right now
                    return false;
                }
            }
        }

        private static bool _hasBeenRequestedThisSession;
        private const string SETTING_HAS_PROMOTED_ANDROID_AND_IOS = "HasPromotedAndroidAndiOS";

        private PromoOtherPlatformsViewModel(BaseViewModel parent) : base(parent)
        {
        }

        public static async void ShowIfNeeded(AccountDataItem account, MainWindowViewModel mainViewModel)
        {
            if (_hasBeenRequestedThisSession)
            {
                return;
            }
            _hasBeenRequestedThisSession = true;

            try
            {
                
            }
            catch { }
        }
    }
}
