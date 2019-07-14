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
    public class HomeworkWin : BaseHomeworkWin, IComparable, IComparable<HomeworkWin>, IComparable<ExamWin>
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Homework; }
        }

        #region DisplayProperties

        public string SubTitle
        {
            get
            {
                return (Parent as ClassWin).Name + " - due " + this.Date.ToString("d");
            }
        }

        #endregion


        protected override BaseItem serialize()
        {
            Homework into = new Homework();

            base.serialize(into);

            return into;
        }

        protected override void deserialize(BaseItem item, List<PropertyNames> changedProperties)
        {
            Homework from = item as Homework;

            base.deserialize(from, changedProperties);
        }

        public override int CompareTo(object obj)
        {
            if (obj is BaseItemWin)
                return CompareTo(obj as BaseItemWin);

            throw new NotImplementedException("Item wasn't of supported type to compare with");
        }

        public override int CompareTo(BaseItemWin other)
        {
            if (other is HomeworkWin)
                return CompareTo(other as HomeworkWin);

            else if (other is ExamWin)
                return CompareTo(other as ExamWin);

            return base.CompareTo(other);
        }

        public int CompareTo(ExamWin other)
        {
            //if on same day, homeworks always display after exams
            if (Date.Date == other.Date.Date)
                return 1;

            //else oldest item goes first
            else
                return Date.CompareTo(other.Date);
        }

        public int CompareTo(HomeworkWin other)
        {
            //if this is due on an earlier day
            if (Date.Date < other.Date.Date)
                return -1;

            //else if this is due on a later day
            if (Date.Date > other.Date.Date)
                return 1;

            //else they're both on the same day...

            //if this is complete
            if (IsComplete)
            {
                //if both are complete
                if (other.IsComplete)
                {
                    return compareViaClass(Parent as ClassWin, Updated, Date, other.Parent as ClassWin, other.Updated, other.Date);
                }

                //other isn't complete, place this later
                return 1;
            }

            //else this isn't complete

            //if the other is complete, this goes first
            if (other.IsComplete)
                return -1;

            //sort by schedule
            return compareViaClass(Parent as ClassWin, Updated, Date, other.Parent as ClassWin, other.Updated, other.Date);
        }
    }
}
