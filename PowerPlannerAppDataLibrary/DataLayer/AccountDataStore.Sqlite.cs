using Dapper;
using Microsoft.Data.Sqlite;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public partial class AccountDataStore
    {
        private const string ClassColumns = "Identifier, DateCreated, Updated, UpperIdentifier, Name, Details, CourseNumber, ShouldAverageGradeTotals, DoesRoundGradesUp, Color, Position, GradeScales, Credits, OverriddenGPA, OverriddenGrade, StartDate, EndDate, GpaType, PassingGrade, LastTaskTimeOption, LastEventTimeOption, LastTaskDueTime, LastEventStartTime, LastEventDurationProperty, ImageNames";
        private const string MegaItemColumns = "Identifier, DateCreated, Updated, UpperIdentifier, Name, Details, ImageNames, Date, GradeReceived, GradeTotal, IsDropped, IndividualWeight, WeightCategoryIdentifier, EndTime, Reminder, PercentComplete, MegaItemType, AppointmentLocalId"
#if ANDROID
            + ", HasSentReminder"
#endif
            ;
        private const string GradeColumns = "Identifier, DateCreated, Updated, UpperIdentifier, Name, Details, ImageNames, Date, GradeReceived, GradeTotal, IsDropped, IndividualWeight";
        private const string ScheduleColumns = "Identifier, DateCreated, Updated, UpperIdentifier, DayOfWeek, StartTime, EndTime, Room, ScheduleType, ScheduleWeek, LocationLatitude, LocationLongitude, AppointmentLocalId, Name, Details, ImageNames";
        private const string SemesterColumns = "Identifier, DateCreated, Updated, UpperIdentifier, Name, Start, End, OverriddenGPA, OverriddenCredits, Details, ImageNames";
        private const string YearColumns = "Identifier, DateCreated, Updated, UpperIdentifier, Name, OverriddenGPA, OverriddenCredits, Details, ImageNames";
        private const string WeightCategoryColumns = "Identifier, DateCreated, Updated, UpperIdentifier, Name, WeightValue, Details, ImageNames";

        private void CreateSchema()
        {
            _db.Execute("""
                PRAGMA journal_mode=WAL;
                PRAGMA busy_timeout=5000;
                CREATE TABLE IF NOT EXISTS DataItemClass (
                    Identifier TEXT NOT NULL PRIMARY KEY, DateCreated INTEGER, Updated INTEGER, UpperIdentifier TEXT,
                    Name TEXT, Details TEXT, CourseNumber TEXT, ShouldAverageGradeTotals INTEGER, DoesRoundGradesUp INTEGER,
                    Color BLOB, Position INTEGER, GradeScales TEXT, Credits REAL, OverriddenGPA REAL, OverriddenGrade REAL,
                    StartDate INTEGER, EndDate INTEGER, GpaType INTEGER, PassingGrade REAL, LastTaskTimeOption TEXT,
                    LastEventTimeOption TEXT, LastTaskDueTime INTEGER, LastEventStartTime INTEGER,
                    LastEventDurationProperty INTEGER, ImageNames TEXT);
                CREATE TABLE IF NOT EXISTS DataItemMegaItem (
                    Identifier TEXT NOT NULL PRIMARY KEY, DateCreated INTEGER, Updated INTEGER, UpperIdentifier TEXT,
                    Name TEXT, Details TEXT, ImageNames TEXT, Date INTEGER, GradeReceived REAL, GradeTotal REAL,
                    IsDropped INTEGER, IndividualWeight REAL, WeightCategoryIdentifier TEXT, EndTime INTEGER,
                    Reminder INTEGER, PercentComplete REAL, MegaItemType INTEGER, AppointmentLocalId TEXT);
                CREATE TABLE IF NOT EXISTS DataItemGrade (
                    Identifier TEXT NOT NULL PRIMARY KEY, DateCreated INTEGER, Updated INTEGER, UpperIdentifier TEXT,
                    Name TEXT, Details TEXT, ImageNames TEXT, Date INTEGER, GradeReceived REAL, GradeTotal REAL,
                    IsDropped INTEGER, IndividualWeight REAL);
                CREATE TABLE IF NOT EXISTS DataItemSchedule (
                    Identifier TEXT NOT NULL PRIMARY KEY, DateCreated INTEGER, Updated INTEGER, UpperIdentifier TEXT,
                    DayOfWeek INTEGER, StartTime INTEGER, EndTime INTEGER, Room TEXT, ScheduleType INTEGER,
                    ScheduleWeek INTEGER, LocationLatitude REAL, LocationLongitude REAL, AppointmentLocalId TEXT,
                    Name TEXT, Details TEXT, ImageNames TEXT);
                CREATE TABLE IF NOT EXISTS DataItemSemester (
                    Identifier TEXT NOT NULL PRIMARY KEY, DateCreated INTEGER, Updated INTEGER, UpperIdentifier TEXT,
                    Name TEXT, Start INTEGER, End INTEGER, OverriddenGPA REAL, OverriddenCredits REAL, Details TEXT,
                    ImageNames TEXT);
                CREATE TABLE IF NOT EXISTS DataItemYear (
                    Identifier TEXT NOT NULL PRIMARY KEY, DateCreated INTEGER, Updated INTEGER, UpperIdentifier TEXT NOT NULL,
                    Name TEXT,
                    OverriddenGPA REAL, OverriddenCredits REAL, Details TEXT, ImageNames TEXT);
                CREATE TABLE IF NOT EXISTS DataItemWeightCategory (
                    Identifier TEXT NOT NULL PRIMARY KEY, DateCreated INTEGER, Updated INTEGER, UpperIdentifier TEXT,
                    Name TEXT, WeightValue REAL, Details TEXT, ImageNames TEXT);
                CREATE TABLE IF NOT EXISTS ImageToUpload (FileName TEXT NOT NULL PRIMARY KEY);
                CREATE TABLE IF NOT EXISTS DataInfo (Key INTEGER NOT NULL PRIMARY KEY, Version INTEGER NOT NULL);
                CREATE INDEX IF NOT EXISTS Index_DataItemClass_Identifier ON DataItemClass (Identifier);
                CREATE INDEX IF NOT EXISTS Index_DataItemClass_UpperIdentifier ON DataItemClass (UpperIdentifier);
                CREATE INDEX IF NOT EXISTS Index_DataItemMegaItem_Identifier ON DataItemMegaItem (Identifier);
                CREATE INDEX IF NOT EXISTS Index_DataItemMegaItem_UpperIdentifier ON DataItemMegaItem (UpperIdentifier);
                CREATE INDEX IF NOT EXISTS Index_DataItemGrade_Identifier ON DataItemGrade (Identifier);
                CREATE INDEX IF NOT EXISTS Index_DataItemGrade_UpperIdentifier ON DataItemGrade (UpperIdentifier);
                CREATE INDEX IF NOT EXISTS Index_DataItemSchedule_Identifier ON DataItemSchedule (Identifier);
                CREATE INDEX IF NOT EXISTS Index_DataItemSchedule_UpperIdentifier ON DataItemSchedule (UpperIdentifier);
                CREATE INDEX IF NOT EXISTS Index_DataItemSemester_Identifier ON DataItemSemester (Identifier);
                CREATE INDEX IF NOT EXISTS Index_DataItemSemester_UpperIdentifier ON DataItemSemester (UpperIdentifier);
                CREATE INDEX IF NOT EXISTS Index_DataItemWeightCategory_Identifier ON DataItemWeightCategory (Identifier);
                CREATE INDEX IF NOT EXISTS Index_DataItemWeightCategory_UpperIdentifier ON DataItemWeightCategory (UpperIdentifier);
                CREATE INDEX IF NOT EXISTS Index_DataItemYear_Identifier ON DataItemYear (Identifier);
                """);
#if ANDROID
            EnsureColumn("DataItemMegaItem", "HasSentReminder", "INTEGER");
#endif
            EnsureColumn("DataItemClass", "StartDate", "INTEGER");
            EnsureColumn("DataItemClass", "EndDate", "INTEGER");
            EnsureColumn("DataItemClass", "GpaType", "INTEGER");
            EnsureColumn("DataItemClass", "PassingGrade", "REAL");
            EnsureColumn("DataItemClass", "LastTaskTimeOption", "TEXT");
            EnsureColumn("DataItemClass", "LastEventTimeOption", "TEXT");
            EnsureColumn("DataItemClass", "LastTaskDueTime", "INTEGER");
            EnsureColumn("DataItemClass", "LastEventStartTime", "INTEGER");
            EnsureColumn("DataItemClass", "LastEventDurationProperty", "INTEGER");
            EnsureColumn("DataItemYear", "UpperIdentifier", "TEXT");
        }

        private void EnsureColumn(string tableName, string columnName, string columnType)
        {
            using (var command = _db.CreateCommand())
            {
                command.CommandText = $"PRAGMA table_info([{tableName}])";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                            return;
                    }
                }

                command.CommandText = $"ALTER TABLE [{tableName}] ADD COLUMN [{columnName}] {columnType}";
                command.ExecuteNonQuery();
            }
        }

        private static Guid ReadGuid(string value)
        {
            return string.IsNullOrEmpty(value) ? Guid.Empty : Guid.Parse(value);
        }

        private static DateTime ReadDateTime(long? value)
        {
            return new DateTime(value ?? SqlDate.MinValue.Ticks, DateTimeKind.Utc);
        }

        private static TimeSpan? ReadTimeSpan(long? value)
        {
            return value.HasValue ? new TimeSpan(value.Value) : null;
        }

        private static string CreateIdentifierSet(IEnumerable<Guid> identifiers)
        {
            return "|" + string.Join("|", identifiers.Select(identifier => identifier.ToString())) + "|";
        }

        private T ApplyBase<T>(T item, BaseRow row) where T : BaseDataItem
        {
            item.Identifier = ReadGuid(row.Identifier);
            item.DateCreated = ReadDateTime(row.DateCreated);
            item.Updated = ReadDateTime(row.Updated);
            item.Account = Account;
            return item;
        }

        private void ApplyUnderOne(BaseDataItemUnderOne item, UnderOneRow row)
        {
            ApplyBase(item, row);
            item.UpperIdentifier = ReadGuid(row.UpperIdentifier);
        }

        private void ApplyWithImages(BaseDataItemWithImages item, WithImagesRow row)
        {
            ApplyUnderOne(item, row);
            item.Name = row.Name ?? string.Empty;
            item.Details = row.Details ?? string.Empty;
            item.RawImageNames = row.ImageNames;
        }

        private DataItemClass ToEntity(ClassRow row)
        {
            var item = new DataItemClass();
            ApplyWithImages(item, row);
            item.CourseNumber = row.CourseNumber ?? string.Empty;
            item.ShouldAverageGradeTotals = row.ShouldAverageGradeTotals != 0;
            item.DoesRoundGradesUp = row.DoesRoundGradesUp != 0;
            item.RawColor = row.Color;
            item.Position = (byte)(row.Position ?? 0);
            item.RawGradeScales = row.GradeScales;
            item.Credits = row.Credits ?? Grade.NO_CREDITS;
            item.OverriddenGPA = row.OverriddenGPA ?? Grade.UNGRADED;
            item.OverriddenGrade = row.OverriddenGrade ?? Grade.UNGRADED;
            item.StartDate = ReadDateTime(row.StartDate);
            item.EndDate = ReadDateTime(row.EndDate);
            item.GpaType = (GpaType)(row.GpaType ?? (long)GpaType.Standard);
            item.PassingGrade = row.PassingGrade ?? Class.DefaultPassingGrade;
            item.LastTaskTimeOption = row.LastTaskTimeOption;
            item.LastEventTimeOption = row.LastEventTimeOption;
            item.LastTaskDueTime = ReadTimeSpan(row.LastTaskDueTime);
            item.LastEventStartTime = ReadTimeSpan(row.LastEventStartTime);
            item.LastEventDuration = ReadTimeSpan(row.LastEventDurationProperty);
            return item;
        }

        private DataItemMegaItem ToEntity(MegaItemRow row)
        {
            var item = new DataItemMegaItem();
            ApplyWithImages(item, row);
            item.Date = ReadDateTime(row.Date);
            item.GradeReceived = row.GradeReceived ?? Grade.UNGRADED;
            item.GradeTotal = row.GradeTotal ?? 100;
            item.IsDropped = row.IsDropped != 0;
            item.IndividualWeight = row.IndividualWeight ?? 1;
            item.WeightCategoryIdentifier = ReadGuid(row.WeightCategoryIdentifier);
            item.EndTime = ReadDateTime(row.EndTime);
            item.Reminder = ReadDateTime(row.Reminder);
            item.PercentComplete = row.PercentComplete ?? 0;
            item.MegaItemType = (MegaItemType)(row.MegaItemType ?? 0);
            item.AppointmentLocalId = row.AppointmentLocalId;
#if ANDROID
            item.HasSentReminder = row.HasSentReminder != 0;
#endif
            return item;
        }

        private DataItemGrade ToEntity(GradeRow row)
        {
            var item = new DataItemGrade();
            ApplyWithImages(item, row);
            item.Date = ReadDateTime(row.Date);
            item.GradeReceived = row.GradeReceived ?? Grade.UNGRADED;
            item.GradeTotal = row.GradeTotal ?? 100;
            item.IsDropped = row.IsDropped != 0;
            item.IndividualWeight = row.IndividualWeight ?? 1;
            return item;
        }

        private DataItemSchedule ToEntity(ScheduleRow row)
        {
            var item = new DataItemSchedule();
            ApplyWithImages(item, row);
            item.DayOfWeek = (DayOfWeek)(row.DayOfWeek ?? 0);
            item.StartTime = ReadDateTime(row.StartTime);
            item.EndTime = ReadDateTime(row.EndTime);
            item.Room = row.Room ?? string.Empty;
            item.ScheduleType = (Schedule.Type)(row.ScheduleType ?? 0);
            item.ScheduleWeek = (Schedule.Week)(row.ScheduleWeek ?? 0);
            item.LocationLatitude = row.LocationLatitude ?? 0;
            item.LocationLongitude = row.LocationLongitude ?? 0;
            item.AppointmentLocalId = row.AppointmentLocalId;
            return item;
        }

        private DataItemSemester ToEntity(SemesterRow row)
        {
            var item = new DataItemSemester();
            ApplyWithImages(item, row);
            item.Start = ReadDateTime(row.Start);
            item.End = ReadDateTime(row.End);
            item.OverriddenGPA = row.OverriddenGPA ?? Grade.UNGRADED;
            item.OverriddenCredits = row.OverriddenCredits ?? Grade.UNGRADED;
            return item;
        }

        private DataItemYear ToEntity(YearRow row)
        {
            var item = new DataItemYear();
            ApplyWithImages(item, row);
            item.OverriddenGPA = row.OverriddenGPA ?? Grade.UNGRADED;
            item.OverriddenCredits = row.OverriddenCredits ?? Grade.UNGRADED;
            return item;
        }

        private DataItemWeightCategory ToEntity(WeightCategoryRow row)
        {
            var item = new DataItemWeightCategory();
            ApplyWithImages(item, row);
            item.WeightValue = row.WeightValue ?? 0;
            return item;
        }

        private DataItemClass[] LoadClasses(SqliteTransaction transaction = null)
        {
            return _db.Query<ClassRow>("SELECT Identifier, DateCreated, Updated, UpperIdentifier, Name, Details, CourseNumber, ShouldAverageGradeTotals, DoesRoundGradesUp, Color, Position, GradeScales, Credits, OverriddenGPA, OverriddenGrade, StartDate, EndDate, GpaType, PassingGrade, LastTaskTimeOption, LastEventTimeOption, LastTaskDueTime, LastEventStartTime, LastEventDurationProperty, ImageNames FROM DataItemClass", transaction: transaction).Select(ToEntity).ToArray();
        }

        private DataItemMegaItem[] LoadMegaItems(SqliteTransaction transaction = null)
        {
            return _db.Query<MegaItemRow>("SELECT Identifier, DateCreated, Updated, UpperIdentifier, Name, Details, ImageNames, Date, GradeReceived, GradeTotal, IsDropped, IndividualWeight, WeightCategoryIdentifier, EndTime, Reminder, PercentComplete, MegaItemType, AppointmentLocalId"
#if ANDROID
                + ", HasSentReminder"
#endif
                + " FROM DataItemMegaItem", transaction: transaction).Select(ToEntity).ToArray();
        }

        private DataItemGrade[] LoadGrades(SqliteTransaction transaction = null)
        {
            return _db.Query<GradeRow>("SELECT Identifier, DateCreated, Updated, UpperIdentifier, Name, Details, ImageNames, Date, GradeReceived, GradeTotal, IsDropped, IndividualWeight FROM DataItemGrade", transaction: transaction).Select(ToEntity).ToArray();
        }

        private DataItemSchedule[] LoadSchedules(SqliteTransaction transaction = null)
        {
            return _db.Query<ScheduleRow>("SELECT Identifier, DateCreated, Updated, UpperIdentifier, DayOfWeek, StartTime, EndTime, Room, ScheduleType, ScheduleWeek, LocationLatitude, LocationLongitude, AppointmentLocalId, Name, Details, ImageNames FROM DataItemSchedule", transaction: transaction).Select(ToEntity).ToArray();
        }

        private DataItemSemester[] LoadSemesters(SqliteTransaction transaction = null)
        {
            return _db.Query<SemesterRow>("SELECT Identifier, DateCreated, Updated, UpperIdentifier, Name, Start, End, OverriddenGPA, OverriddenCredits, Details, ImageNames FROM DataItemSemester", transaction: transaction).Select(ToEntity).ToArray();
        }

        private DataItemYear[] LoadYears(SqliteTransaction transaction = null)
        {
            return _db.Query<YearRow>("SELECT Identifier, DateCreated, Updated, UpperIdentifier, Name, OverriddenGPA, OverriddenCredits, Details, ImageNames FROM DataItemYear", transaction: transaction).Select(ToEntity).ToArray();
        }

        private DataItemWeightCategory[] LoadWeightCategories(SqliteTransaction transaction = null)
        {
            return _db.Query<WeightCategoryRow>("SELECT Identifier, DateCreated, Updated, UpperIdentifier, Name, WeightValue, Details, ImageNames FROM DataItemWeightCategory", transaction: transaction).Select(ToEntity).ToArray();
        }

        private void UpsertItem(BaseDataItem item, SqliteTransaction transaction = null)
        {
            switch (item)
            {
                case DataItemClass value:
                    _db.Execute($"INSERT OR REPLACE INTO DataItemClass ({ClassColumns}) VALUES (@Identifier, @DateCreated, @Updated, @UpperIdentifier, @Name, @Details, @CourseNumber, @ShouldAverageGradeTotals, @DoesRoundGradesUp, @Color, @Position, @GradeScales, @Credits, @OverriddenGPA, @OverriddenGrade, @StartDate, @EndDate, @GpaType, @PassingGrade, @LastTaskTimeOption, @LastEventTimeOption, @LastTaskDueTime, @LastEventStartTime, @LastEventDurationProperty, @ImageNames)", new { Identifier = value.Identifier.ToString(), DateCreated = value.DateCreated.Ticks, Updated = value.Updated.Ticks, UpperIdentifier = value.UpperIdentifier.ToString(), value.Name, value.Details, value.CourseNumber, ShouldAverageGradeTotals = value.ShouldAverageGradeTotals ? 1 : 0, DoesRoundGradesUp = value.DoesRoundGradesUp ? 1 : 0, Color = value.RawColor, Position = (long)value.Position, GradeScales = value.RawGradeScales, value.Credits, value.OverriddenGPA, value.OverriddenGrade, StartDate = value.StartDate.Ticks, EndDate = value.EndDate.Ticks, GpaType = (long)value.GpaType, value.PassingGrade, value.LastTaskTimeOption, value.LastEventTimeOption, LastTaskDueTime = value.LastTaskDueTime?.Ticks, LastEventStartTime = value.LastEventStartTime?.Ticks, LastEventDurationProperty = value.LastEventDuration?.Ticks, ImageNames = value.RawImageNames }, transaction);
                    break;
                case DataItemMegaItem value:
                    _db.Execute($"INSERT OR REPLACE INTO DataItemMegaItem ({MegaItemColumns}) VALUES (@Identifier, @DateCreated, @Updated, @UpperIdentifier, @Name, @Details, @ImageNames, @Date, @GradeReceived, @GradeTotal, @IsDropped, @IndividualWeight, @WeightCategoryIdentifier, @EndTime, @Reminder, @PercentComplete, @MegaItemType, @AppointmentLocalId"
#if ANDROID
                        + ", @HasSentReminder"
#endif
                        + ")", new { Identifier = value.Identifier.ToString(), DateCreated = value.DateCreated.Ticks, Updated = value.Updated.Ticks, UpperIdentifier = value.UpperIdentifier.ToString(), value.Name, value.Details, ImageNames = value.RawImageNames, Date = value.Date.Ticks, value.GradeReceived, value.GradeTotal, IsDropped = value.IsDropped ? 1 : 0, value.IndividualWeight, WeightCategoryIdentifier = value.WeightCategoryIdentifier.ToString(), EndTime = value.EndTime.Ticks, Reminder = value.Reminder.Ticks, value.PercentComplete, MegaItemType = (long)value.MegaItemType, value.AppointmentLocalId
#if ANDROID
                            , HasSentReminder = value.HasSentReminder ? 1 : 0
#endif
                        }, transaction);
                    break;
                case DataItemGrade value:
                    _db.Execute($"INSERT OR REPLACE INTO DataItemGrade ({GradeColumns}) VALUES (@Identifier, @DateCreated, @Updated, @UpperIdentifier, @Name, @Details, @ImageNames, @Date, @GradeReceived, @GradeTotal, @IsDropped, @IndividualWeight)", new { Identifier = value.Identifier.ToString(), DateCreated = value.DateCreated.Ticks, Updated = value.Updated.Ticks, UpperIdentifier = value.UpperIdentifier.ToString(), value.Name, value.Details, ImageNames = value.RawImageNames, Date = value.Date.Ticks, value.GradeReceived, value.GradeTotal, IsDropped = value.IsDropped ? 1 : 0, value.IndividualWeight }, transaction);
                    break;
                case DataItemSchedule value:
                    _db.Execute($"INSERT OR REPLACE INTO DataItemSchedule ({ScheduleColumns}) VALUES (@Identifier, @DateCreated, @Updated, @UpperIdentifier, @DayOfWeek, @StartTime, @EndTime, @Room, @ScheduleType, @ScheduleWeek, @LocationLatitude, @LocationLongitude, @AppointmentLocalId, @Name, @Details, @ImageNames)", new { Identifier = value.Identifier.ToString(), DateCreated = value.DateCreated.Ticks, Updated = value.Updated.Ticks, UpperIdentifier = value.UpperIdentifier.ToString(), DayOfWeek = (long)value.DayOfWeek, StartTime = value.StartTime.Ticks, EndTime = value.EndTime.Ticks, value.Room, ScheduleType = (long)value.ScheduleType, ScheduleWeek = (long)value.ScheduleWeek, value.LocationLatitude, value.LocationLongitude, value.AppointmentLocalId, value.Name, value.Details, ImageNames = value.RawImageNames }, transaction);
                    break;
                case DataItemSemester value:
                    _db.Execute($"INSERT OR REPLACE INTO DataItemSemester ({SemesterColumns}) VALUES (@Identifier, @DateCreated, @Updated, @UpperIdentifier, @Name, @Start, @End, @OverriddenGPA, @OverriddenCredits, @Details, @ImageNames)", new { Identifier = value.Identifier.ToString(), DateCreated = value.DateCreated.Ticks, Updated = value.Updated.Ticks, UpperIdentifier = value.UpperIdentifier.ToString(), value.Name, Start = value.Start.Ticks, End = value.End.Ticks, value.OverriddenGPA, value.OverriddenCredits, value.Details, ImageNames = value.RawImageNames }, transaction);
                    break;
                case DataItemYear value:
                    _db.Execute($"INSERT OR REPLACE INTO DataItemYear ({YearColumns}) VALUES (@Identifier, @DateCreated, @Updated, @UpperIdentifier, @Name, @OverriddenGPA, @OverriddenCredits, @Details, @ImageNames)", new { Identifier = value.Identifier.ToString(), DateCreated = value.DateCreated.Ticks, Updated = value.Updated.Ticks, UpperIdentifier = value.UpperIdentifier.ToString(), value.Name, value.OverriddenGPA, value.OverriddenCredits, value.Details, ImageNames = value.RawImageNames }, transaction);
                    break;
                case DataItemWeightCategory value:
                    _db.Execute($"INSERT OR REPLACE INTO DataItemWeightCategory ({WeightCategoryColumns}) VALUES (@Identifier, @DateCreated, @Updated, @UpperIdentifier, @Name, @WeightValue, @Details, @ImageNames)", new { Identifier = value.Identifier.ToString(), DateCreated = value.DateCreated.Ticks, Updated = value.Updated.Ticks, UpperIdentifier = value.UpperIdentifier.ToString(), value.Name, value.WeightValue, value.Details, ImageNames = value.RawImageNames }, transaction);
                    break;
            }
        }

        internal class BaseRow
        {
            public string Identifier { get; set; }
            public long? DateCreated { get; set; }
            public long? Updated { get; set; }
        }

        internal class UnderOneRow : BaseRow
        {
            public string UpperIdentifier { get; set; }
        }

        internal class WithImagesRow : UnderOneRow
        {
            public string Name { get; set; }
            public string Details { get; set; }
            public string ImageNames { get; set; }
        }

        internal sealed class ClassRow : WithImagesRow
        {
            public string CourseNumber { get; set; }
            public long? ShouldAverageGradeTotals { get; set; }
            public long? DoesRoundGradesUp { get; set; }
            public byte[] Color { get; set; }
            public long? Position { get; set; }
            public string GradeScales { get; set; }
            public double? Credits { get; set; }
            public double? OverriddenGPA { get; set; }
            public double? OverriddenGrade { get; set; }
            public long? StartDate { get; set; }
            public long? EndDate { get; set; }
            public long? GpaType { get; set; }
            public double? PassingGrade { get; set; }
            public string LastTaskTimeOption { get; set; }
            public string LastEventTimeOption { get; set; }
            public long? LastTaskDueTime { get; set; }
            public long? LastEventStartTime { get; set; }
            public long? LastEventDurationProperty { get; set; }
        }

        internal sealed class MegaItemRow : WithImagesRow
        {
            public long? Date { get; set; }
            public double? GradeReceived { get; set; }
            public double? GradeTotal { get; set; }
            public long? IsDropped { get; set; }
            public double? IndividualWeight { get; set; }
            public string WeightCategoryIdentifier { get; set; }
            public long? EndTime { get; set; }
            public long? Reminder { get; set; }
            public double? PercentComplete { get; set; }
            public long? MegaItemType { get; set; }
            public string AppointmentLocalId { get; set; }
#if ANDROID
            public long? HasSentReminder { get; set; }
#endif
        }

        internal sealed class GradeRow : WithImagesRow
        {
            public long? Date { get; set; }
            public double? GradeReceived { get; set; }
            public double? GradeTotal { get; set; }
            public long? IsDropped { get; set; }
            public double? IndividualWeight { get; set; }
        }

        internal sealed class ScheduleRow : WithImagesRow
        {
            public long? DayOfWeek { get; set; }
            public long? StartTime { get; set; }
            public long? EndTime { get; set; }
            public string Room { get; set; }
            public long? ScheduleType { get; set; }
            public long? ScheduleWeek { get; set; }
            public double? LocationLatitude { get; set; }
            public double? LocationLongitude { get; set; }
            public string AppointmentLocalId { get; set; }
        }

        internal sealed class SemesterRow : WithImagesRow
        {
            public long? Start { get; set; }
            public long? End { get; set; }
            public double? OverriddenGPA { get; set; }
            public double? OverriddenCredits { get; set; }
        }

        internal sealed class YearRow : WithImagesRow
        {
            public double? OverriddenGPA { get; set; }
            public double? OverriddenCredits { get; set; }
        }

        internal sealed class WeightCategoryRow : WithImagesRow
        {
            public double? WeightValue { get; set; }
        }
    }
}