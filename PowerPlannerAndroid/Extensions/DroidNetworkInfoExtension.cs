using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAndroid.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Extensions.NetworkInfo;

namespace PowerPlannerAndroid.Extensions
{
    public class DroidNetworkInfoExtension : NetworkInfoExtension
    {
        public override NetworkCostType GetNetworkCostType()
        {
            try
            {
                ConnectivityManager connMgr = (ConnectivityManager)NativeApplication.Current.ApplicationContext.GetSystemService(Context.ConnectivityService);

                if (connMgr != null)
                {
                    Network network = connMgr.ActiveNetwork;
                    if (network != null)
                    {
                        NetworkCapabilities capabilities = connMgr.GetNetworkCapabilities(network);
                        if (capabilities.HasCapability(NetCapability.NotMetered) ||
                            (OperatingSystem.IsAndroidVersionAtLeast(30) && capabilities.HasCapability(NetCapability.TemporarilyNotMetered)))
                        {
                            return NetworkCostType.Unlimited;
                        }

                        return NetworkCostType.Limited;
                    }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            return NetworkCostType.Disconnected;
        }
    }
}