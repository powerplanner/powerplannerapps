using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems
{
    public abstract class BaseDataItemWithName : BaseDataItemUnderOne
    {
        public static readonly DataItemProperty NameProperty = DataItemProperty.Register(SyncPropertyNames.Name);

        public string Name
        {
            get { return GetValue<string>(NameProperty, ""); }
            set { SetValue(NameProperty, value); }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.Name.ToString()] = Name;

            base.serialize(into);
        }

        protected void serialize(BaseItemWithName into)
        {
            into.Name = Name;

            base.serialize(into);
        }

        protected void deserialize(BaseItemWithName from, List<BaseDataItem.SyncPropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (!Name.Equals(from.Name))
                    changedProperties.Add(BaseDataItem.SyncPropertyNames.Name);
            }

            Name = from.Name;

            base.deserialize(from, changedProperties);
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.Name:
                    return Name;
            }

            return base.GetPropertyValue(p);
        }
    }
}
