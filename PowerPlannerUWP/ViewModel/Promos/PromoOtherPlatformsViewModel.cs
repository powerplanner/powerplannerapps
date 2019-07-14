using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow;
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
                // Don't show for offline accounts or for devices that aren't Desktop
                if (account == null || !account.IsOnlineAccount || InterfacesUWP.DeviceInfo.DeviceFamily != InterfacesUWP.DeviceFamily.Desktop)
                {
                    return;
                }

                if (ApplicationData.Current.RoamingSettings.Values.ContainsKey(SETTING_HAS_PROMOTED_ANDROID_AND_IOS))
                {
                    return;
                }

                // Postpone operations so that we don't affect launch speed
                await System.Threading.Tasks.Task.Delay(2000);

                var dataStore = await AccountDataStore.Get(account.LocalAccountId);

                // If they actually have some classes
                bool hasContent = await System.Threading.Tasks.Task.Run(async delegate
                {
                    using (await Locks.LockDataForReadAsync())
                    {
                        return dataStore.TableClasses.Count() > 1;
                    }
                });

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
                        mainViewModel.ShowPopup(new PromoOtherPlatformsViewModel(mainViewModel));
                    }

                    ApplicationData.Current.RoamingSettings.Values[SETTING_HAS_PROMOTED_ANDROID_AND_IOS] = true;
                }
            }
            catch { }
        }
    }
}
