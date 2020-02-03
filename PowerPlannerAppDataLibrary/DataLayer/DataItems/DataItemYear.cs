using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems
{
    public class DataItemYear : BaseDataItemWithOverriddenGPACredits
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Year; }
        }



        protected override BaseItem serialize()
        {
            Year into = new Year();

            base.serialize(into);

            return into;
        }

        protected override void deserialize(BaseItem item, List<SyncPropertyNames> changedProperties)
        {
            Year i = item as Year;

            base.deserialize(i, changedProperties);
        }
    }
}
