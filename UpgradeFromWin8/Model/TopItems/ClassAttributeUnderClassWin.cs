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
    public class ClassAttributeUnderClassWin : BaseItemUnderTwoWin
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.ClassAttributeUnderClass; }
        }

        protected override PowerPlannerSending.BaseItem serialize()
        {
            ClassAttributeUnderClass into = new ClassAttributeUnderClass();

            base.serialize(into);

            return into;
        }

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<PropertyNames> changedProperties)
        {
            ClassAttributeUnderClass from = item as ClassAttributeUnderClass;

            base.deserialize(from, changedProperties);
        }

        public override IEnumerable<BaseItemWin> GetChildren()
        {
            return new BaseItemWin[0];
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
