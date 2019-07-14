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
    public class ClassSubjectWin : BaseItemWithDetailsWin
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.ClassSubject; }
        }

        protected override PowerPlannerSending.BaseItem serialize()
        {
            ClassSubject into = new ClassSubject();

            base.serialize(into);

            return into;
        }

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<PropertyNames> changedProperties)
        {
            ClassSubject from = item as ClassSubject;

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
