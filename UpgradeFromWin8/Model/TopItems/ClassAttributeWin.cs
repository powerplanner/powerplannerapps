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
    public class ClassAttributeWin : BaseItemWithNameWin
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.ClassAttribute; }
        }

        protected override PowerPlannerSending.BaseItem serialize()
        {
            ClassAttribute item = new ClassAttribute();

            base.serialize(item);

            return item;
        }

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<PropertyNames> changedProperties)
        {
            ClassAttribute from = item as ClassAttribute;

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
