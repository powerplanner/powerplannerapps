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
    public class DataItemClassAttribute : BaseDataItemWithName
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

        protected override void deserialize(BaseItem item, List<SyncPropertyNames> changedProperties)
        {
            ClassAttribute from = item as ClassAttribute;

            base.deserialize(from, changedProperties);
        }
    }
}
