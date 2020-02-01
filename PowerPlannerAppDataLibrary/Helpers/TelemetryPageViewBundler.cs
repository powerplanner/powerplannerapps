using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public class TelemetryPageViewBundler
    {
        /// <summary>
        /// Event properties will include AccountId, Date, and then 18 spots more left for page properties
        /// </summary>
        private const int MaxPageProperties = 18;

        private const int MaxPropertyValueLength = 125;

        public DateTime UtcDate { get; private set; } = DateTime.UtcNow.Date;

        public string UserId { get; private set; }

        private List<string> _pageProperties = new List<string>();

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
            if (_pageProperties.Count == 0)
            {
                _pageProperties.Add(pageEntry);
                return true;
            }

            // If current has space
            if (HasSpace(_pageProperties.Last(), pageEntry))
            {
                _pageProperties[_pageProperties.Count - 1] = _pageProperties.Last() + ";" + pageEntry;
                return true;
            }

            // Otherwise if we're under property limit
            if (_pageProperties.Count <= MaxPageProperties)
            {
                // Add new
                _pageProperties.Add(pageEntry);
            }

            // Otherwise we're full
            return false;
        }

        public Dictionary<string, string> GenerateProperties()
        {
            var answer = new Dictionary<string, string>()
            {
                { "Date", UtcDate.ToString("yyyy-MM-dd") } // ISO8601 format
            };

            // Note that AccountId is added automatically

            // Entries0 - Entries17
            for (int i = 0; i < _pageProperties.Count; i++)
            {
                answer.Add($"Entries{i}", _pageProperties[i]);
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
            // Calendar,00:00:05
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
