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
    public class TeacherUnderScheduleWin : BaseItemUnderTwoWin
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.TeacherUnderSchedule; }
        }

        protected override PowerPlannerSending.BaseItem serialize()
        {
            TeacherUnderSchedule into = new TeacherUnderSchedule();

            base.serialize(into);

            return into;
        }

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<PropertyNames> changedProperties)
        {
            TeacherUnderSchedule from = item as TeacherUnderSchedule;

            base.deserialize(from, changedProperties);
        }

        public override IEnumerable<BaseItemWin> GetChildren()
        {
            throw new NotImplementedException();
        }

        internal override void Remove(BaseItemWin item)
        {
            throw new NotImplementedException();
        }

        internal override void Add(BaseItemWin item)
        {
            throw new NotImplementedException();
        }
    }
}
