using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public class TelemetryPageViewBundler
    {
        private const int MaxPropertyValueLength = 8000; // Up to 8192 technically

        public DateTime UtcDate { get; private set; } = DateTime.UtcNow.Date;

        public string UserId { get; private set; }

        private string _pageEntries;

        public TelemetryPageViewBundler(string userId)
        {
            UserId = userId;
        }

        public bool TryAddPage(string userId, string pageName, DateTime utcTimeVisited)
        {
            // If different date, we need a new group
            if (utcTimeVisited.Date != UtcDate)
            {
                return false;
            }

            // If different user ID, we need a new group
            if (UserId != userId)
            {
                return false;
            }

            string pageEntry = FormatPageEntry(pageName, utcTimeVisited);

            // If very first
            if (_pageEntries == null)
            {
                _pageEntries = pageEntry;
                return true;
            }

            // If current has space
            if (HasSpace(_pageEntries, pageEntry))
            {
                _pageEntries += ";" + pageEntry;
                return true;
            }

            // Otherwise we're full
            return false;
        }

        public Dictionary<string, string> GenerateProperties()
        {
            var answer = new Dictionary<string, string>()
            {
                { "Date", UtcDate.ToString("yyyy-MM-dd") }, // ISO8601 format
                { "AccountId", UserId } // We have to add AccountId, since if it changed, the AccountId automatically logged would be different
            };

            if (_pageEntries != null)
            {
                answer.Add("Entries", _pageEntries);
            }

            return answer;
        }

        private static bool HasSpace(string propertyValue, string pageEntry)
        {
            // {existing};{new} (the +1 is for the semicolon)
            return propertyValue.Length + 1 + pageEntry.Length <= MaxPropertyValueLength;
        }

        private static string FormatPageEntry(string pageName, DateTime utcTimeVisited)
        {
            // Calendar,15:07:05
            return $"{pageName},{FormatTime(utcTimeVisited)}";
        }

        private static string FormatTime(DateTime utcTimeVisited)
        {
            // Trim off any milliseconds
            TimeSpan timeVisited = new TimeSpan(utcTimeVisited.Hour, utcTimeVisited.Minute, utcTimeVisited.Second);

            // Formats it correctly in format Microsoft Log Query expects!
            return timeVisited.ToString();
        }
    }
}
