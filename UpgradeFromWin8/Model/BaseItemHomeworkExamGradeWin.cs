using UpgradeFromWin8.Model.TopItems;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace UpgradeFromWin8.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model")]
    public abstract class BaseHomeworkExamGradeWin : BaseItemWithImagesWin
    {
        private DateTime _date = SqlDate.MinValue;
        [DataMember]
        public DateTime Date
        {
            get { return _date; }
            set { SetProperty(ref _date, DateTime.SpecifyKind(value, DateTimeKind.Utc), "Date"); }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.Date.ToString()] = Date;

            base.serialize(into);
        }

        protected void serialize(BaseHomeworkExamGrade into)
        {
            into.Date = Date;

            base.serialize(into);
        }

        protected void deserialize(BaseHomeworkExamGrade from, List<BaseItemWin.PropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (Date != from.Date.ToUniversalTime())
                    changedProperties.Add(BaseItemWin.PropertyNames.Date);
            }

            Date = from.Date.ToUniversalTime();

            base.deserialize(from, changedProperties);
        }

        protected static int compareViaClass(
            ClassWin class1, DateTime updated1, DateTime actualDate1,
            ClassWin class2, DateTime updated2, DateTime actualDate2)
        {
            if (class1 == null || class2 == null || class1 == class2)
                return updated1.CompareTo(updated2);

            ScheduleWin schedule1 = findSchedule(class1, actualDate1);
            ScheduleWin schedule2 = findSchedule(class2, actualDate2);

            //if both schedules missing, use the updated time
            if (schedule1 == null && schedule2 == null)
                return updated1.CompareTo(updated2);

            //if the other schedule is missing, this item should go first
            if (schedule2 == null)
                return -1;

            //if this schedule is missing, the other item should go first
            if (schedule1 == null)
                return 1;

            //get the comparison of their start times
            int comp = schedule1.StartTime.TimeOfDay.CompareTo(schedule2.StartTime.TimeOfDay);

            //if they started at the same time, use the updated time
            if (comp == 0)
                return updated1.CompareTo(updated2);

            return comp;
        }

        private static ScheduleWin findSchedule(ClassWin c, DateTime date)
        {
            for (int i = 0; i < c.Schedules.Count; i++)
                if (c.Schedules[i].DayOfWeek == date.DayOfWeek)
                    return c.Schedules[i];

            return null;
        }

        public override IEnumerable<BaseItemWin> GetChildren()
        {
            return new BaseItemWin[0];
        }

        internal override void Remove(BaseItemWin item)
        {
            //should never be called
            throw new NotImplementedException();
        }

        internal override void Add(BaseItemWin item)
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
