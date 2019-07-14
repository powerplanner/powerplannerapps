using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class ClassSubjectWP : BaseItemWithDetailsWP
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.ClassSubject; }
        }

        protected override PowerPlannerSending.BaseItem serialize(int offset)
        {
            ClassSubject into = new ClassSubject();

            base.serialize(into, offset);

            return into;
        }

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<PropertyNames> changedProperties)
        {
            ClassSubject from = item as ClassSubject;

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
