using UpgradeFromWin8.Model.TopItems;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromWin8.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model")]
    public abstract class BaseHomeworkExamWin : BaseHomeworkExamGradeWin
    {
        private DateTime _endTime = DateValues.UNASSIGNED;
        [DataMember]
        public DateTime EndTime
        {
            get { return _endTime; }
            set { SetProperty(ref _endTime, DateTime.SpecifyKind(value, DateTimeKind.Utc), "EndTime"); }
        }

        private DateTime _reminder = DateValues.UNASSIGNED;
        [DataMember]
        public DateTime Reminder
        {
            get { return _reminder; }
            set { SetProperty(ref _reminder, DateTime.SpecifyKind(value, DateTimeKind.Utc), "Reminder"); }
        }

        public bool HasReminder
        {
            get { return Reminder != DateValues.UNASSIGNED; }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.EndTime.ToString()] = EndTime;
            into[PropertyNames.Reminder.ToString()] = Reminder;

            base.serialize(into);
        }

        protected void serialize(BaseHomeworkExam into)
        {
            into.EndTime = EndTime;
            into.Reminder = Reminder;

            base.serialize(into);
        }

        protected void deserialize(BaseHomeworkExam from, List<BaseItemWin.PropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (EndTime != from.EndTime.ToUniversalTime())
                    changedProperties.Add(BaseItemWin.PropertyNames.EndTime);

                if (Reminder != from.Reminder.ToUniversalTime())
                    changedProperties.Add(BaseItemWin.PropertyNames.Reminder);
            }

            EndTime = from.EndTime.ToUniversalTime();
            Reminder = from.Reminder.ToUniversalTime();

            base.deserialize(from, changedProperties);
        }

        /// <summary>
        /// Returns the semester it's under, or null if it couldn't get its semester
        /// </summary>
        /// <returns></returns>
        public SemesterWin GetSemester()
        {
            ClassWin c = Parent as ClassWin;
            if (c == null)
                return null;

            return c.Parent as SemesterWin;
        }

        public override string ToString()
        {
            return Name + " - " + Date;
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
