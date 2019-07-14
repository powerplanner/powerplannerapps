using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class ClassAttributeWP : BaseItemWithNameWP
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.ClassAttribute; }
        }

        protected override PowerPlannerSending.BaseItem serialize(int offset)
        {
            ClassAttribute item = new ClassAttribute();

            base.serialize(item, offset);

            return item;
        }

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<PropertyNames> changedProperties)
        {
            ClassAttribute from = item as ClassAttribute;

            base.deserialize(from, changedProperties);
        }

        public override IEnumerable<BaseItemWP> GetChildren()
        {
            return new BaseItemWP[0];
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
