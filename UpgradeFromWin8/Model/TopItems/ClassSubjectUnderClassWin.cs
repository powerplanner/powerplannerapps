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
    public class ClassSubjectUnderClassWin : BaseItemUnderTwoWin
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.ClassSubjectUnderClass; }
        }

        protected override PowerPlannerSending.BaseItem serialize()
        {
            ClassSubjectUnderClass into = new ClassSubjectUnderClass();

            base.serialize(into);

            return into;
        }

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<BaseItemWin.PropertyNames> changedProperties)
        {
            ClassSubjectUnderClass from = item as ClassSubjectUnderClass;

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
