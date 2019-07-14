using PowerPlannerSending;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class TaskWP : BaseHomeworkWP
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Task; }
        }

        public override int CompareTo(object obj)
        {
            if (obj is BaseItemWP)
                return CompareTo(obj as BaseItemWP);

            return 0;
        }

        public override int CompareTo(BaseItemWP other)
        {
            if (other is HomeworkWP || other is ExamWP)
            {
                //if on same day, these go earlier than tasks
                if (Date.Date == ((ExamWP)other).Date.Date)
                    return 1;

                //else oldest item goes first
                else
                    return Date.CompareTo(((ExamWP)other).Date);
            }

            return base.CompareTo(other);
        }

        protected override BaseItem serialize(int offset)
        {
            Task into = new Task();

            base.serialize(into, offset);

            return into;
        }

        protected override void deserialize(BaseItem item, List<PropertyNames> changedProperties)
        {
            PowerPlannerSending.Task i = item as PowerPlannerSending.Task;

            base.deserialize(i, changedProperties);
        }
    }
}
