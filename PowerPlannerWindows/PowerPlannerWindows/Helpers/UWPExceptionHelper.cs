using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerUWP.Helpers
{
    public static class UWPExceptionHelper
    {
        public static bool TrackIfRpcServerUnavailable(Exception ex, string host)
        {
            // RPC server is unavailable
            if (ExceptionHelper.IsHResult(ex, 0x800706BA))
            {
                TelemetryExtension.Current?.TrackEvent($"Error_{host}_RpcUnavailable_0x800706BA");
                return true;
            }

            return false;
        }

        public static bool TrackIfPathInvalid(Exception ex, string host)
        {
            // The specified path is invalid.
            if (ExceptionHelper.IsHResult(ex, 0x800700A1))
            {
                // This issue seems to affect things that require package identity, like registering background tasks
                // or scheduling tile/toast notifications... only affects a super small set of users, but when they
                // get the error, they seem to be permanently affected.
                // User "jpCrayola" (1369871) is the one that's hit by this error the most
                TelemetryExtension.Current?.TrackEvent($"Error_{host}_PathInvalid_0x800700A1");
                return true;
            }

            return false;
        }

        public static bool TrackIfWpnDbMalformed(Exception ex, string host)
        {
            // The database disk image is malformed.
            if (ExceptionHelper.IsHResult(ex, 0x87AF000B))
            {
                // This issue occurs when the WPN notifications database is corrupted.
                // Seems to persistently occur once it starts happening.
                TelemetryExtension.Current?.TrackEvent($"Error_{host}_WpnDbMalformed_0x87AF000B");
                return true;
            }

            return false;
        }

        public static bool TrackIfNotificationPlatformUnavailable(Exception ex, string host)
        {
            // The notification platform is unavailable
            if (ExceptionHelper.IsHResult(ex, 0x803E0105))
            {
                // Occurs randomly when notification platform or Shell goes down
                TelemetryExtension.Current?.TrackEvent($"Error_{host}_NotifPlatUnavailable_0x803E0105");
                return true;
            }

            return false;
        }

        public static bool TrackIfInternalErrorOccurred(Exception ex, string host)
        {
            // An internal error occurred
            if (ExceptionHelper.IsHResult(ex, 0x8007054F))
            {
                // Seems to occur very rarely, happened once likely when sending tile notifications to primary tile?
                TelemetryExtension.Current?.TrackEvent($"Error_{host}_InternalError_0x8007054F");
                return true;
            }

            return false;
        }

        public static bool TrackIfNotificationsIssue(Exception ex, string host)
        {
            return TrackIfRpcServerUnavailable(ex, host)
                || TrackIfPathInvalid(ex, host)
                || TrackIfWpnDbMalformed(ex, host)
                || TrackIfNotificationPlatformUnavailable(ex, host)
                || TrackIfInternalErrorOccurred(ex, host);
        }

        public static bool TrackIfElementNotFound(Exception ex, string host)
        {
            // Element not found
            if (ExceptionHelper.IsHResult(ex, 0x80070490))
            {
                // This seems to occur sometimes when trying to send tile notifications to secondary tiles
                TelemetryExtension.Current?.TrackEvent($"Error_{host}_ElementNotFound_0x80070490");
                return true;
            }

            return false;
        }
    }
}
