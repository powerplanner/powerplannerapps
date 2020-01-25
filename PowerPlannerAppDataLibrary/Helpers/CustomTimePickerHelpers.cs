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

        // =============================
        // TEXT PARSING:
        // =============================

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
                var successful = HandleSingleNumberSplitParts(ref timeSpan, is24hour, isPM, numberPart, numberSplitParts);
                return HandleRelative(ref timeSpan, startTime.TimeOfDay, relativeTime, successful);
            }

            // Otherwise, if there was more than one item - we know that it was definitely "xx:xx", so, we'll just parse those numbers!
            var wasSuccessful = ParseMinuteAndHour(ref timeSpan, is24hour, isPM, numberSplitParts);
            return HandleRelative(ref timeSpan, startTime.TimeOfDay, relativeTime, wasSuccessful);
        }

        /// <summary>
        /// Checks if the <paramref name="str"/> is a 12-hour time and if so, whether it's AM or PM.
        /// </summary>
        private static bool CheckFor12Hour(string str, out bool is24hour, out bool isPM)
        {
            is24hour = true;
            isPM = false;

            if (str.EndsWith("am"))
                is24hour = false;
            else if (str.EndsWith("pm"))
            {
                is24hour = false;
                isPM = true;
            }

            return true;
        }

        /// <summary>
        /// Takes in a string array with two items in it, one is the hour, and one is the minute.
        /// </summary>
        private static bool ParseMinuteAndHour(ref TimeSpan timeSpan, bool is24hour, bool isPM, string[] numberSplitParts)
        {
            if (!int.TryParse(numberSplitParts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int hour) || !Handle12Hour(ref hour, out bool maxTime, is24hour, isPM))
                return false; // Fail

            // If after adding 12-hours for PM, the clock hits 24:00, push that back down to 23:59 - and return that.
            if (maxTime)
            {
                timeSpan = new TimeSpan(23, 59, 0);
                return true;
            }

            if (!int.TryParse(numberSplitParts[1], out int minute) || minute >= 60)
                return false; // Fail

            timeSpan = new TimeSpan(hour, minute, 0);

            return true;
        }

        // xhxm or xx
        private static bool HandleSingleNumberSplitParts(ref TimeSpan timeSpan, bool is24hour, bool isPM, string numberPart, string[] numberParts)
        {
            // Check if it's in the format "xhxm" (or the format "xm").
            if (HandleXHXM(ref timeSpan, is24hour, isPM, numberPart))
                return true;

            // If we got here, then we've confirmed that it is not in the xhxm format.
            if (int.TryParse(numberParts[0], out int singleHour) && singleHour < 24 &&
                Handle12Hour(ref singleHour, out bool singleMaxTime, is24hour, isPM))
            {
                // If the 12-hour clock hits 24:00, push that back down to 23:59.
                if (singleMaxTime)
                    timeSpan = new TimeSpan(23, 59, 0);
                else
                    timeSpan = new TimeSpan(singleHour, 0, 0);
                return true;
            }
            else
                return false; // Fail
        }

        private static bool HandleXHXM(ref TimeSpan timeSpan, bool is24hour, bool isPM, string numberPart)
        {
            var isXHXM = is24hour ? numberPart.EndsWith("m") : numberPart.EndsWith("h");

            // If it's not XHXM, then stop.
            if (isXHXM)
            {
                string[] parts;
                int mPos = 0;

                // The hour part can be excluded (so, we can literally just have "xm"/"xxm", so, let's see if we have an hour part.
                var hPos = numberPart.IndexOf('h');
                var hasHour = hPos != -1;

                // If we do have a "h", we'll split the string into two (the hour and minute), and place it into the array.
                if (hasHour) {
                    parts = new string[] { numberPart.Substring(0, hPos), numberPart.Substring(hPos + 1, numberPart.Length - hPos - 1) };
                    mPos = parts[1].IndexOf('m');
                }

                // If not, then it could be just a minute (like "xm"), so, we're going to check to see if there's an "m" in there, and if not, then it's not that - so it can't be XHXM.
                else if ((mPos = numberPart.IndexOf('m')) != -1)
                {
                    parts = new string[] { numberPart };
                }

                // If it wasn't "xhxm" or "xm", then it can't be XHXM, so, return false.
                else return false;

                // Remove the "m", if there is one.
                if (mPos != -1)
                    parts[hasHour ? 1 : 0] = parts[hasHour ? 1 : 0].Remove(mPos, 1);

                // Finally, parse the (optionally hour) and minute.
                if (hasHour)
                    return ParseMinuteAndHour(ref timeSpan, is24hour, isPM, parts);
                else 
                {
                    var successful = int.TryParse(parts[0], out int res);
                    timeSpan = new TimeSpan(0, res, 0);
                    return successful;
                }
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
        public static bool Handle12Hour(ref int hour, out bool maxTime, bool is24hour, bool isPM)
        {
            maxTime = false;

            // Don't do anything if it's actually 24-hour.
            if (is24hour)
                return true;

            // However, if it is, and it's PM, then we have to add 12 hours, and fail if that doesn't work.
            if (isPM)
            {
                hour += 12;
                if (hour > 24)
                    return false; // Fail
                if (hour == 24)
                    maxTime = true;
            }

            return true;
        }
    }
}
