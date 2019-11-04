using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.ApplicationModel.Store;

namespace PowerPlannerUWPLibrary.Extensions
{
    public class UWPInAppPurchaseExtension : InAppPurchaseExtension
    {
        public override Task<bool> OwnsInAppPurchaseAsync()
        {
#if DEBUG
            return Task.FromResult(true);
#endif

            try
            {
                var licenseInformation = CurrentApp.LicenseInformation;
                return Task.FromResult(licenseInformation.ProductLicenses[Variables.FULL_VERSION_TOKEN].IsActive || licenseInformation.ProductLicenses[Variables.FULL_VERSION_TOKEN_FROM_WP].IsActive);
            }

            catch (Exception ex)
            {
                if (ExceptionHelper.IsHResult(ex, 0x803F6107))
                {
                    TelemetryExtension.Current?.TrackEvent("Error_IapLicense_0x803F6107");
                }
                else
                {
                    try
                    {
                        if (!ex.Message.Equals("The object invoked has disconnected from its clients.") && !ex.Message.Contains("The RPC server is unavailable"))
                            TelemetryExtension.Current?.TrackException(ex);
                    }

                    catch { }
                }

                return Task.FromResult(false);
            }
        }

        public override async Task<bool> PromptPurchase()
        {
            try
            {
                var purchaseResults = await CurrentApp.RequestProductPurchaseAsync(Variables.FULL_VERSION_TOKEN);

                // If they just purchased, we'll upload the purchase status to their account
                if (purchaseResults.Status == ProductPurchaseStatus.Succeeded)
                {
                    var currAccount = PowerPlannerApp.Current.GetCurrentAccount();

                    if (currAccount != null)
                    {
                        var dontBlock = Sync.SetAsPremiumAccount(currAccount);
                    }
                }

                return purchaseResults.Status == ProductPurchaseStatus.AlreadyPurchased || purchaseResults.Status == ProductPurchaseStatus.Succeeded;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return false;
            }
        }
    }
}
