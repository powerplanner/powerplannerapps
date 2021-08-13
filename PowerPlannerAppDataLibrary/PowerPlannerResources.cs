using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using System.Reflection;
using System.Globalization;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerAppDataLibrary
{
    public static class PowerPlannerResources
    {
        private static ResourceManager _resourceManager;
        private static CultureInfo _cultureInfo;

        public static string GetString(string id)
        {
            if (LocalizationExtension.Current != null)
            {
                return LocalizationExtension.Current.GetString(id);
            }

            if (_resourceManager == null)
            {
                //_resourceManager = Strings.Resources.ResourceManager;
                _resourceManager = new ResourceManager("PowerPlannerAppDataLibrary.Strings.Resources", typeof(PowerPlannerResources).GetTypeInfo().Assembly);
            }

            if (_cultureInfo == null)
            {
                _cultureInfo = PortableLocalizedResources.GetCurrentCulture();
            }

            return _resourceManager.GetString(id, _cultureInfo);
        }

        public static string GetCapitalizedString(string id)
        {
            string str = GetString(id);

            if (str.Length > 0)
            {
                if (_cultureInfo == null)
                {
                    _cultureInfo = PortableLocalizedResources.GetCurrentCulture();
                }

                if (_cultureInfo.TwoLetterISOLanguageName == "ar")
                {
                    return str.Substring(0, str.Length - 1) + char.ToUpper(str[str.Length - 1]);
                }

                return char.ToUpper(str[0]) + str.Substring(1);
            }

            return str;
        }

        public static string GetStringAsLowercaseWithParameters(string id, params object[] insertParams)
        {
            string str = GetString(id);

            str = str.ToLower();
            str = string.Format(str, insertParams);

            return str;
        }

        public static string GetStringWithParameters(string id, params object[] insertParams)
        {
            string str = GetString(id);

            str = string.Format(str, insertParams);

            return str;
        }

        /// <summary>
        /// Returns something like "Ex: {exampleText}"
        /// </summary>
        /// <param name="exampleText"></param>
        /// <returns></returns>
        public static string GetExamplePlaceholderString(string exampleText)
        {
            return GetStringWithParameters("String_ExamplePlaceholderText", exampleText);
        }

        public static string GetStringMenuItem(NavigationManager.MainMenuSelections menuItem)
        {
            return GetString("MainMenuItem_" + menuItem);
        }

        public static string GetEnabledDisabledString(bool enabled)
        {
            return GetString(enabled ? "String_Enabled" : "String_Disabled");
        }

        /// <summary>
        /// Returns something like "Due {x}", where {x} is dueValue
        /// </summary>
        /// <param name="dueValue"></param>
        /// <returns></returns>
        public static string GetDueX(string dueValue)
        {
            return string.Format(GetString("String_DueX"), dueValue);
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

        public static string GetXMinutes(int minutes)
        {
            return string.Format(GetString("String_XMinutes"), minutes);
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
                    return GetString("String_BothWeeks");

                case Schedule.Week.WeekOne:
                    return GetStringWeekA();

                case Schedule.Week.WeekTwo:
                    return GetStringWeekB();

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

        public static string GetMenuItemDelete()
        {
            return GetString("MenuItemDelete");
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

        public static string GetStringPremiumDescription(params int[] bulletsToInclude)
        {
            string str = PowerPlannerResources.GetString("Settings_UpgradeToPremium_Description.Text");

            string[] splits = str.Split(new char[] { '\n' }).Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.TrimEnd()).ToArray();

            string firstDescription = splits[0] + "\n\n" + splits[1];
            string[] bullets = splits.Skip(2).Take(splits.Length - 3).ToArray();
            string lastDescription = splits.Last();

            string answer = firstDescription;

            for (int i = 0; i < bullets.Length; i++)
            {
                if (bulletsToInclude.Contains(i))
                {
                    answer += "\n" + bullets[i];
                }
            }

            return answer + "\n\n" + lastDescription;
        }

        public static string GetXHours(int hours)
        {
            if (hours == 1)
            {
                return GetString("String_OneHour");
            }

            return string.Format(GetString("String_XHours"), hours);
        }
    }
}
