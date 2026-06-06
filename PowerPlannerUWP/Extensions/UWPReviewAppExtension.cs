using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Services.Store;
using Windows.System;

namespace PowerPlannerUWP.Extensions
{
    internal class UWPReviewAppExtension : ReviewAppExtension
    {
        public override async Task ReviewAppAsync()
        {
            try
            {
                if (ApiInformation.IsMethodPresent("Windows.Services.Store.StoreContext", "RequestRateAndReviewAppAsync"))
                {
                    var storeContext = StoreContext.GetDefault();
                    var result = await storeContext.RequestRateAndReviewAppAsync();

                    // If showing dialog succeeded
                    if (result.Status != StoreRateAndReviewStatus.NetworkError
                        && result.Status != StoreRateAndReviewStatus.Error)
                    {
                        // We don't want exceptions parsing here to cause fallback behavior
                        try
                        {
                            var props = new Dictionary<string, string>()
                            {
                                { "Status", result.Status.ToString() }
                            };
                            if (result.Status == StoreRateAndReviewStatus.Succeeded)
                            {
                                bool updated = result.WasUpdated;
                                props.Add("Updated", updated.ToString());
                            }

                            TelemetryExtension.Current?.TrackEvent("ReviewAppResponse", props);
                        }
                        catch (Exception ex)
                        {
                            TelemetryExtension.Current?.TrackException(ex);
                        }

                        // We don't continue falling back at all
                        return;
                    }

                    TelemetryExtension.Current?.TrackException(result.ExtendedError);
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                // Fall back to normal
            }

            await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9wzdncrfj25v"));
        }
    }
}
