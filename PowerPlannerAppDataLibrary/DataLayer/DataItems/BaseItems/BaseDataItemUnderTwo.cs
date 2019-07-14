using PowerPlannerSending;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems
{
    public abstract class BaseDataItemUnderTwo : BaseDataItemUnderOne
    {
        public static readonly DataItemProperty SecondUpperIdentifierProperty = DataItemProperty.Register(SyncPropertyNames.SecondUpperIdentifier);

        [Column("SecondUpperIdentifier")]
        public Guid SecondUpperIdentifier
        {
            get { return GetValue<Guid>(SecondUpperIdentifierProperty); }
            set { SetValue(SecondUpperIdentifierProperty, value); }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.SecondUpperIdentifier.ToString()] = SecondUpperIdentifier;
        }

        protected void serialize(BaseItemUnderTwo into)
        {
            into.SecondUpperIdentifier = SecondUpperIdentifier;
        }

        protected void deserialize(BaseItemUnderTwo from, List<BaseDataItem.SyncPropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (SecondUpperIdentifier != from.SecondUpperIdentifier)
                    changedProperties.Add(BaseDataItem.SyncPropertyNames.SecondUpperIdentifier);
            }

            SecondUpperIdentifier = from.SecondUpperIdentifier;
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.SecondUpperIdentifier:
                    return SecondUpperIdentifier;
            }

            return base.GetPropertyValue(p);
        }
    }
}
