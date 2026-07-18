using PowerPlannerAppDataLibrary.DataLayer.TileSettings;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    internal static class AccountDataPersistence
    {
        public static string Serialize(AccountDataItem account)
        {
            Validate(account);
            return SerializeXml(account);
        }

        public static AccountDataItem Deserialize(string contents, Guid localAccountId)
        {
            if (string.IsNullOrWhiteSpace(contents) || !contents.TrimStart().StartsWith("<"))
            {
                throw new InvalidDataException("Account data was not XML.");
            }

            AccountDataItem account = DeserializeXml(contents, localAccountId);
            Validate(account);
            return account;
        }

        private static void Validate(AccountDataItem account)
        {
            if (account == null)
            {
                throw new InvalidDataException("Account data was empty.");
            }

            if (string.IsNullOrEmpty(account.Username))
            {
                throw new InvalidDataException("Account data did not contain a username.");
            }
        }

        private static string SerializeXml(AccountDataItem account)
        {
            XNamespace accountNamespace = "http://schemas.datacontract.org/2004/07/PowerPlannerUWPLibrary.DataLayer";
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

            XElement Element(string name, object value) => new XElement(accountNamespace + name, value);
            XElement NullableElement(string name, object value) => value == null
                ? new XElement(accountNamespace + name, new XAttribute(xsi + "nil", true))
                : Element(name, value);
            string Date(DateTime value) => XmlConvert.ToString(value, XmlDateTimeSerializationMode.RoundtripKind);

            var root = new XElement(accountNamespace + "AccountDataItem",
                new XAttribute(XNamespace.Xmlns + "i", xsi),
                Element("Version", account.Version?.ToString()),
                Element("AccountDataVersion", account.AccountDataVersion),
                Element("SyncedDataVersion", account.SyncedDataVersion),
                Element("CurrentChangeNumber", account.CurrentChangeNumber),
                Element("CurrentDefaultGradeScaleIndex", account.CurrentDefaultGradeScaleIndex),
                Element("NeedsInitialSync", account.NeedsInitialSync),
                Element("NeedsToSyncSettings", account.NeedsToSyncSettings),
                Element("IsAppointmentsUpToDate", account.IsAppointmentsUpToDate),
                Element("IsTasksCalendarIntegrationDisabled", account.IsTasksCalendarIntegrationDisabled),
                Element("IsClassesCalendarIntegrationDisabled", account.IsClassesCalendarIntegrationDisabled),
                NullableElement("_serializedSchoolTimeZone", account.SchoolTimeZone?.Id),
                Element("GpaOption", account.GpaOption),
                Element("WeekOneStartsOn", Date(account.WeekOneStartsOn)),
                Element("ShowPastCompleteItemsOnFullCalendar", account.ShowPastCompleteItemsOnCalendar),
                Element("ShowSchedule", account.ShowSchedule),
                Element("ShowBackground", account.ShowBackground),
                Element("Reviewed", account.Reviewed),
                Element("HasAddedRepeating", account.HasAddedRepeating),
                account.ClassRemindersTimeSpan.HasValue
                    ? Element("ClassRemindersTimeSpan", XmlConvert.ToString(account.ClassRemindersTimeSpan.Value))
                    : NullableElement("ClassRemindersTimeSpan", null),
                Element("RemindersDayBefore", account.RemindersDayBefore),
                Element("RemindersDayOf", account.RemindersDayOf),
                Element("ImageUploadOption", account.ImageUploadOption),
                Element("IsPushDisabled", account.IsPushDisabled),
                SerializeGradeScales(accountNamespace, account.DefaultGradeScale),
                Element("DefaultDoesAverageGradeTotals", account.DefaultDoesAverageGradeTotals),
                Element("DefaultDoesRoundGradesUp", account.DefaultDoesRoundGradesUp),
                NullableElement("CustomPrimaryThemeColor", account.CustomPrimaryThemeColor == null ? null : Convert.ToBase64String(account.CustomPrimaryThemeColor)),
                Element("_isSoundEffectsDisabled", !account.IsSoundEffectsEnabled),
                NullableElement("NoClassColor", account.NoClassColor == null ? null : Convert.ToBase64String(account.NoClassColor)),
                SerializeMainTileSettings(accountNamespace, account.MainTileSettings),
                Element("DeviceId", account.DeviceId),
                Element("AccountId", account.AccountId),
                Element("Username", account.Username),
                NullableElement("Token", account.LocalToken),
                NullableElement("OnlineToken", account.Token),
                NullableElement("Password", null),
                Element("RememberUsername", account.RememberUsername),
                Element("RememberPassword", account.RememberPassword),
                Element("AutoLogin", account.AutoLogin),
                Element("DateLastDayBeforeReminderWasSent", Date(account.PersistedDateLastDayBeforeReminderWasSent)),
                SerializeCustomEndTimes(accountNamespace, account.CustomEndTimes),
                Element("CurrentSemesterId", account.CurrentSemesterId),
                Element("PremiumAccountExpiresOn", Date(account.PremiumAccountExpiresOn)));

            return new XDocument(new XDeclaration("1.0", "utf-8", null), root).ToString(SaveOptions.DisableFormatting);
        }

        private static XElement SerializeGradeScales(XNamespace accountNamespace, IEnumerable<GradeScale> gradeScales)
        {
            return new XElement(accountNamespace + "_defaultGradeScale",
                (gradeScales ?? Enumerable.Empty<GradeScale>()).Select(scale =>
                    new XElement(accountNamespace + "GradeScale",
                        new XElement(accountNamespace + "StartGrade", scale.StartGrade.ToString("R", CultureInfo.InvariantCulture)),
                        new XElement(accountNamespace + "GPA", scale.GPA.ToString("R", CultureInfo.InvariantCulture)))));
        }

        private static XElement SerializeMainTileSettings(XNamespace accountNamespace, MainTileSettings settings)
        {
            return new XElement(accountNamespace + "_mainTileSettings",
                new XElement(accountNamespace + "ShowTasks", settings.ShowTasks),
                new XElement(accountNamespace + "ShowEvents", settings.ShowEvents),
                new XElement(accountNamespace + "SkipItemsOlderThan", settings.SkipItemsOlderThan));
        }

        private static XElement SerializeCustomEndTimes(XNamespace accountNamespace, IDictionary<DayOfWeek, TimeSpan> customEndTimes)
        {
            return new XElement(accountNamespace + "CustomEndTimes",
                (customEndTimes ?? new Dictionary<DayOfWeek, TimeSpan>()).Select(item =>
                    new XElement(accountNamespace + "KeyValueOfDayOfWeekduration",
                        new XElement(accountNamespace + "Key", item.Key),
                        new XElement(accountNamespace + "Value", XmlConvert.ToString(item.Value)))));
        }

        private static AccountDataItem DeserializeXml(string contents, Guid localAccountId)
        {
            XDocument document = XDocument.Parse(contents);
            var account = new AccountDataItem(localAccountId);

            string GetValue(string name) => document.Root?.Elements().FirstOrDefault(element => element.Name.LocalName == name)?.Value;
            bool HasValue(string name) => GetValue(name) != null;

            if (TryParseVersion(document.Root?.Elements().FirstOrDefault(element => element.Name.LocalName == "Version"), out Version version)) account.Version = version;
            if (int.TryParse(GetValue("AccountDataVersion"), out int accountDataVersion)) account.AccountDataVersion = accountDataVersion;
            if (int.TryParse(GetValue("SyncedDataVersion"), out int syncedDataVersion)) account.SyncedDataVersion = syncedDataVersion;
            if (int.TryParse(GetValue("CurrentChangeNumber"), out int currentChangeNumber)) account.CurrentChangeNumber = currentChangeNumber;
            if (long.TryParse(GetValue("CurrentDefaultGradeScaleIndex"), out long gradeScaleIndex)) account.CurrentDefaultGradeScaleIndex = gradeScaleIndex;
            if (bool.TryParse(GetValue("NeedsInitialSync"), out bool needsInitialSync)) account.NeedsInitialSync = needsInitialSync;
            if (bool.TryParse(GetValue("NeedsToSyncSettings"), out bool needsToSyncSettings)) account.NeedsToSyncSettings = needsToSyncSettings;
            if (bool.TryParse(GetValue("IsAppointmentsUpToDate"), out bool appointmentsUpToDate)) account.IsAppointmentsUpToDate = appointmentsUpToDate;
            if (bool.TryParse(GetValue("IsTasksCalendarIntegrationDisabled"), out bool tasksCalendarDisabled)) account.IsTasksCalendarIntegrationDisabled = tasksCalendarDisabled;
            if (bool.TryParse(GetValue("IsClassesCalendarIntegrationDisabled"), out bool classesCalendarDisabled)) account.IsClassesCalendarIntegrationDisabled = classesCalendarDisabled;

            string schoolTimeZone = GetValue("_serializedSchoolTimeZone");
            if (!string.IsNullOrEmpty(schoolTimeZone))
            {
                try { account.SchoolTimeZone = TimeZoneInfo.FindSystemTimeZoneById(schoolTimeZone); } catch { }
            }

            if (TryParseEnum(GetValue("GpaOption"), out GpaOptions gpaOption)) account.GpaOption = gpaOption;
            if (TryParseDateTime(GetValue("WeekOneStartsOn"), out DateTime weekOneStartsOn)) account.WeekOneStartsOn = weekOneStartsOn;
            if (bool.TryParse(GetValue("ShowPastCompleteItemsOnFullCalendar") ?? GetValue("ShowPastCompleteItemsOnCalendar"), out bool showPast)) account.ShowPastCompleteItemsOnCalendar = showPast;
            if (bool.TryParse(GetValue("ShowSchedule"), out bool showSchedule)) account.ShowSchedule = showSchedule;
            if (bool.TryParse(GetValue("ShowBackground"), out bool showBackground)) account.ShowBackground = showBackground;
            if (bool.TryParse(GetValue("Reviewed"), out bool reviewed)) account.Reviewed = reviewed;
            if (bool.TryParse(GetValue("HasAddedRepeating"), out bool hasAddedRepeating)) account.HasAddedRepeating = hasAddedRepeating;
            if (HasValue("ClassRemindersTimeSpan")) account.ClassRemindersTimeSpan = ParseNullableTimeSpan(document.Root.Elements().First(element => element.Name.LocalName == "ClassRemindersTimeSpan"));
            if (bool.TryParse(GetValue("RemindersDayBefore"), out bool remindersDayBefore)) account.RemindersDayBefore = remindersDayBefore;
            if (bool.TryParse(GetValue("RemindersDayOf"), out bool remindersDayOf)) account.RemindersDayOf = remindersDayOf;
            if (TryParseEnum(GetValue("ImageUploadOption"), out ImageUploadOptions uploadOption)) account.ImageUploadOption = uploadOption;
            if (bool.TryParse(GetValue("IsPushDisabled"), out bool pushDisabled)) account.IsPushDisabled = pushDisabled;

            XElement defaultGradeScale = document.Root?.Elements().FirstOrDefault(element => element.Name.LocalName == "_defaultGradeScale");
            if (defaultGradeScale != null) account.DefaultGradeScale = ParseGradeScales(defaultGradeScale);
            if (bool.TryParse(GetValue("DefaultDoesAverageGradeTotals"), out bool averageGradeTotals)) account.DefaultDoesAverageGradeTotals = averageGradeTotals;
            if (bool.TryParse(GetValue("DefaultDoesRoundGradesUp"), out bool roundGradesUp)) account.DefaultDoesRoundGradesUp = roundGradesUp;
            if (TryParseBytes(GetValue("CustomPrimaryThemeColor"), out byte[] primaryThemeColor)) account.CustomPrimaryThemeColor = primaryThemeColor;
            if (bool.TryParse(GetValue("_isSoundEffectsDisabled"), out bool soundEffectsDisabled)) account.IsSoundEffectsEnabled = !soundEffectsDisabled;
            if (TryParseBytes(GetValue("NoClassColor"), out byte[] noClassColor)) account.NoClassColor = noClassColor;

            XElement mainTileSettings = document.Root?.Elements().FirstOrDefault(element => element.Name.LocalName == "_mainTileSettings");
            if (mainTileSettings != null)
            {
                if (bool.TryParse(GetChildValue(mainTileSettings, "ShowTasks"), out bool showTasks)) account.MainTileSettings.ShowTasks = showTasks;
                if (bool.TryParse(GetChildValue(mainTileSettings, "ShowEvents"), out bool showEvents)) account.MainTileSettings.ShowEvents = showEvents;
                if (int.TryParse(GetChildValue(mainTileSettings, "SkipItemsOlderThan"), out int skipItemsOlderThan)) account.MainTileSettings.SkipItemsOlderThan = skipItemsOlderThan;
            }

            if (int.TryParse(GetValue("DeviceId"), out int deviceId)) account.DeviceId = deviceId;
            if (long.TryParse(GetValue("AccountId"), out long accountId)) account.AccountId = accountId;
            if (HasValue("Username")) account.Username = GetValue("Username");
            if (HasValue("Token")) account.LocalToken = GetValue("Token");
            if (HasValue("OnlineToken")) account.Token = GetValue("OnlineToken");
            if (HasValue("Password") && !string.IsNullOrEmpty(GetValue("Password"))) account.LocalToken = GetValue("Password");
            if (bool.TryParse(GetValue("RememberUsername"), out bool rememberUsername)) account.RememberUsername = rememberUsername;
            if (bool.TryParse(GetValue("RememberPassword"), out bool rememberPassword)) account.RememberPassword = rememberPassword;
            if (bool.TryParse(GetValue("AutoLogin"), out bool autoLogin)) account.AutoLogin = autoLogin;
            if (TryParseDateTime(GetValue("DateLastDayBeforeReminderWasSent"), out DateTime reminderDate)) account.PersistedDateLastDayBeforeReminderWasSent = reminderDate;
            account.CustomEndTimes = ParseCustomEndTimes(document.Root?.Elements().FirstOrDefault(element => element.Name.LocalName == "CustomEndTimes"));
            if (Guid.TryParse(GetValue("CurrentSemesterId"), out Guid semesterId)) account.CurrentSemesterId = semesterId;
            if (TryParseDateTime(GetValue("PremiumAccountExpiresOn"), out DateTime premiumExpiresOn)) account.PremiumAccountExpiresOn = premiumExpiresOn;

            return account;
        }

        private static string GetChildValue(XElement parent, string name) => parent.Elements().FirstOrDefault(element => element.Name.LocalName == name)?.Value;

        private static bool TryParseEnum<T>(string value, out T result) where T : struct
        {
            if (Enum.TryParse(value, out result)) return true;
            if (int.TryParse(value, out int numericValue))
            {
                result = (T)Enum.ToObject(typeof(T), numericValue);
                return true;
            }
            return false;
        }

        private static bool TryParseDateTime(string value, out DateTime result)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result);
        }

        private static TimeSpan? ParseNullableTimeSpan(XElement element)
        {
            XAttribute nil = element.Attributes().FirstOrDefault(attribute => attribute.Name.LocalName == "nil");
            if (nil?.Value == "true") return null;
            try { return XmlConvert.ToTimeSpan(element.Value); } catch { return null; }
        }

        private static bool TryParseBytes(string value, out byte[] result)
        {
            result = null;
            if (string.IsNullOrEmpty(value)) return false;
            try { result = Convert.FromBase64String(value); return true; } catch { return false; }
        }

        private static GradeScale[] ParseGradeScales(XElement parent)
        {
            return parent.Descendants().Where(element => element.Name.LocalName == "GradeScale").Select(element => new GradeScale
            {
                StartGrade = double.Parse(GetChildValue(element, "StartGrade"), CultureInfo.InvariantCulture),
                GPA = double.Parse(GetChildValue(element, "GPA"), CultureInfo.InvariantCulture)
            }).ToArray();
        }

        private static Dictionary<DayOfWeek, TimeSpan> ParseCustomEndTimes(XElement parent)
        {
            var result = new Dictionary<DayOfWeek, TimeSpan>();
            if (parent == null) return result;
            foreach (XElement item in parent.Elements())
            {
                if (TryParseEnum(GetChildValue(item, "Key"), out DayOfWeek day))
                {
                    try { result[day] = XmlConvert.ToTimeSpan(GetChildValue(item, "Value")); } catch { }
                }
            }
            return result;
        }

        private static bool TryParseVersion(XElement element, out Version version)
        {
            version = null;
            if (element == null) return false;
            if (Version.TryParse(element.Value, out version)) return true;
            string major = GetChildValue(element, "_Major");
            string minor = GetChildValue(element, "_Minor");
            string build = GetChildValue(element, "_Build");
            string revision = GetChildValue(element, "_Revision");
            return Version.TryParse($"{major ?? "0"}.{minor ?? "0"}.{build ?? "0"}.{revision ?? "0"}", out version);
        }
    }
}