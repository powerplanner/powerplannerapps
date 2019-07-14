using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ToolsPortable;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public abstract class BaseHomeworkExamGradeWP : BaseItemWithImagesWP
    {
        [DataMember]
        public DateTime Date { get; set; } = SqlDate.MinValue;

        protected void serialize(BaseHomeworkExamGrade into, int offset)
        {
            //into.Date = DateTime.SpecifyKind(Date, DateTimeKind.Utc);
            into.Date = Date;

            base.serialize(into, offset);
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.Date.ToString()] = Date;

            base.serialize(into);
        }

        protected void deserialize(BaseHomeworkExamGrade from, List<PropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (Date != from.Date.ToUniversalTime())
                    changedProperties.Add(PropertyNames.Date);
            }

            Date = from.Date.ToUniversalTime();

            base.deserialize(from, changedProperties);
        }

        private static ScheduleWP findSchedule(ClassWP c, DateTime date)
        {
            for (int i = 0; i < c.Schedules.Count; i++)
                if (c.Schedules[i].DayOfWeek == date.DayOfWeek)
                    return c.Schedules[i];

            return null;
        }

        public override IEnumerable<BaseItemWP> GetChildren()
        {
            return new BaseItemWP[0];
        }

        internal override void Remove(BaseItemWP item)
        {
            //should never be called
            throw new NotImplementedException();
        }

        internal override void Add(BaseItemWP item)
        {
            throw new NotImplementedException();
        }

        internal override BaseItemWP FindFromSection(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.Date:
                    return Date;
            }

            return base.GetPropertyValue(p);
        }
    }
}
