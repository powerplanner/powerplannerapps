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
    public class DataItemGrade : BaseDataItemHomeworkExamGrade
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Grade; }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            base.serialize(into);
        }

        protected override BaseItem serialize()
        {
            Grade into = new Grade();

            base.serialize(into);

            return into;
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            return base.GetPropertyValue(p);
        }

        protected override void deserialize(BaseItem item, List<SyncPropertyNames> changedProperties)
        {
            Grade from = item as Grade;

            base.deserialize(from, changedProperties);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
