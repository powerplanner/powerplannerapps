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
    [DataContract]
    public class DataItemTeacherUnderSchedule : BaseDataItemUnderTwo
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.TeacherUnderSchedule; }
        }

        protected override PowerPlannerSending.BaseItem serialize()
        {
            TeacherUnderSchedule into = new TeacherUnderSchedule();

            base.serialize(into);

            return into;
        }

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<SyncPropertyNames> changedProperties)
        {
            TeacherUnderSchedule from = item as TeacherUnderSchedule;

            base.deserialize(from, changedProperties);
        }
    }
}
