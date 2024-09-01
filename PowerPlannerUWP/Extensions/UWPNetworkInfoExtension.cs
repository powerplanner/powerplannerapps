using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace PowerPlannerUWP.Extensions
{
    public class UWPNetworkInfoExtension : NetworkInfoExtension
    {
        public override PowerPlannerAppDataLibrary.Extensions.NetworkInfo.NetworkCostType GetNetworkCostType()
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();
            if (profile == null)
                return PowerPlannerAppDataLibrary.Extensions.NetworkInfo.NetworkCostType.Disconnected;
            
            var cost = profile.GetConnectionCost();

            if (cost == null)
                return PowerPlannerAppDataLibrary.Extensions.NetworkInfo.NetworkCostType.Limited;

            if (cost.NetworkCostType == NetworkCostType.Unrestricted)
            {
                return PowerPlannerAppDataLibrary.Extensions.NetworkInfo.NetworkCostType.Unlimited;
            }

            return PowerPlannerAppDataLibrary.Extensions.NetworkInfo.NetworkCostType.Limited;
        }
    }
}
