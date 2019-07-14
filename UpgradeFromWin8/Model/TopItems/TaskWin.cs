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
    public class TaskWin : BaseHomeworkWin
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Task; }
        }

        public override int CompareTo(object obj)
        {
            if (obj is BaseItemWin)
                return CompareTo(obj as BaseItemWin);

            return 0;
        }

        public override int CompareTo(BaseItemWin other)
        {
            if (other is HomeworkWin || other is ExamWin)
            {
                //if on same day, these go earlier than tasks
                if (Date.Date == ((ExamWin)other).Date.Date)
                    return 1;

                //else oldest item goes first
                else
                    return Date.CompareTo(((ExamWin)other).Date);
            }

            if (other is TaskWin)
            {
                TaskWin task = other as TaskWin;

                //if this is due on an earlier day
                if (Date.Date < task.Date.Date)
                    return -1;

                //else if this is due on a later day
                if (Date.Date > task.Date.Date)
                    return 1;

                //else they're both on the same day...

                //if this is complete
                if (IsComplete)
                {
                    //if both are complete
                    if (task.IsComplete)
                    {
                        return Updated.CompareTo(task.Updated);
                    }

                    //other isn't complete, place this later
                    return 1;
                }

                //else this isn't complete

                //if the other is complete, this goes first
                if (task.IsComplete)
                    return -1;

                //else if this is more recent, put it last
                return Updated.CompareTo(task.Updated);
            }

            return base.CompareTo(other);
        }

        protected override BaseItem serialize()
        {
            PowerPlannerSending.Task into = new PowerPlannerSending.Task();

            base.serialize(into);

            return into;
        }

        protected override void deserialize(BaseItem item, List<PropertyNames> changedProperties)
        {
            PowerPlannerSending.Task i = item as PowerPlannerSending.Task;

            base.deserialize(i, changedProperties);
        }
    }
}
