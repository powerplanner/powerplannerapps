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
    public class DataItemTask : BaseDataItemHomework
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Task; }
        }

        protected override BaseItem serialize()
        {
            PowerPlannerSending.Task into = new PowerPlannerSending.Task();

            base.serialize(into);

            return into;
        }

        protected override void deserialize(BaseItem item, List<SyncPropertyNames> changedProperties)
        {
            PowerPlannerSending.Task i = item as PowerPlannerSending.Task;

            base.deserialize(i, changedProperties);
        }
    }
}
