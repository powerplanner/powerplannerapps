using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromWin8.Model.TopItems
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model.TopItems")]
    public class ExamWin : BaseHomeworkExamWin, IComparable, IComparable<ExamWin>, IComparable<HomeworkWin>
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Exam; }
        }

        #region DisplayProperties

        public string SubTitle
        {
            get { return (Parent as ClassWin).Name + " - on " + this.Date.ToString("d"); }
        }

        #endregion

        public override int CompareTo(object obj)
        {
            if (obj is BaseItemWin)
                return CompareTo(obj as BaseItemWin);

            return 0;
        }

        public override int CompareTo(BaseItemWin other)
        {
            if (other is ExamWin)
                return CompareTo(other as ExamWin);

            if (other is HomeworkWin)
            {
                return CompareTo(other as HomeworkWin);
            }

            return base.CompareTo(other);
        }

        public int CompareTo(ExamWin other)
        {
            //oldest exams are shown first
            int compare = Date.CompareTo(other.Date);

            if (compare == 0)
                return compareViaClass(Parent as ClassWin, Updated, Date, other.Parent as ClassWin, other.Updated, other.Date); //use schedule

            return compare;
        }

        public int CompareTo(HomeworkWin other)
        {
            //if on same day, exams go first
            if (Date.Date == other.Date.Date)
                return -1;

            else
                return Date.CompareTo(other.Date);
        }

        protected override BaseItem serialize()
        {
            Exam into = new Exam();

            base.serialize(into);

            return into;
        }

        protected override void deserialize(BaseItem item, List<PropertyNames> changedProperties)
        {
            Exam i = item as Exam;

            base.deserialize(i, changedProperties);
        }
    }
}
