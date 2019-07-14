using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public abstract class BaseHomeworkExamWP : BaseHomeworkExamGradeWP
    {
        [DataMember]
        public DateTime EndTime { get; set; } = DateValues.UNASSIGNED;
        
        [DataMember]
        public DateTime Reminder { get; set; } = DateValues.UNASSIGNED;

        public bool HasReminder
        {
            get { return Reminder != DateValues.UNASSIGNED; }
        }

        /// <summary>
        /// Returns true if the item is still active (not archived). As in, if it's a homework, this returns true if the homework is incomplete, or if the homework wasn't already due yet.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsActive(DateTime today);

        protected void serialize(BaseHomeworkExam into, int offset)
        {
            //into.EndTime = DateTime.SpecifyKind(EndTime, DateTimeKind.Utc);
            //into.Reminder = DateTime.SpecifyKind(Reminder, DateTimeKind.Utc);
            into.EndTime = EndTime;
            into.Reminder = Reminder;

            base.serialize(into, offset);
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.EndTime.ToString()] = EndTime;
            into[PropertyNames.Reminder.ToString()] = Reminder;

            base.serialize(into);
        }

        protected void deserialize(BaseHomeworkExam from, List<PropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (EndTime != from.EndTime.ToUniversalTime())
                    changedProperties.Add(PropertyNames.EndTime);

                if (Reminder != from.Reminder.ToUniversalTime())
                    changedProperties.Add(PropertyNames.Reminder);
            }

            EndTime = from.EndTime.ToUniversalTime();
            Reminder = from.Reminder.ToUniversalTime();

            base.deserialize(from, changedProperties);
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.EndTime:
                    return EndTime;

                case PropertyNames.Reminder:
                    return Reminder;
            }

            return base.GetPropertyValue(p);
        }
    }
}
