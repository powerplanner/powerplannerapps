using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems
{
    public class DataItemClassSubject : BaseDataItemWithDetails
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

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<BaseDataItem.SyncPropertyNames> changedProperties)
        {
            ClassSubject from = item as ClassSubject;

            base.deserialize(from, changedProperties);
        }
    }
}
