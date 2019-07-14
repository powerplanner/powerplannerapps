using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems
{
    public class DataItemClass : BaseDataItemWithImages, IComparable<DataItemClass>
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Class; }
        }

        

        public static readonly DataItemProperty CourseNumberProperty = DataItemProperty.Register(SyncPropertyNames.CourseNumber);

        [Column("CourseNumber")]
        public string CourseNumber
        {
            get { return GetValue<string>(CourseNumberProperty, ""); }
            set { SetValue(CourseNumberProperty, value); }
        }

        public static readonly DataItemProperty ShouldAverageGradeTotalsProperty = DataItemProperty.Register(SyncPropertyNames.ShouldAverageGradeTotals);
        
        [Column("ShouldAverageGradeTotals")]
        public bool ShouldAverageGradeTotals
        {
            get { return GetValue<bool>(ShouldAverageGradeTotalsProperty, false); }
            set { SetValue(ShouldAverageGradeTotalsProperty, value); }
        }

        public static readonly DataItemProperty DoesRoundGradesUpProperty = DataItemProperty.Register(SyncPropertyNames.DoesRoundGradesUp);

        [Column("DoesRoundGradesUp")]
        public bool DoesRoundGradesUp
        {
            get { return GetValue<bool>(DoesRoundGradesUpProperty, true); }
            set { SetValue(DoesRoundGradesUpProperty, value); }
        }

        private static readonly byte[] DEFAULT_RAW_COLOR = new byte[] { 51, 153, 51 };
        public static readonly DataItemProperty RawColorProperty = DataItemProperty.Register(SyncPropertyNames.Color);

        [Column("Color")]
        public byte[] RawColor
        {
            get { return GetValue<byte[]>(RawColorProperty, DEFAULT_RAW_COLOR); }
            set { SetValue(RawColorProperty, value); }
        }

        public static readonly DataItemProperty PositionProperty = DataItemProperty.Register(SyncPropertyNames.Position);

        [Column("Position")]
        public byte Position
        {
            get { return GetValue<byte>(PositionProperty, 0); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly DataItemProperty RawGradeScalesProperty = DataItemProperty.Register(SyncPropertyNames.GradeScales);

        /// <summary>
        /// Do not use this property. It's only here for storing in the SQL database.
        /// </summary>
        [Column("GradeScales")]
        public string RawGradeScales
        {
            get { return GetValue<string>(RawGradeScalesProperty); }
            set { SetValue(RawGradeScalesProperty, value); }
        }

        public static readonly DataItemProperty CreditsProperty = DataItemProperty.Register(SyncPropertyNames.Credits);

        [Column("Credits")]
        public double Credits
        {
            get { return GetValue<double>(CreditsProperty, Grade.NO_CREDITS); }
            set { SetValue(CreditsProperty, value); }
        }

        public static readonly DataItemProperty OverriddenGPAProperty = DataItemProperty.Register(SyncPropertyNames.OverriddenGPA);

        [Column("OverriddenGPA")]
        public double OverriddenGPA
        {
            get { return GetValue<double>(OverriddenGPAProperty, Grade.UNGRADED); }
            set { SetValue(OverriddenGPAProperty, value); }
        }

        public static readonly DataItemProperty OverriddenGradeProperty = DataItemProperty.Register(SyncPropertyNames.OverriddenGrade);
        [Column("OverriddenGrade")]
        public double OverriddenGrade
        {
            get { return GetValue<double>(OverriddenGradeProperty, Grade.UNGRADED); }
            set { SetValue(OverriddenGradeProperty, value); }
        }

        public static readonly DataItemProperty StartDateProperty = DataItemProperty.Register(SyncPropertyNames.StartDate);
        [Column("StartDate")]
        public DateTime StartDate
        {
            get { return GetValue<DateTime>(StartDateProperty, SqlDate.MinValue); }
            set { SetValue(StartDateProperty, value); }
        }

        public static readonly DataItemProperty EndDateProperty = DataItemProperty.Register(SyncPropertyNames.EndDate);
        [Column("EndDate")]
        public DateTime EndDate
        {
            get { return GetValue<DateTime>(EndDateProperty, SqlDate.MinValue); }
            set { SetValue(EndDateProperty, value); }
        }

        public static readonly DataItemProperty GpaTypeProperty = DataItemProperty.Register(SyncPropertyNames.GpaType);
        [Column("GpaType")]
        public GpaType GpaType
        {
            get { return GetValue<GpaType>(GpaTypeProperty, GpaType.Standard); }
            set { SetValue(GpaTypeProperty, value); }
        }

        public static readonly DataItemProperty PassingGradeProperty = DataItemProperty.Register(SyncPropertyNames.PassingGrade);
        /// <summary>
        /// Expressed as a percentage (so 0.6)
        /// </summary>
        [Column("PassingGrade")]
        public double PassingGrade
        {
            get { return GetValue<double>(PassingGradeProperty, Class.DefaultPassingGrade); }
            set { SetValue(PassingGradeProperty, value); }
        }

        // No sync property names since we're not syncing this
        public static readonly DataItemProperty LastTaskTimeOptionProperty = DataItemProperty.Register();
        [Column("LastTaskTimeOption")]
        public virtual string LastTaskTimeOption
        {
            get { return GetValue<string>(LastTaskTimeOptionProperty, null); }
            set { SetValue(LastTaskTimeOptionProperty, value); }
        }

        public static readonly DataItemProperty LastEventTimeOptionProperty = DataItemProperty.Register();
        [Column("LastEventTimeOption")]
        public virtual string LastEventTimeOption
        {
            get { return GetValue<string>(LastEventTimeOptionProperty, null); }
            set { SetValue(LastEventTimeOptionProperty, value); }
        }

        public static readonly DataItemProperty LastTaskDueTimeProperty = DataItemProperty.Register();
        [Column("LastTaskDueTime")]
        public virtual TimeSpan? LastTaskDueTime
        {
            get { return GetValue<TimeSpan?>(LastTaskDueTimeProperty, null); }
            set { SetValue(LastTaskDueTimeProperty, value); }
        }

        public static readonly DataItemProperty LastEventStartTimeProperty = DataItemProperty.Register();
        [Column("LastEventStartTime")]
        public virtual TimeSpan? LastEventStartTime
        {
            get { return GetValue<TimeSpan?>(LastEventStartTimeProperty, null); }
            set { SetValue(LastEventStartTimeProperty, value); }
        }

        public static readonly DataItemProperty LastEventDurationProperty = DataItemProperty.Register();
        [Column("LastEventDurationProperty")]
        public virtual TimeSpan? LastEventDuration
        {
            get { return GetValue<TimeSpan?>(LastEventDurationProperty, null); }
            set { SetValue(LastEventDurationProperty, value); }
        }

        public GradeScale[] GetGradeScales()
        {
            return DeserializeFromString<GradeScale[]>(RawGradeScales);
        }

        public void SetGradeScales(GradeScale[] value)
        {
            RawGradeScales = SerializeToString(value);
        }




        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.CourseNumber.ToString()] = CourseNumber;
            into[SyncPropertyNames.GradeScales.ToString()] = GetGradeScales();
            into[SyncPropertyNames.ShouldAverageGradeTotals.ToString()] = ShouldAverageGradeTotals;
            into[SyncPropertyNames.Position.ToString()] = Position;

            if (RawColor != null)
                into[SyncPropertyNames.Color.ToString()] = ColorHelper.ToString(RawColor);

            into[SyncPropertyNames.DoesRoundGradesUp.ToString()] = DoesRoundGradesUp;
            into[SyncPropertyNames.Credits.ToString()] = Credits;
            into[SyncPropertyNames.OverriddenGrade.ToString()] = OverriddenGrade;
            into[SyncPropertyNames.OverriddenGPA.ToString()] = OverriddenGPA;
            into[SyncPropertyNames.StartDate.ToString()] = StartDate;
            into[SyncPropertyNames.EndDate.ToString()] = EndDate;
            into[SyncPropertyNames.GpaType.ToString()] = GpaType;
            into[SyncPropertyNames.PassingGrade.ToString()] = PassingGrade;

            base.serialize(into);
        }

        protected override BaseItem serialize()
        {
            Class into = new Class()
            {
                CourseNumber = CourseNumber,
                GradeScales = GetGradeScales(),
                ShouldAverageGradeTotals = ShouldAverageGradeTotals,
                Credits = Credits,
                Position = Position,
                Color = ColorHelper.ToString(RawColor),
                DoesRoundGradesUp = DoesRoundGradesUp,
                OverriddenGPA = OverriddenGPA,
                OverriddenGrade = OverriddenGrade,
                StartDate = StartDate,
                EndDate = EndDate,
                GpaType = GpaType,
                PassingGrade = PassingGrade
            };

            base.serialize(into);

            return into;
        }

        protected override void deserialize(BaseItem item, List<SyncPropertyNames> changedProperties)
        {
            Class c = item as Class;
            byte[] rawColor = ColorHelper.ToBytes(c.Color);

            if (changedProperties != null)
            {
                if (CourseNumber != c.CourseNumber)
                    changedProperties.Add(SyncPropertyNames.CourseNumber);

                var gradeScales = GetGradeScales();
                if ((gradeScales == null && c.GradeScales != null) ||
                    (gradeScales != null && c.GradeScales == null) ||
                    (gradeScales != null && c.GradeScales != null && !gradeScales.SequenceEqual(c.GradeScales)))
                    changedProperties.Add(SyncPropertyNames.GradeScales);

                if (ShouldAverageGradeTotals != c.ShouldAverageGradeTotals)
                    changedProperties.Add(SyncPropertyNames.ShouldAverageGradeTotals);

                if (Credits != c.Credits)
                    changedProperties.Add(SyncPropertyNames.Credits);

                if (Position != c.Position)
                    changedProperties.Add(SyncPropertyNames.Position);

                if (!RawColor.SequenceEqual(rawColor))
                    changedProperties.Add(SyncPropertyNames.Color);

                if (DoesRoundGradesUp != c.DoesRoundGradesUp)
                    changedProperties.Add(SyncPropertyNames.DoesRoundGradesUp);

                if (GpaType != c.GpaType)
                    changedProperties.Add(SyncPropertyNames.GpaType);

                if (PassingGrade != c.PassingGrade)
                    changedProperties.Add(SyncPropertyNames.PassingGrade);

                if (OverriddenGrade != c.OverriddenGrade)
                    changedProperties.Add(SyncPropertyNames.OverriddenGrade);

                if (OverriddenGPA != c.OverriddenGPA)
                    changedProperties.Add(SyncPropertyNames.OverriddenGPA);

                if (StartDate != c.StartDate.ToUniversalTime())
                    changedProperties.Add(SyncPropertyNames.StartDate);

                if (EndDate != c.EndDate.ToUniversalTime())
                    changedProperties.Add(SyncPropertyNames.EndDate);
            }

            CourseNumber = c.CourseNumber;
            SetGradeScales(c.GradeScales);
            ShouldAverageGradeTotals = c.ShouldAverageGradeTotals;
            Credits = c.Credits;
            Position = c.Position;

            RawColor = rawColor;

            DoesRoundGradesUp = c.DoesRoundGradesUp;
            GpaType = c.GpaType;
            PassingGrade = c.PassingGrade;

            OverriddenGPA = c.OverriddenGPA;
            OverriddenGrade = c.OverriddenGrade;

            StartDate = c.StartDate.ToUniversalTime();
            EndDate = c.EndDate.ToUniversalTime();

            base.deserialize(c, changedProperties);
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.CourseNumber:
                    return CourseNumber;

                case SyncPropertyNames.GradeScales:
                    return GetGradeScales();

                case SyncPropertyNames.ShouldAverageGradeTotals:
                    return ShouldAverageGradeTotals;

                case SyncPropertyNames.Credits:
                    return Credits;

                case SyncPropertyNames.Position:
                    return Position;

                case SyncPropertyNames.Color:
                    return ColorHelper.ToString(RawColor);

                case SyncPropertyNames.DoesRoundGradesUp:
                    return DoesRoundGradesUp;

                case SyncPropertyNames.OverriddenGrade:
                    return OverriddenGrade;

                case SyncPropertyNames.OverriddenGPA:
                    return OverriddenGPA;

                case SyncPropertyNames.StartDate:
                    return StartDate;

                case SyncPropertyNames.EndDate:
                    return EndDate;

                case SyncPropertyNames.GpaType:
                    return GpaType;

                case SyncPropertyNames.PassingGrade:
                    return PassingGrade;
            }

            return base.GetPropertyValue(p);
        }


        public override int CompareTo(BaseDataItem other)
        {
            if (other is DataItemClass)
                return CompareTo(other as DataItemClass);

            return base.CompareTo(other);
        }

        public int CompareTo(DataItemClass other)
        {
            int comp = Name.CompareTo(other.Name);

            if (comp == 0)
                return DateCreated.CompareTo(other.DateCreated);

            return comp;
        }
    }
}
