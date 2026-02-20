using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems
{
    [Obsolete("Legacy type no longer used, replaced by DataItemMegaItem. Kept around just for data upgrade purposes.")]
    public abstract class BaseDataItemHomeworkExam : BaseDataItemHomeworkExamGrade
    {
        #region EndTime

        public static readonly DataItemProperty EndTimeProperty = DataItemProperty.Register(SyncPropertyNames.EndTime);

        [Column("EndTime")]
        public DateTime EndTime
        {
            get { return GetValue<DateTime>(EndTimeProperty, DateValues.UNASSIGNED); }
            set { SetValue(EndTimeProperty, value); }
        }

        #endregion

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

        #region AppointmentLocalId

        [Column("AppointmentLocalId")]
        public string AppointmentLocalId
        {
            get;
            set;
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

        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.EndTime.ToString()] = EndTime;
            into[SyncPropertyNames.Reminder.ToString()] = Reminder;
            
            into[SyncPropertyNames.WeightCategoryIdentifier.ToString()] = WeightCategoryIdentifier;

            base.serialize(into);
        }

        protected void serialize(BaseHomeworkExam into)
        {
            into.EndTime = EndTime;
            into.Reminder = Reminder;

            into.WeightCategoryIdentifier = WeightCategoryIdentifier;

            base.serialize(into);
        }

        protected void deserialize(BaseHomeworkExam from, List<BaseDataItem.SyncPropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (EndTime != from.EndTime.ToUniversalTime())
                    changedProperties.Add(BaseDataItem.SyncPropertyNames.EndTime);

                if (Reminder != from.Reminder.ToUniversalTime())
                    changedProperties.Add(BaseDataItem.SyncPropertyNames.Reminder);

                if (WeightCategoryIdentifier != from.WeightCategoryIdentifier)
                    changedProperties.Add(SyncPropertyNames.WeightCategoryIdentifier);
            }

            EndTime = from.EndTime.ToUniversalTime();
            Reminder = from.Reminder.ToUniversalTime();

            WeightCategoryIdentifier = from.WeightCategoryIdentifier;

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
                case SyncPropertyNames.EndTime:
                    return EndTime;

                case SyncPropertyNames.Reminder:
                    return Reminder;

                case SyncPropertyNames.WeightCategoryIdentifier:
                    return WeightCategoryIdentifier;
            }

            return base.GetPropertyValue(p);
        }
    }
}
