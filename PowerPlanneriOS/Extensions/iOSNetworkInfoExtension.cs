using CoreFoundation;
using Network;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Extensions.NetworkInfo;

namespace PowerPlanneriOS.Extensions
{
    public class iOSNetworkInfoExtension : NetworkInfoExtension
    {
        private NetworkCostType _costType = NetworkCostType.Disconnected;
        private NWPathMonitor _monitor;
        public iOSNetworkInfoExtension()
        {
            _monitor = new NWPathMonitor();

            var queue = new DispatchQueue("NetworkMonitorQueue");
            _monitor.SetQueue(queue);

            _monitor.SnapshotHandler = (path) =>
            {
                if (path.Status == NWPathStatus.Satisfied)
                {
                    if (path.IsConstrained)
                    {
                        _costType = NetworkCostType.Limited;
                    }
                    else
                    {
                        _costType = NetworkCostType.Unlimited;
                    }
                }
                else
                {
                    _costType = NetworkCostType.Disconnected;
                }
            };

            _monitor.Start();
        }

        public override NetworkCostType GetNetworkCostType()
        {
            return _costType;
        }
    }
}
