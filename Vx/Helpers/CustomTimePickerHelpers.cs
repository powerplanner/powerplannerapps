using System;
using System.Text.RegularExpressions;

namespace Vx.Helpers
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

        private static readonly Regex TimeRegex = new Regex(@"^(\d?\d)(?::(\d\d?))?\s*([aApP][mM]?)?");

        /// <summary>
        /// This will take some text and convert it into a TimeSpan, and will return true of false depending on success or not.
        /// Supported formats (square brackets are optional): xx:xx [AM/PM], xhx/xhxm/xxhxxm.
        /// In addition to this, if the given time has a "+" at the beginning, it will make the TimeSpan that amount relative to the <paramref name="startTime"/> (typically used on the EndTime).
        /// If the text given contains something in brackets at the end, e.g. "1:23 (+1h)", that will be ignored.
        /// </summary>
        public static bool ParseComboBoxItem(TimeSpan startTime, string itemText, bool is24hour, out TimeSpan timeSpan)
        {
            itemText = itemText.Trim();

            if (itemText == null || itemText.Length == 0)
                return false; // Fail

            timeSpan = new TimeSpan();

            // NOTE: We're manually parsing it since not only did "TimeSpan.TryParse" not work with AM/PM, but the text may contain things such as brackets at the end, and we want the option to use relative times (if someone wanted to).
            // Check to see if this begins with a "+", making it a relative time, if it does begin with a plus, we'll remember that and remove it to stop it from causing problems later.

            // If relative time
            if (itemText[0] == '+')
            {
                itemText = itemText.Substring(1);

                if (!HandleXHXM(ref timeSpan, true, false, itemText))
                {
                    // If it's not XHXM, then it's just one number, which we assume is the hour.
                    if (!ParseMinuteORHour(itemText, ref timeSpan, true, true, false))
                    {
                        var hourMinMatch = Regex.Match(itemText, @"^(\d?\d):(\d\d)$");
                        if (!hourMinMatch.Success)
                        {
                            return false;
                        }

                        int relHour = int.Parse(hourMinMatch.Groups[1].Value);
                        int relMin = int.Parse(hourMinMatch.Groups[2].Value);

                        timeSpan = new TimeSpan(relHour, relMin, 0);
                    }
                }

                timeSpan = startTime + timeSpan;
                if (timeSpan > new TimeSpan(23, 59, 0))
                {
                    timeSpan = new TimeSpan(23, 59, 0);
                }

                return true;
            }

            var match = TimeRegex.Match(itemText);
            if (!match.Success)
            {
                return false;
            }

            string hourStr = match.Groups[1].Value;
            string minStr = match.Groups[2].Success ? match.Groups[2].Value : null;
            if (minStr != null && minStr.Length == 1)
            {
                minStr += "0";
            }
            string amPmStr = match.Groups[3].Success ? match.Groups[3].Value : null;

            int hour = int.Parse(hourStr);
            int min = minStr != null ? int.Parse(minStr) : 0;

            if (hour > 23 || min > 59)
            {
                return false;
            }

            bool? isAm = null;

            // If in 24-hour mode, or they entered 24-hour style despite not being in 24-hour
            if (is24hour || (!is24hour && ((hourStr.Length == 2 && hour != 10 && hour != 11 && hour != 12) || hour == 0)))
            {
                // And switch to 24hr mode
                is24hour = true;
            }

            // If they specified AM/PM (regardless of 12/24 hour), that takes precedent
            if (amPmStr != null)
            {
                isAm = amPmStr.StartsWith("a", StringComparison.CurrentCultureIgnoreCase);
            }

            // Handle both 24 hour and AM/PM used
            if (is24hour && amPmStr != null)
            {
                if (hour == 0 || hour > 12)
                {
                    // Can't use AM/PM with definitively entered 24-hour times
                    return false;
                }

                // AM/PM string takes precedent
                is24hour = false;
            }

            if (is24hour)
            {
                if ((hour == 0 || hour > 12) && amPmStr != null)
                {
                    // Can't use AM/PM with definitively entered 24-hour times
                    return false;
                }

                timeSpan = new TimeSpan(hour, min, 0);
            }
            else
            {
                if (isAm == null)
                {
                    // If start time is specified, we should pick whichever is closest (and greater)...
                    // For example, if start time is 3am and they enter 4:00, pick 4am instead of 4pm
                    if (startTime != default(TimeSpan))
                    {
                        var earlierOption = ParseAmPmTime(hour, min, isAm: true);
                        var laterOption = ParseAmPmTime(hour, min, isAm: false);

                        if (earlierOption > startTime)
                        {
                            isAm = true;
                        }
                        else
                        {
                            isAm = false;
                        }
                    }
                    else
                    {
                        // Otherwise pick based on typical reasonable range
                        isAm = hour >= 6 && hour <= 11;
                    }
                }

                timeSpan = ParseAmPmTime(hour, min, isAm.Value);
            }

            if (timeSpan > startTime || startTime == default(TimeSpan))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static TimeSpan ParseAmPmTime(int hour, int minute, bool isAm)
        {
            if (isAm && hour == 12)
            {
                hour = 0;
            }

            else if (!isAm && hour != 12)
            {
                // If PM, add 12
                hour += 12;
            }

            return new TimeSpan(hour, minute, 0);
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
