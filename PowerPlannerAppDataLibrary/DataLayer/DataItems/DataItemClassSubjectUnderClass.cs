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
    public class DataItemClassSubjectUnderClass : BaseDataItemUnderTwo
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

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<BaseDataItem.SyncPropertyNames> changedProperties)
        {
            ClassSubjectUnderClass from = item as ClassSubjectUnderClass;

            base.deserialize(from, changedProperties);
        }
    }
}
