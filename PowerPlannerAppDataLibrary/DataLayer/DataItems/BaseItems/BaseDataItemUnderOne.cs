using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems
{
    public abstract class BaseDataItemUnderOne : BaseDataItem
    {
        public static readonly DataItemProperty UpperIdentifierProperty = DataItemProperty.Register(SyncPropertyNames.UpperIdentifier);

        [Column("UpperIdentifier")]
        public Guid UpperIdentifier
        {
            get { return GetValue<Guid>(UpperIdentifierProperty); }
            set { SetValue(UpperIdentifierProperty, value); }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.UpperIdentifier.ToString()] = UpperIdentifier;
        }

        protected void serialize(BaseItem into)
        {
            into.UpperIdentifier = UpperIdentifier;
        }

        protected override void deserialize(BaseItem from, List<BaseDataItem.SyncPropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (UpperIdentifier != from.UpperIdentifier)
                    changedProperties.Add(BaseDataItem.SyncPropertyNames.UpperIdentifier);
            }

            UpperIdentifier = from.UpperIdentifier;
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.UpperIdentifier:
                    return UpperIdentifier;
            }

            return base.GetPropertyValue(p);
        }
    }
}
