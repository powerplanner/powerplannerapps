using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems
{
    public class DataItemWeightCategory : BaseDataItemWithImages
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.WeightCategory; }
        }

        public static readonly DataItemProperty WeightValueProperty = DataItemProperty.Register(SyncPropertyNames.WeightValue);

        [Column("WeightValue")]
        public double WeightValue
        {
            get { return GetValue<double>(WeightValueProperty); }
            set { SetValue(WeightValueProperty, value); }
        }


        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.WeightValue.ToString()] = WeightValue;

            base.serialize(into);
        }

        protected override BaseItem serialize()
        {
            WeightCategory into = new WeightCategory()
            {
                WeightValue = WeightValue
            };

            base.serialize(into);

            return into;
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.WeightValue:
                    return WeightValue;
            }

            return base.GetPropertyValue(p);
        }

        protected override void deserialize(BaseItem item, List<SyncPropertyNames> changedProperties)
        {
            WeightCategory from = item as WeightCategory;

            if (changedProperties != null)
            {
                if (WeightValue != from.WeightValue)
                    changedProperties.Add(SyncPropertyNames.WeightValue);
            }

            WeightValue = from.WeightValue;

            base.deserialize(from, changedProperties);
        }
    }
}
