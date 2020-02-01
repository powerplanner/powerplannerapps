using PowerPlannerSending;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems
{
    public abstract class BaseDataItemHomeworkExamGrade : BaseDataItemWithImages
    {
        public static readonly DataItemProperty DateProperty = DataItemProperty.Register(SyncPropertyNames.Date);

        [Column("Date")]
        public DateTime Date
        {
            get { return GetValue<DateTime>(DateProperty, SqlDate.MinValue); }
            set { SetValue(DateProperty, value); }
        }

        public static readonly DataItemProperty GradeReceivedProperty = DataItemProperty.Register(SyncPropertyNames.GradeReceived);

        [Column("GradeReceived")]
        public double GradeReceived
        {
            get { return GetValue<double>(GradeReceivedProperty, Grade.UNGRADED); }
            set { SetValue(GradeReceivedProperty, value); }
        }

        public static readonly DataItemProperty GradeTotalProperty = DataItemProperty.Register(SyncPropertyNames.GradeTotal);

        [Column("GradeTotal")]
        public double GradeTotal
        {
            get { return GetValue<double>(GradeTotalProperty, 100); }
            set { SetValue(GradeTotalProperty, value); }
        }

        public static readonly DataItemProperty IsDroppedProperty = DataItemProperty.Register(SyncPropertyNames.IsDropped);

        [Column("IsDropped")]
        public bool IsDropped
        {
            get { return GetValue<bool>(IsDroppedProperty, false); }
            set { SetValue(IsDroppedProperty, value); }
        }

        public static readonly DataItemProperty IndividualWeightProperty = DataItemProperty.Register(SyncPropertyNames.IndividualWeight);

        [Column("IndividualWeight")]
        public double IndividualWeight
        {
            get { return GetValue<double>(IndividualWeightProperty, 1); }
            set { SetValue(IndividualWeightProperty, value); }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.Date.ToString()] = Date;

            into[SyncPropertyNames.GradeReceived.ToString()] = GradeReceived;
            into[SyncPropertyNames.GradeTotal.ToString()] = GradeTotal;
            into[SyncPropertyNames.IsDropped.ToString()] = IsDropped;
            into[SyncPropertyNames.IndividualWeight.ToString()] = IndividualWeight;

            base.serialize(into);
        }

        protected void serialize(BaseHomeworkExamGrade into)
        {
            into.Date = Date;

            into.GradeReceived = GradeReceived;
            into.GradeTotal = GradeTotal;
            into.IsDropped = IsDropped;
            into.IndividualWeight = IndividualWeight;

            base.serialize(into);
        }

        protected void deserialize(BaseHomeworkExamGrade from, List<BaseDataItem.SyncPropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (Date != from.Date.ToUniversalTime())
                    changedProperties.Add(BaseDataItem.SyncPropertyNames.Date);

                if (GradeReceived != from.GradeReceived)
                    changedProperties.Add(SyncPropertyNames.GradeReceived);

                if (GradeTotal != from.GradeTotal)
                    changedProperties.Add(SyncPropertyNames.GradeTotal);

                if (IsDropped != from.IsDropped)
                    changedProperties.Add(SyncPropertyNames.IsDropped);

                if (IndividualWeight != from.IndividualWeight)
                    changedProperties.Add(SyncPropertyNames.IndividualWeight);
            }

            Date = from.Date.ToUniversalTime();

            GradeReceived = from.GradeReceived;
            GradeTotal = from.GradeTotal;
            IsDropped = from.IsDropped;
            IndividualWeight = from.IndividualWeight;

            base.deserialize(from, changedProperties);
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.Date:
                    return Date;

                case SyncPropertyNames.GradeReceived:
                    return GradeReceived;

                case SyncPropertyNames.GradeTotal:
                    return GradeTotal;

                case SyncPropertyNames.IsDropped:
                    return IsDropped;

                case SyncPropertyNames.IndividualWeight:
                    return IndividualWeight;
            }

            return base.GetPropertyValue(p);
        }
    }
}
