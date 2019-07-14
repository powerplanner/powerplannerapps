using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class ExamWP : BaseHomeworkExamWP, IComparable, IComparable<ExamWP>, IComparable<HomeworkWP>
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Exam; }
        }
        

        public override int CompareTo(object obj)
        {
            if (obj is BaseItemWP)
                return CompareTo(obj as BaseItemWP);

            return 0;
        }

        public override int CompareTo(BaseItemWP other)
        {
            if (other is ExamWP)
                return CompareTo(other as ExamWP);

            if (other is HomeworkWP)
            {
                return CompareTo(other as HomeworkWP);
            }

            return base.CompareTo(other);
        }

        public int CompareTo(ExamWP other)
        {
            return 0;
        }

        public int CompareTo(HomeworkWP other)
        {
            //if on same day, exams go first
            if (Date.Date == other.Date.Date)
                return -1;

            else
                return Date.CompareTo(other.Date);
        }

        protected override BaseItem serialize(int offset)
        {
            Exam into = new Exam();

            base.serialize(into, offset);

            return into;
        }

        protected override void deserialize(BaseItem item, List<PropertyNames> changedProperties)
        {
            Exam i = item as Exam;

            base.deserialize(i, changedProperties);
        }

        public override bool IsActive(DateTime today)
        {
            return Date.Date >= today;
        }
    }
}
