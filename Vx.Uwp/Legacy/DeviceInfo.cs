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

        private static ulong _buildNumber;
        public static ulong BuildNumber
        {
            get
            {
                if (_buildNumber == 0)
                {
                    string deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                    ulong version = ulong.Parse(deviceFamilyVersion);
                    _buildNumber = (version & 0x00000000FFFF0000L) >> 16;
                }

                return _buildNumber;
            }
        }
        public static bool IsWindows11 => BuildNumber >= 22000;
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
