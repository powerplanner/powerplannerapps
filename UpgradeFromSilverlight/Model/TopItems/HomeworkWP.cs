using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class HomeworkWP : BaseHomeworkWP, IComparable, IComparable<HomeworkWP>, IComparable<ExamWP>
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Homework; }
        }
        

        protected override BaseItem serialize(int offset)
        {
            Homework into = new Homework();

            base.serialize(into, offset);

            return into;
        }

        protected override void deserialize(BaseItem item, List<PropertyNames> changedProperties)
        {
            Homework from = item as Homework;

            base.deserialize(from, changedProperties);
        }

        public override int CompareTo(object obj)
        {
            if (obj is BaseItemWP)
                return CompareTo(obj as BaseItemWP);

            throw new NotImplementedException("Item wasn't of supported type to compare with");
        }

        public override int CompareTo(BaseItemWP other)
        {
            if (other is HomeworkWP)
                return CompareTo(other as HomeworkWP);

            else if (other is ExamWP)
                return CompareTo(other as ExamWP);

            return base.CompareTo(other);
        }

        public int CompareTo(ExamWP other)
        {
            //if on same day, homeworks always display after exams
            if (Date.Date == other.Date.Date)
                return 1;

            //else oldest item goes first
            else
                return Date.CompareTo(other.Date);
        }

        public int CompareTo(HomeworkWP other)
        {
            //if this is due on an earlier day
            if (Date.Date < other.Date.Date)
                return -1;

            //else if this is due on a later day
            if (Date.Date > other.Date.Date)
                return 1;

            return 0;
        }
    }
}
