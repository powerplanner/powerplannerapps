using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using Windows.ApplicationModel.Resources;

namespace PowerPlannerUWP
{
    public static class LocalizedResources
    {
        private static ResourceLoader resources;

        private static ResourceLoader GetResourceLoader()
        {
            if (resources == null)
            {
                resources = new ResourceLoader("Resources");
            }
            return resources;
        }

        public static string GetString(string resource)
        {
            // We replace periods with forward slash, since that's how UWP requires accessing these
            return GetResourceLoader().GetString(resource.Replace('.', '/'));
        }

        public static class Common
        {
            public static string GetStringHoliday()
            {
                return GetString("String_Holiday");
            }

            /// <summary>
            /// Returns "Today"
            /// </summary>
            /// <returns></returns>
            public static string GetRelativeDateToday()
            {
                return GetString("RelativeDate_Today");
            }

            /// <summary>
            /// Returns "Tomorrow"
            /// </summary>
            /// <returns></returns>
            public static string GetRelativeDateTomorrow()
            {
                return GetString("RelativeDate_Tomorrow");
            }

            /// <summary>
            /// Returns "In 2 days"
            /// </summary>
            /// <returns></returns>
            public static string GetRelativeDateInTwoDays()
            {
                return GetString("RelativeDate_InTwoDays");
            }

            /// <summary>
            /// Returns "In {0} days"
            /// </summary>
            /// <param name="days"></param>
            /// <returns></returns>
            public static string GetRelativeDateInXDays(int days)
            {
                return string.Format(GetString("RelativeDate_InXDays"), days);
            }

            /// <summary>
            /// Returns "Within {0} days"
            /// </summary>
            /// <param name="days"></param>
            /// <returns></returns>
            public static string GetRelativeDateWithinXDays(int days)
            {
                return string.Format(GetString("RelativeDate_WithinXDays"), days);
            }

            /// <summary>
            /// Returns "Future"
            /// </summary>
            /// <returns></returns>
            public static string GetRelativeDateFuture()
            {
                return GetString("RelativeDate_Future");
            }

            /// <summary>
            /// Returns "This Monday"
            /// </summary>
            /// <param name="dayOfWeek"></param>
            /// <returns></returns>
            public static string GetRelativeDateThisDayOfWeek(DayOfWeek dayOfWeek)
            {
                return string.Format(GetString("RelativeDate_ThisDayOfWeek"), DateTools.ToLocalizedString(dayOfWeek));
            }

            /// <summary>
            /// Returns "Next Monday"
            /// </summary>
            /// <param name="dayOfWeek"></param>
            /// <returns></returns>
            public static string GetRelativeDateNextDayOfWeek(DayOfWeek dayOfWeek)
            {
                return string.Format(GetString("RelativeDate_NextDayOfWeek"), DateTools.ToLocalizedString(dayOfWeek));
            }

            public static string GetRelativeDateXDaysAgo(int daysAgo)
            {
                return string.Format(GetString("RelativeDate_XDaysAgo"), daysAgo);
            }

            /// <summary>
            /// Returns "In the past"
            /// </summary>
            /// <returns></returns>
            public static string GetRelativeDateInThePast()
            {
                return GetString("RelativeDate_InThePast");
            }

            public static string GetStringPinToStart()
            {
                return GetString("String_PinToStart");
            }

            public static string GetStringUnpinFromStart()
            {
                return GetString("String_UnpinFromStart");
            }

            public static string GetLocalizedWeek(Schedule.Week week)
            {
                switch (week)
                {
                    case Schedule.Week.BothWeeks:
                        return LocalizedResources.GetString("String_BothWeeks");

                    case Schedule.Week.WeekOne:
                        return LocalizedResources.Common.GetStringWeekA();

                    case Schedule.Week.WeekTwo:
                        return LocalizedResources.Common.GetStringWeekB();

                    default:
                        throw new NotImplementedException();
                }
            }

            public static string GetStringWeekA()
            {
                return GetString("String_WeekA");
            }

            public static string GetStringWeekB()
            {
                return GetString("String_WeekB");
            }



            public static string GetRelativeDateYesterday()
            {
                return GetString("RelativeDate_Yesterday");
            }

            public static string GetStringNewItem()
            {
                return GetString("String_NewItem");
            }

            public static string GetStringGoToToday()
            {
                return GetString("String_GoToToday");
            }

            public static string GetStringTimeToTime(string start, string end)
            {
                return string.Format(GetString("String_TimeToTime"), start, end);
            }

            public static string GetStringSave()
            {
                return GetString("String_Save");
            }

            public static string GetStringCancel()
            {
                return GetString("String_Cancel");
            }

            public static string GetStringClose()
            {
                return GetString("String_Close");
            }

            public static string GetStringNoNameMessageBody()
            {
                return GetString("String_NoNameMessageBody");
            }

            public static string GetStringNoNameMessageHeader()
            {
                return GetString("String_NoNameMessageHeader");
            }

            public static string GetStringNoClassMessageBody()
            {
                return GetString("String_NoClassMessageBody");
            }

            public static string GetStringNoClassMessageHeader()
            {
                return GetString("String_NoClassMessageHeader");
            }

            public static string GetStringOfflineExplanation()
            {
                return GetString("String_OfflineExplanation");
            }

            public static string GetStringTask()
            {
                return GetString("String_Task");
            }

            public static string GetStringEvent()
            {
                return GetString("String_Event");
            }
        }
    }
}
