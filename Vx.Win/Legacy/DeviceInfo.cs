using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Profile;
using Windows.UI.ViewManagement;

namespace InterfacesUWP
{
    public static class DeviceInfo
    {
        private static DeviceFamily? _currentDeviceFamily;

        public static DeviceFamily DeviceFamily
        {
            get
            {
                if (_currentDeviceFamily == null)
                {
                    switch (AnalyticsInfo.VersionInfo.DeviceFamily)
                    {
                        case "Windows.Mobile":
                            _currentDeviceFamily = DeviceFamily.Mobile;
                            break;

                        case "Windows.Desktop":
                            _currentDeviceFamily = DeviceFamily.Desktop;
                            break;

                        default:
                            _currentDeviceFamily = DeviceFamily.Unknown;
                            break;
                    }
                }

                return _currentDeviceFamily.Value;
            }
        }

        public static DeviceFormFactor GetCurrentDeviceFormFactor()
        {
            if (UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Touch)
            {
                if (DeviceFamily == DeviceFamily.Mobile)
                    return DeviceFormFactor.Mobile;

                return DeviceFormFactor.Tablet;
            }

            else
            {
                return DeviceFormFactor.Desktop;
            }
        }
    }

    public enum DeviceFamily
    {
        Desktop,
        Mobile,
        Unknown
    }

    public enum DeviceFormFactor
    {
        Mobile,
        Tablet,
        Desktop
    }
}
