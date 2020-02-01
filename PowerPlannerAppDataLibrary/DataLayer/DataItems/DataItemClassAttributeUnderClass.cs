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
    public class DataItemClassAttributeUnderClass : BaseDataItemUnderTwo
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

        protected override void deserialize(BaseItem item, List<BaseDataItem.SyncPropertyNames> changedProperties)
        {
            ClassAttributeUnderClass from = item as ClassAttributeUnderClass;

            base.deserialize(from, changedProperties);
        }
    }
}
