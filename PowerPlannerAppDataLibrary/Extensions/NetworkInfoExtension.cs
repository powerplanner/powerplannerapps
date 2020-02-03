using PowerPlannerAppDataLibrary.Extensions.NetworkInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class NetworkInfoExtension
    {
        public static NetworkInfoExtension Current { get; set; }

        public abstract NetworkCostType GetNetworkCostType();
    }

    namespace NetworkInfo
    {
        public enum NetworkCostType
        {
            Unlimited,
            Limited,
            Disconnected
        }
    }
}
