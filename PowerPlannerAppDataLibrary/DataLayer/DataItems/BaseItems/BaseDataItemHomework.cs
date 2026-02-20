using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems
{
    [Obsolete("Legacy type no longer used, replaced by DataItemMegaItem. Kept around just for data upgrade purposes.")]
    public abstract class BaseDataItemHomework : BaseDataItemHomeworkExam
    {
        public static readonly DataItemProperty PercentCompleteProperty = DataItemProperty.Register(SyncPropertyNames.PercentComplete);
        
        [Column("PercentComplete")]
        public double PercentComplete
        {
            get { return GetValue<double>(PercentCompleteProperty, 0); }
            set { SetValue(PercentCompleteProperty, value); }
        }
        
        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.PercentComplete.ToString()] = PercentComplete;
            
            base.serialize(into);
        }

        protected void serialize(BaseHomework into)
        {
            into.PercentComplete = PercentComplete;

            base.serialize(into);
        }

        protected void deserialize(BaseHomework from, List<BaseDataItem.SyncPropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (PercentComplete != from.PercentComplete)
                    changedProperties.Add(BaseDataItem.SyncPropertyNames.PercentComplete);
            }


            PercentComplete = from.PercentComplete;

            base.deserialize(from, changedProperties);
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.PercentComplete:
                    return PercentComplete;
            }

            return base.GetPropertyValue(p);
        }
    }
}
