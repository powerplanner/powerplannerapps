using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PowerPlannerAppDataLibrary.Helpers
{
    /// <summary>
    /// Helper methods to generate intervaled time strings (00:00, 00:30 etc.) for ComboBoxes.
    /// </summary>
    public static class CustomTimePickerHelpers
    {
        /// <summary>
        /// Generates time offset text such as "+30m" or "+1h40m".
        /// </summary>
        public static string GenerateTimeOffsetText(TimeSpan time)
        {
            // Don't do anything if they're both 0.
            if (time.Hours == 0 && time.Minutes == 0)
                return "";

            string res = " (+";

            if (time.Hours > 0)
            {
                res += ForceTwoDigitNumber(time.Hours);
                res += "h";
            }
            if (time.Minutes > 0)
            {
                res += ForceTwoDigitNumber(time.Minutes);
                res += "m";
            }
            res += ")";

            return res;
        }

        static string ForceTwoDigitNumber(int num) => num < 10 ? "0" + num.ToString() : num.ToString();

        #region Text Parsing

        /// <summary>
        /// This will take some text and convert it into a TimeSpan, and will return true of false depending on success or not.
        /// Supported formats (square brackets are optional): xx:xx [AM/PM], xhx/xhxm/xxhxxm.
        /// In addition to this, if the given time has a "+" at the beginning, it will make the TimeSpan that amount relative to the <paramref name="startTime"/> (typically used on the EndTime).
        /// If the text given contains something in brackets at the end, e.g. "1:23 (+1h)", that will be ignored.
        /// </summary>
        public static bool ParseComboBoxItem(DateTime startTime, string itemText, out TimeSpan timeSpan)
        {
            if (itemText == null || itemText.Length == 0)
                return false; // Fail

            timeSpan = new TimeSpan();

            // NOTE: We're manually parsing it since not only did "TimeSpan.TryParse" not work with AM/PM, but the text may contain things such as brackets at the end, and we want the option to use relative times (if someone wanted to).
            // Check to see if this begins with a "+", making it a relative time, if it does begin with a plus, we'll remember that and remove it to stop it from causing problems later.
            var relativeTime = itemText[0] == '+';
            if (relativeTime)
                itemText = itemText.Remove(0, 1);

            // Remove anything that might be in brackets at the end, since the end time will probably contain this, e.g. (+30m).
            var splitByBracket = itemText.Split('(');
            if (splitByBracket.Length == 0)
                return false; // Fail

            var withRemovedBracketsAndLower = splitByBracket[0].ToLower().TrimEnd(); // TrimEnd will remove any trailing whitespace.
            if (withRemovedBracketsAndLower.Length == 0)
                return false; // Fail

            // Check if there's an AM or PM.
            CheckFor12Hour(withRemovedBracketsAndLower, out bool is24hour, out bool isPM);

            // Split it up as if it's a "xx:xx".
            var numberPart = is24hour ? withRemovedBracketsAndLower : withRemovedBracketsAndLower.Substring(0, withRemovedBracketsAndLower.Length - 2).TrimEnd();
            var numberSplitParts = numberPart.Split(':');
            if (numberSplitParts.Length == 0)
                return false;

            // If there was one item, then that means that we either have just one number or it was in the format "xhxm".
            if (numberSplitParts.Length == 1)
            {
                var successful = HandleSingleNumberSplitParts(ref timeSpan, is24hour, isPM, numberPart);
                return HandleRelative(ref timeSpan, startTime.TimeOfDay, relativeTime, successful);
            }

            // Otherwise, if there was more than one item - we know that it was definitely "xx:xx", so, we'll just parse those numbers!
            var wasSuccessful = ParseMinuteANDHour(ref timeSpan, is24hour, isPM, numberSplitParts);
            return HandleRelative(ref timeSpan, startTime.TimeOfDay, relativeTime, wasSuccessful);
        }

        /// <summary>
        /// Checks if the <paramref name="str"/> is a 12-hour time and if so, whether it's AM or PM.
        /// </summary>
        static bool CheckFor12Hour(string str, out bool is24hour, out bool isPM)
        {
            is24hour = true;
            isPM = false;

            if (str.EndsWith("am") || str.EndsWith("a"))
                is24hour = false;
            else if (str.EndsWith("pm") || str.EndsWith("p"))
            {
                is24hour = false;
                isPM = true;
            }

            return true;
        }

        // xhxm or xx
        static bool HandleSingleNumberSplitParts(ref TimeSpan timeSpan, bool is24hour, bool isPM, string numberPart)
        {
            // Check if it's in the format "xhxm" (or the format "xm").
            if (HandleXHXM(ref timeSpan, is24hour, isPM, numberPart))
                return true;

            // If it's not XHXM, then it's just one number, which we assume is the hour.
            if (!ParseMinuteORHour(numberPart, ref timeSpan, true, is24hour, isPM))
                return false;

            return true;
        }

        static bool HandleXHXM(ref TimeSpan timeSpan, bool is24hour, bool isPM, string numberPart)
        {
            // You can't enter a 12-hour time (AM/PM) as an "xhxm" format.
            if (!is24hour)
                return false;

            var isXHXM = numberPart.EndsWith("m") || numberPart.EndsWith("h");

            // Now, we'll work out what that minute and hour is.
            if (isXHXM)
            {
                var hPos = numberPart.IndexOf('h');
                var mPos = numberPart.IndexOf('m');
                var hasHour = hPos != -1;
                var hasMinute = mPos != -1;
                var hourPart = hasHour ? numberPart.Substring(0, hPos) : "";
                var minutePart = hasMinute ? numberPart.Substring(hasHour ? hPos + 1 : 0, numberPart.Length - hPos - 1).TrimEnd('m') : "";

                if (hasHour && hasMinute)
                    return ParseMinuteANDHour(ref timeSpan, is24hour, isPM, new string[] { hourPart, minutePart });
                else if (hasHour)
                    return ParseMinuteORHour(hourPart, ref timeSpan, true, is24hour, isPM);
                else if (hasMinute)
                    return ParseMinuteORHour(minutePart, ref timeSpan, false, is24hour, isPM);

                // If it wasn't any of those, this can't be valid.
                else return false;
            }
            return false;
        }

        static bool HandleRelative(ref TimeSpan before, TimeSpan offset, bool isRelative, bool wasSuccessful)
        {
            if (!isRelative)
                return wasSuccessful;

            before = offset.Add(before);

            if (before.TotalHours >= 24)
                before = new TimeSpan(23, 59, 0);

            return wasSuccessful;
        }

        /// <summary>
        /// Ensures that the hour on a 12 hour time is handled correctly.
        /// </summary>
        /// <param name="maxTime">If we hit 24 hours, then this will specify that the final TimeSpan needs to be 23:59, as 24:00 would be a problem.</param>
        /// <returns>True if valid, False is invalid</returns>
        static bool Handle12Hour(ref int hour, out bool maxTime, bool is24hour, bool isPM)
        {
            maxTime = false;

            // Don't do anything if it's actually 24-hour.
            if (is24hour)
                return true;

            // However, if it is, and it's PM, then we have to add 12 hours, and fail if that doesn't work.
            if (isPM)
            {
                // If the hour is "12", don't do anything, since "12 PM" is not "24:00", it's actually "12:00" in 24-hour time!
                if (hour == 12)
                    return true;

                hour += 12;
                if (hour > 24)
                    return false; // Fail
                if (hour == 24)
                    maxTime = true;
            }

            // "12 AM" becomes "00:xx"
            else if (hour == 12)
                hour = 0;

            return true;
        }

        /// <summary>
        /// Takes in a string array with two items in it, one is the hour, and one is the minute.
        /// </summary>
        static bool ParseMinuteANDHour(ref TimeSpan timeSpan, bool is24hour, bool isPM, string[] numberSplitParts)
        {
            // Handle the minute.
            if (!ParseMinuteORHour(numberSplitParts[1], ref timeSpan, false, is24hour, isPM))
                return false;

            // Handle the hour.
            if (!ParseMinuteORHour(numberSplitParts[0], ref timeSpan, true, is24hour, isPM))
                return false;

            return true;
        }

        /// <summary>
        /// Parses the given number, and places it into either the hour or minute of the given TimeSpan.
        /// </summary>
        static bool ParseMinuteORHour(string text, ref TimeSpan timeSpan, bool isHour, bool is24hour, bool isPM)
        {
            var timeLimit = isHour ? 23 : 59;

            if (isHour)
            {
                if (!int.TryParse(text, out int hour))
                    return false;

                if (!Handle12Hour(ref hour, out bool maxTime, is24hour, isPM))
                    return false;

                // If it reached the maximum time, push the time down to "23:59"
                if (maxTime)
                {
                    timeSpan = new TimeSpan(23, 59, 0);
                    return true;
                }

                if (hour < 0 || hour > timeLimit)
                    return false;

                timeSpan = timeSpan.Add(TimeSpan.FromHours(hour));
            } else {

                if (!int.TryParse(text, out int minute))
                    return false;

                if (minute < 0 || minute > timeLimit)
                    return false;

                timeSpan = timeSpan.Add(TimeSpan.FromMinutes(minute));
            }

            return true;
        }
#endregion
    }
}
