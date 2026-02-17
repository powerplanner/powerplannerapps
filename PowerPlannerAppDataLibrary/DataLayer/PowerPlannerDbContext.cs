using Microsoft.EntityFrameworkCore;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.IO;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    /// <summary>
    /// Entity Framework Core DbContext for Power Planner database.
    /// Replaces the legacy sqlite-net-pcl implementation.
    /// </summary>
    public class PowerPlannerDbContext : DbContext
    {
        private readonly string _databasePath;
        public AccountDataItem Account { get; private set; }

        public PowerPlannerDbContext(string databasePath, AccountDataItem account)
        {
            _databasePath = databasePath;
            Account = account;
        }

        // DbSets for all entity types
        public DbSet<DataItemClass> Classes { get; set; }
        public DbSet<DataItemMegaItem> MegaItems { get; set; }
        public DbSet<DataItemGrade> Grades { get; set; }
        public DbSet<DataItemSchedule> Schedules { get; set; }
        public DbSet<DataItemSemester> Semesters { get; set; }
        public DbSet<DataItemWeightCategory> WeightCategories { get; set; }
        public DbSet<DataItemYear> Years { get; set; }
        public DbSet<ImageToUpload> ImagesToUpload { get; set; }
        public DbSet<AccountDataStore.DataInfo> DataInfos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={_databasePath}");
                
                // Performance optimizations
                optionsBuilder.EnableSensitiveDataLogging(false);
                optionsBuilder.EnableDetailedErrors(false);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure DataItemClass
            modelBuilder.Entity<DataItemClass>(entity =>
            {
                entity.ToTable("DataItemClass");
                entity.HasKey(e => e.Identifier);
                entity.HasIndex(e => e.Identifier).HasDatabaseName("Index_DataItemClass_Identifier");
                entity.HasIndex(e => e.UpperIdentifier).HasDatabaseName("Index_DataItemClass_UpperIdentifier");
                
                entity.Property(e => e.Identifier).HasColumnName("Identifier");
                entity.Property(e => e.DateCreated).HasColumnName("DateCreated");
                entity.Property(e => e.Updated).HasColumnName("Updated");
                entity.Property(e => e.UpperIdentifier).HasColumnName("UpperIdentifier");
                entity.Property(e => e.Name).HasColumnName("Name");
                entity.Property(e => e.Details).HasColumnName("Details");
                entity.Property(e => e.CourseNumber).HasColumnName("CourseNumber");
                entity.Property(e => e.ShouldAverageGradeTotals).HasColumnName("ShouldAverageGradeTotals");
                entity.Property(e => e.DoesRoundGradesUp).HasColumnName("DoesRoundGradesUp");
                entity.Property(e => e.RawColor).HasColumnName("Color");
                entity.Property(e => e.Position).HasColumnName("Position");
                entity.Property(e => e.RawGradeScales).HasColumnName("GradeScales");
                entity.Property(e => e.Credits).HasColumnName("Credits");
                entity.Property(e => e.OverriddenGPA).HasColumnName("OverriddenGPA");
                entity.Property(e => e.OverriddenGrade).HasColumnName("OverriddenGrade");
                entity.Property(e => e.StartDate).HasColumnName("StartDate");
                entity.Property(e => e.EndDate).HasColumnName("EndDate");
                entity.Property(e => e.GpaType).HasColumnName("GpaType");
                entity.Property(e => e.PassingGrade).HasColumnName("PassingGrade");
                entity.Property(e => e.LastTaskTimeOption).HasColumnName("LastTaskTimeOption");
                entity.Property(e => e.LastEventTimeOption).HasColumnName("LastEventTimeOption");
                entity.Property(e => e.LastTaskDueTime).HasColumnName("LastTaskDueTime");
                entity.Property(e => e.LastEventStartTime).HasColumnName("LastEventStartTime");
                entity.Property(e => e.LastEventDuration).HasColumnName("LastEventDurationProperty");
                entity.Property(e => e.RawImageNames).HasColumnName("ImageNames");

                // Ignore navigation properties that aren't stored in database
                entity.Ignore(e => e.Account);
            });

            // Configure DataItemSemester
            modelBuilder.Entity<DataItemSemester>(entity =>
            {
                entity.ToTable("DataItemSemester");
                entity.HasKey(e => e.Identifier);
                entity.HasIndex(e => e.Identifier).HasDatabaseName("Index_DataItemSemester_Identifier");
                entity.HasIndex(e => e.UpperIdentifier).HasDatabaseName("Index_DataItemSemester_UpperIdentifier");
                
                entity.Property(e => e.Identifier).HasColumnName("Identifier");
                entity.Property(e => e.DateCreated).HasColumnName("DateCreated");
                entity.Property(e => e.Updated).HasColumnName("Updated");
                entity.Property(e => e.UpperIdentifier).HasColumnName("UpperIdentifier");
                entity.Property(e => e.Name).HasColumnName("Name");
                entity.Property(e => e.Start).HasColumnName("Start");
                entity.Property(e => e.End).HasColumnName("End");
                entity.Property(e => e.OverriddenGPA).HasColumnName("OverriddenGPA");
                entity.Property(e => e.OverriddenCredits).HasColumnName("OverriddenCredits");

                entity.Ignore(e => e.Account);
            });

            // Configure DataItemYear
            modelBuilder.Entity<DataItemYear>(entity =>
            {
                entity.ToTable("DataItemYear");
                entity.HasKey(e => e.Identifier);
                entity.HasIndex(e => e.Identifier).HasDatabaseName("Index_DataItemYear_Identifier");
                
                entity.Property(e => e.Identifier).HasColumnName("Identifier");
                entity.Property(e => e.DateCreated).HasColumnName("DateCreated");
                entity.Property(e => e.Updated).HasColumnName("Updated");
                entity.Property(e => e.Name).HasColumnName("Name");
                entity.Property(e => e.OverriddenGPA).HasColumnName("OverriddenGPA");
                entity.Property(e => e.OverriddenCredits).HasColumnName("OverriddenCredits");

                entity.Ignore(e => e.Account);
            });

            // Configure DataItemGrade
            modelBuilder.Entity<DataItemGrade>(entity =>
            {
                entity.ToTable("DataItemGrade");
                entity.HasKey(e => e.Identifier);
                entity.HasIndex(e => e.Identifier).HasDatabaseName("Index_DataItemGrade_Identifier");
                entity.HasIndex(e => e.UpperIdentifier).HasDatabaseName("Index_DataItemGrade_UpperIdentifier");
                
                entity.Property(e => e.Identifier).HasColumnName("Identifier");
                entity.Property(e => e.DateCreated).HasColumnName("DateCreated");
                entity.Property(e => e.Updated).HasColumnName("Updated");
                entity.Property(e => e.UpperIdentifier).HasColumnName("UpperIdentifier");
                entity.Property(e => e.Name).HasColumnName("Name");
                entity.Property(e => e.Details).HasColumnName("Details");
                entity.Property(e => e.RawImageNames).HasColumnName("ImageNames");
                entity.Property(e => e.Date).HasColumnName("Date");
                entity.Property(e => e.GradeReceived).HasColumnName("GradeReceived");
                entity.Property(e => e.GradeTotal).HasColumnName("GradeTotal");
                entity.Property(e => e.IsDropped).HasColumnName("IsDropped");
                entity.Property(e => e.IndividualWeight).HasColumnName("IndividualWeight");

                entity.Ignore(e => e.Account);
            });

            // Configure DataItemMegaItem
            modelBuilder.Entity<DataItemMegaItem>(entity =>
            {
                entity.ToTable("DataItemMegaItem");
                entity.HasKey(e => e.Identifier);
                entity.HasIndex(e => e.Identifier).HasDatabaseName("Index_DataItemMegaItem_Identifier");
                entity.HasIndex(e => e.UpperIdentifier).HasDatabaseName("Index_DataItemMegaItem_UpperIdentifier");
                
                entity.Property(e => e.Identifier).HasColumnName("Identifier");
                entity.Property(e => e.DateCreated).HasColumnName("DateCreated");
                entity.Property(e => e.Updated).HasColumnName("Updated");
                entity.Property(e => e.UpperIdentifier).HasColumnName("UpperIdentifier");
                entity.Property(e => e.Name).HasColumnName("Name");
                entity.Property(e => e.Details).HasColumnName("Details");
                entity.Property(e => e.RawImageNames).HasColumnName("ImageNames");
                entity.Property(e => e.Date).HasColumnName("Date");
                entity.Property(e => e.GradeReceived).HasColumnName("GradeReceived");
                entity.Property(e => e.GradeTotal).HasColumnName("GradeTotal");
                entity.Property(e => e.IsDropped).HasColumnName("IsDropped");
                entity.Property(e => e.IndividualWeight).HasColumnName("IndividualWeight");
                entity.Property(e => e.WeightCategoryIdentifier).HasColumnName("WeightCategoryIdentifier");
                entity.Property(e => e.EndTime).HasColumnName("EndTime");
                entity.Property(e => e.Reminder).HasColumnName("Reminder");
                entity.Property(e => e.PercentComplete).HasColumnName("PercentComplete");
                entity.Property(e => e.MegaItemType).HasColumnName("MegaItemType");
                entity.Property(e => e.AppointmentLocalId).HasColumnName("AppointmentLocalId");
#if ANDROID
                entity.Property(e => e.HasSentReminder).HasColumnName("HasSentReminder");
#endif

                entity.Ignore(e => e.Account);
            });

            // Configure DataItemSchedule
            modelBuilder.Entity<DataItemSchedule>(entity =>
            {
                entity.ToTable("DataItemSchedule");
                entity.HasKey(e => e.Identifier);
                entity.HasIndex(e => e.Identifier).HasDatabaseName("Index_DataItemSchedule_Identifier");
                entity.HasIndex(e => e.UpperIdentifier).HasDatabaseName("Index_DataItemSchedule_UpperIdentifier");
                
                entity.Property(e => e.Identifier).HasColumnName("Identifier");
                entity.Property(e => e.DateCreated).HasColumnName("DateCreated");
                entity.Property(e => e.Updated).HasColumnName("Updated");
                entity.Property(e => e.UpperIdentifier).HasColumnName("UpperIdentifier");
                entity.Property(e => e.DayOfWeek).HasColumnName("DayOfWeek");
                entity.Property(e => e.StartTime).HasColumnName("StartTime");
                entity.Property(e => e.EndTime).HasColumnName("EndTime");
                entity.Property(e => e.Room).HasColumnName("Room");
                entity.Property(e => e.ScheduleType).HasColumnName("ScheduleType");
                entity.Property(e => e.ScheduleWeek).HasColumnName("ScheduleWeek");
                entity.Property(e => e.LocationLatitude).HasColumnName("LocationLatitude");
                entity.Property(e => e.LocationLongitude).HasColumnName("LocationLongitude");
                entity.Property(e => e.AppointmentLocalId).HasColumnName("AppointmentLocalId");

                entity.Ignore(e => e.Account);
            });

            // Configure DataItemWeightCategory
            modelBuilder.Entity<DataItemWeightCategory>(entity =>
            {
                entity.ToTable("DataItemWeightCategory");
                entity.HasKey(e => e.Identifier);
                entity.HasIndex(e => e.Identifier).HasDatabaseName("Index_DataItemWeightCategory_Identifier");
                entity.HasIndex(e => e.UpperIdentifier).HasDatabaseName("Index_DataItemWeightCategory_UpperIdentifier");
                
                entity.Property(e => e.Identifier).HasColumnName("Identifier");
                entity.Property(e => e.DateCreated).HasColumnName("DateCreated");
                entity.Property(e => e.Updated).HasColumnName("Updated");
                entity.Property(e => e.UpperIdentifier).HasColumnName("UpperIdentifier");
                entity.Property(e => e.Name).HasColumnName("Name");
                entity.Property(e => e.WeightValue).HasColumnName("WeightValue");

                entity.Ignore(e => e.Account);
            });

            // Configure ImageToUpload
            modelBuilder.Entity<ImageToUpload>(entity =>
            {
                entity.ToTable("ImageToUpload");
                entity.HasKey(e => e.FileName);
            });

            // Configure DataInfo
            modelBuilder.Entity<AccountDataStore.DataInfo>(entity =>
            {
                entity.ToTable("DataInfo");
                entity.HasKey(e => e.Key);
            });
        }

        /// <summary>
        /// Helper method to apply account to entities after they're loaded
        /// </summary>
        public T ApplyAccount<T>(T entity) where T : BaseDataItem
        {
            if (entity != null)
            {
                entity.Account = Account;
            }
            return entity;
        }
    }
}
