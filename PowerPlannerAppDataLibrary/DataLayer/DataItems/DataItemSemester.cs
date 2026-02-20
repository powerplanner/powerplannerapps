using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems
{
    [DataContract]
    public class DataItemSemester : BaseDataItemWithOverriddenGPACredits
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Semester; }
        }

        public static readonly DataItemProperty StartProperty = DataItemProperty.Register(SyncPropertyNames.Start);

        [Column("Start")]
        public DateTime Start
        {
            get { return GetValue<DateTime>(StartProperty, DateValues.UNASSIGNED); }
            set { SetValue(StartProperty, value); }
        }

        public static readonly DataItemProperty EndProperty = DataItemProperty.Register(SyncPropertyNames.End);

        [Column("End")]
        public DateTime End
        {
            get { return GetValue<DateTime>(EndProperty, DateValues.UNASSIGNED); }
            set { SetValue(EndProperty, value); }
        }



        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.Start.ToString()] = Start;
            into[SyncPropertyNames.End.ToString()] = End;

            base.serialize(into);
        }

        protected override BaseItem serialize()
        {
            Semester into = new Semester()
            {
                Start = Start,
                End = End
            };

            base.serialize(into);

            return into;
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.Start:
                    return Start;

                case SyncPropertyNames.End:
                    return End;
            }

            return base.GetPropertyValue(p);
        }

        protected override void deserialize(BaseItem item, List<SyncPropertyNames> changedProperties)
        {
            Semester i = item as Semester;

            if (changedProperties != null)
            {
                if (Start != i.Start.ToUniversalTime())
                    changedProperties.Add(SyncPropertyNames.Start);

                if (End != i.End.ToUniversalTime())
                    changedProperties.Add(SyncPropertyNames.End);
            }

            Start = i.Start.ToUniversalTime();
            End = i.End.ToUniversalTime();

            base.deserialize(i, changedProperties);
        }

    }
}
