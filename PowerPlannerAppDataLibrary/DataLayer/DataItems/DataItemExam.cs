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
    public class DataItemExam : BaseDataItemHomeworkExam
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Exam; }
        }

        protected override BaseItem serialize()
        {
            Exam into = new Exam();

            base.serialize(into);

            return into;
        }

        protected override void deserialize(BaseItem item, List<SyncPropertyNames> changedProperties)
        {
            Exam i = item as Exam;

            base.deserialize(i, changedProperties);
        }
    }
}
