using Network;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Extensions.NetworkInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlanneriOS.Extensions
{
    public class iOSNetworkInfoExtension : NetworkInfoExtension
    {
        public override NetworkCostType GetNetworkCostType()
        {
#if DEBUG
            // The simulator doesn't seem to support NWPathMonitor so I'll just return unlimited
            return NetworkCostType.Unlimited;
#else
            try
            {
                using (var monitor = new NWPathMonitor())
                {
                    var path = monitor.CurrentPath;

                    // Check if the network is reachable
                    if (path != null && path.Status == NWPathStatus.Satisfied)
                    {
                        // Check for Low Data Mode
                        if (path.IsConstrained)
                        {
                            return NetworkCostType.Limited;
                        }

                        return NetworkCostType.Unlimited;
                    }
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            // Return disconnected if no network is reachable
            return NetworkCostType.Disconnected;
#endif
        }
    }
}
