using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerAppDataLibrary.Helpers
{
    internal static class TimeZoneHelpers
    {
        public static bool TryToIanaId(this TimeZoneInfo timeZone, out string ianaId)
        {
            if (timeZone.HasIanaId)
            {
                ianaId = timeZone.Id;
                return true;
            }
            else if (TimeZoneInfo.TryConvertWindowsIdToIanaId(timeZone.Id, out ianaId))
            {
                return true;
            }
            else
            {
                switch (timeZone.Id)
                {
                    case "British Columbia Standard Time":
                        ianaId = "America/Vancouver";
                        return true;

                    case "Alberta Standard Time":
                        ianaId = "America/Edmonton";
                        return true;

                    case "Manitoba Standard Time":
                        ianaId = "America/Winnipeg";
                        return true;
                }
            }

            ianaId = null;
            return false;
        }

        public static bool TryFindSystemTimeZoneByIanaId(string ianaId, out TimeZoneInfo timeZone)
        {
            if (!OperatingSystem.IsWindows())
            {
                return TimeZoneInfo.TryFindSystemTimeZoneById(ianaId, out timeZone);
            }

            switch (ianaId)
            {
                case "America/Vancouver":
                    ianaId = "British Columbia Standard Time";
                    break;

                case "America/Edmonton":
                    ianaId = "Alberta Standard Time";
                    break;

                case "America/Winnipeg":
                    ianaId = "Manitoba Standard Time";
                    break;
            }

            return TimeZoneInfo.TryFindSystemTimeZoneById(ianaId, out timeZone);
        }
    }
}
