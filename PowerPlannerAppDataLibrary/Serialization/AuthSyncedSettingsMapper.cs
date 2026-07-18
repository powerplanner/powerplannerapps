using PowerPlannerSending;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;

namespace PowerPlannerAppDataLibrary.Serialization
{
    internal static class AuthSyncedSettingsMapper
    {
        public static SyncedSettings Convert(ExpandoObject settings)
        {
            if (settings == null)
            {
                return null;
            }

            var values = (IDictionary<string, object>)settings;
            return new SyncedSettings
            {
                GpaOption = GetNullableEnum<GpaOptions>(values, nameof(SyncedSettings.GpaOption)),
                WeekOneStartsOn = GetNullableDateTime(values, nameof(SyncedSettings.WeekOneStartsOn)),
                SelectedSemesterId = GetNullableGuid(values, nameof(SyncedSettings.SelectedSemesterId)),
                SchoolTimeZone = GetString(values, nameof(SyncedSettings.SchoolTimeZone)),
                DefaultGradeScale = GetGradeScales(values, nameof(SyncedSettings.DefaultGradeScale)),
                DefaultDoesRoundGradesUp = GetNullableBoolean(values, nameof(SyncedSettings.DefaultDoesRoundGradesUp)),
                DefaultDoesAverageGradeTotals = GetNullableBoolean(values, nameof(SyncedSettings.DefaultDoesAverageGradeTotals)),
                NoClassColor = GetBytes(values, nameof(SyncedSettings.NoClassColor)),
                PrimaryThemeColor = GetBytes(values, nameof(SyncedSettings.PrimaryThemeColor))
            };
        }

        private static object GetValue(IDictionary<string, object> values, string name)
        {
            values.TryGetValue(name, out object value);
            return value;
        }

        private static string GetString(IDictionary<string, object> values, string name)
        {
            return GetValue(values, name)?.ToString();
        }

        private static bool? GetNullableBoolean(IDictionary<string, object> values, string name)
        {
            object value = GetValue(values, name);
            if (value == null) return null;
            if (value is bool boolean) return boolean;
            return bool.TryParse(value.ToString(), out boolean) ? boolean : null;
        }

        private static DateTime? GetNullableDateTime(IDictionary<string, object> values, string name)
        {
            object value = GetValue(values, name);
            if (value == null) return null;
            if (value is DateTime dateTime) return dateTime;
            if (value is DateTimeOffset dateTimeOffset) return dateTimeOffset.UtcDateTime;
            return DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dateTime) ? dateTime : null;
        }

        private static Guid? GetNullableGuid(IDictionary<string, object> values, string name)
        {
            object value = GetValue(values, name);
            if (value is Guid guid) return guid;
            return value != null && Guid.TryParse(value.ToString(), out guid) ? guid : null;
        }

        private static T? GetNullableEnum<T>(IDictionary<string, object> values, string name) where T : struct
        {
            object value = GetValue(values, name);
            if (value == null) return null;
            if (value is T typedValue) return typedValue;
            if (Enum.TryParse(value.ToString(), out typedValue)) return typedValue;
            try { return (T)Enum.ToObject(typeof(T), System.Convert.ToInt32(value, CultureInfo.InvariantCulture)); } catch { return null; }
        }

        private static byte[] GetBytes(IDictionary<string, object> values, string name)
        {
            object value = GetValue(values, name);
            if (value == null) return null;
            if (value is byte[] bytes) return bytes;
            if (value is string text)
            {
                try { return System.Convert.FromBase64String(text); } catch { return null; }
            }
            if (value is IEnumerable sequence)
            {
                try { return sequence.Cast<object>().Select(item => System.Convert.ToByte(item, CultureInfo.InvariantCulture)).ToArray(); } catch { }
            }
            return null;
        }

        private static GradeScale[] GetGradeScales(IDictionary<string, object> values, string name)
        {
            object value = GetValue(values, name);
            if (value is GradeScale[] gradeScales) return gradeScales;
            if (!(value is IEnumerable sequence)) return null;

            var answer = new List<GradeScale>();
            foreach (object item in sequence)
            {
                if (!(item is IDictionary<string, object> gradeScale)) continue;
                object startGrade = GetValue(gradeScale, nameof(GradeScale.StartGrade));
                object gpa = GetValue(gradeScale, nameof(GradeScale.GPA));
                if (startGrade == null || gpa == null) continue;
                answer.Add(new GradeScale(
                    System.Convert.ToDouble(startGrade, CultureInfo.InvariantCulture),
                    System.Convert.ToDouble(gpa, CultureInfo.InvariantCulture)));
            }
            return answer.ToArray();
        }
    }
}