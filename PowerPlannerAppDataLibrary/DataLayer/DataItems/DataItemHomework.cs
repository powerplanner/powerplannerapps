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
    [Obsolete("Legacy type no longer used, replaced by DataItemMegaItem")]
    public class DataItemHomework : BaseDataItemHomework
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Homework; }
        }


        protected override BaseItem serialize()
        {
            Homework into = new Homework();

            base.serialize(into);

            return into;
        }

        protected override void deserialize(BaseItem item, List<SyncPropertyNames> changedProperties)
        {
            Homework from = item as Homework;

            base.deserialize(from, changedProperties);
        }
    }
}
