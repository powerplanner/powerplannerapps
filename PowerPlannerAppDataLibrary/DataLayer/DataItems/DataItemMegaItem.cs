using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerSending;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems
{
    public class DataItemMegaItem : BaseDataItemHomeworkExamGrade
    {
        public enum TimeOptions
        {
            /// <summary>
            /// All day for events, or end of day for tasks
            /// </summary>
            AllDay,
            BeforeClass,
            StartOfClass,
            DuringClass,
            EndOfClass,
            Custom
        }

        #region MegaItemType

        public static readonly DataItemProperty MegaItemTypeProperty = DataItemProperty.Register(SyncPropertyNames.MegaItemType);

        [Column("MegaItemType")]
        public MegaItemType MegaItemType
        {
            get { return GetValue(MegaItemTypeProperty, default(MegaItemType)); }
            set { SetValue(MegaItemTypeProperty, value); }
        }

        #endregion

        #region EndTime

        public static readonly DataItemProperty EndTimeProperty = DataItemProperty.Register(SyncPropertyNames.EndTime);

        [Column("EndTime")]
        public DateTime EndTime
        {
            get { return GetValue<DateTime>(EndTimeProperty, DateValues.UNASSIGNED); }
            set { SetValue(EndTimeProperty, value); }
        }

        #endregion

        public TimeOptions TimeOption
        {
            get
            {
                switch (MegaItemType)
                {
                    case MegaItemType.Homework:
                        if (Date.Second == 0)
                        {
                            return TimeOptions.StartOfClass;
                        }
                        if (Date.Second == 1)
                        {
                            return TimeOptions.BeforeClass;
                        }
                        if (Date.Second == 2)
                        {
                            return TimeOptions.DuringClass;
                        }
                        if (Date.Second == 3)
                        {
                            return TimeOptions.EndOfClass;
                        }
                        if (Date.Second == 4)
                        {
                            return TimeOptions.Custom;
                        }
                        return TimeOptions.AllDay;

                    case MegaItemType.Task:
                        if (Date.Second == 4)
                        {
                            return TimeOptions.Custom;
                        }
                        return TimeOptions.AllDay;

                    case MegaItemType.Exam:
                        if (EndTime == DateValues.UNASSIGNED)
                        {
                            return TimeOptions.DuringClass;
                        }
                        if (EndTime.Second == 59)
                        {
                            return TimeOptions.AllDay;
                        }
                        return TimeOptions.Custom;

                    case MegaItemType.Event:
                        if (EndTime.Second == 59)
                        {
                            return TimeOptions.AllDay;
                        }
                        return TimeOptions.Custom;
                }

                return TimeOptions.AllDay;
            }
        }

        #region Reminder

        public static readonly DataItemProperty ReminderProperty = DataItemProperty.Register(SyncPropertyNames.Reminder);

        [Column("Reminder")]
        public DateTime Reminder
        {
            get { return GetValue<DateTime>(ReminderProperty, DateValues.UNASSIGNED); }
            set { SetValue(ReminderProperty, value); }
        }

        #endregion

        public static readonly DataItemProperty WeightCategoryIdentifierProperty = DataItemProperty.Register(SyncPropertyNames.WeightCategoryIdentifier);

        [Column("WeightCategoryIdentifier")]
        public Guid WeightCategoryIdentifier
        {
            get { return GetValue<Guid>(WeightCategoryIdentifierProperty, Guid.Empty); }
            set { SetValue(WeightCategoryIdentifierProperty, value); }
        }

        public static readonly DataItemProperty PercentCompleteProperty = DataItemProperty.Register(SyncPropertyNames.PercentComplete);

        [Column("PercentComplete")]
        public double PercentComplete
        {
            get { return GetValue<double>(PercentCompleteProperty, 0); }
            set { SetValue(PercentCompleteProperty, value); }
        }

        #region AppointmentLocalId

        [Column("AppointmentLocalId")]
        public string AppointmentLocalId
        {
            get;
            set;
        }

        public override ItemType ItemType
        {
            get
            {
                return ItemType.MegaItem;
            }
        }

        #endregion

#if ANDROID
        [Column("HasSentReminder")]
        public bool HasSentReminder
        {
            get;
            set;
        }
#endif

        /// <summary>
        /// Returns null if none of the expected types
        /// </summary>
        /// <param name="semester"></param>
        /// <returns></returns>
        public ViewItemTaskOrEvent CreateViewItemTaskOrEvent(ViewItemSemester semester)
        {
            return CreateViewItemTaskOrEvent(semester.Classes, semester.NoClassClass);
        }

        /// <summary>
        /// Returns null if none of the expected types
        /// </summary>
        /// <returns></returns>
        public ViewItemTaskOrEvent CreateViewItemTaskOrEvent(IEnumerable<ViewItemClass> classes, ViewItemClass noClassClass)
        {
            ViewItemTaskOrEvent view;
            switch (MegaItemType)
            {
                case MegaItemType.Homework:
                case MegaItemType.Task:
                case MegaItemType.Exam:
                case MegaItemType.Event:
                    view = new ViewItemTaskOrEvent(this);
                    break;

                default:
                    return null;
            }

            if (this.MegaItemType == MegaItemType.Task || this.MegaItemType == MegaItemType.Event)
            {
                view.Class = noClassClass;
            }
            else
            {
                view.Class = classes.First(i => i.Identifier == this.UpperIdentifier);
            }

            return view;
        }

        public bool IsUnderClass()
        {
            return MegaItemType == MegaItemType.Homework || MegaItemType == MegaItemType.Exam;
        }

        /// <summary>
        /// Used for holidays
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool IsOnDate(DateTime date)
        {
            return date >= Date && date <= EndTime;
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.MegaItemType.ToString()] = MegaItemType;

            into[SyncPropertyNames.EndTime.ToString()] = EndTime;
            into[SyncPropertyNames.Reminder.ToString()] = Reminder;

            into[SyncPropertyNames.WeightCategoryIdentifier.ToString()] = WeightCategoryIdentifier;
            into[SyncPropertyNames.PercentComplete.ToString()] = PercentComplete;

            base.serialize(into);
        }

        protected override void deserialize(BaseItem item, List<SyncPropertyNames> changedProperties)
        {
            MegaItem from = item as MegaItem;

            if (changedProperties != null)
            {
                if (MegaItemType != from.MegaItemType)
                    changedProperties.Add(BaseDataItem.SyncPropertyNames.MegaItemType);

                if (EndTime != from.EndTime.ToUniversalTime())
                    changedProperties.Add(BaseDataItem.SyncPropertyNames.EndTime);

                if (Reminder != from.Reminder.ToUniversalTime())
                    changedProperties.Add(BaseDataItem.SyncPropertyNames.Reminder);

                if (WeightCategoryIdentifier != from.WeightCategoryIdentifier)
                    changedProperties.Add(SyncPropertyNames.WeightCategoryIdentifier);

                if (PercentComplete != from.PercentComplete)
                    changedProperties.Add(BaseDataItem.SyncPropertyNames.PercentComplete);
            }

            MegaItemType = from.MegaItemType;

            EndTime = from.EndTime.ToUniversalTime();
            Reminder = from.Reminder.ToUniversalTime();

            WeightCategoryIdentifier = from.WeightCategoryIdentifier;
            PercentComplete = from.PercentComplete;

            base.deserialize(from, changedProperties);
        }

        public override string ToString()
        {
            return Name + " - " + Date;
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.MegaItemType:
                    return MegaItemType;

                case SyncPropertyNames.EndTime:
                    return EndTime;

                case SyncPropertyNames.Reminder:
                    return Reminder;

                case SyncPropertyNames.WeightCategoryIdentifier:
                    return WeightCategoryIdentifier;

                case SyncPropertyNames.PercentComplete:
                    return PercentComplete;
            }

            return base.GetPropertyValue(p);
        }

        protected override BaseItem serialize()
        {
            var into = new MegaItem()
            {
                MegaItemType = MegaItemType,

                EndTime = EndTime,
                Reminder = Reminder,

                WeightCategoryIdentifier = WeightCategoryIdentifier,
                PercentComplete = PercentComplete
            };

            base.serialize(into);

            return into;
        }

        /// <summary>
        /// Returns true if the MegaItemType is Task, Homework, Event, or Exam
        /// </summary>
        /// <returns></returns>
        public bool IsTaskOrEvent()
        {
            switch (MegaItemType)
            {
                case MegaItemType.Task:
                case MegaItemType.Homework:
                case MegaItemType.Event:
                case MegaItemType.Exam:
                    return true;

                default:
                    return false;
            }
        }
    }
}
