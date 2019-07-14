using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class TeacherUnderScheduleWP : BaseItemUnderTwoWP
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.TeacherUnderSchedule; }
        }

        protected override PowerPlannerSending.BaseItem serialize(int offset)
        {
            TeacherUnderSchedule into = new TeacherUnderSchedule();

            base.serialize(into, offset);

            return into;
        }

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<PropertyNames> changedProperties)
        {
            TeacherUnderSchedule from = item as TeacherUnderSchedule;

            base.deserialize(from, changedProperties);
        }

        public override IEnumerable<BaseItemWP> GetChildren()
        {
            throw new NotImplementedException();
        }

        internal override void Remove(BaseItemWP item)
        {
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
    }
}
