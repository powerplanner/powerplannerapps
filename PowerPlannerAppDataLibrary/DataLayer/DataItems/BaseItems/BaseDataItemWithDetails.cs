using PowerPlannerSending;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems
{
    public abstract class BaseDataItemWithDetails : BaseDataItemWithName
    {
        public static readonly DataItemProperty DetailsProperty = DataItemProperty.Register(SyncPropertyNames.Details);

        [Column("Details")]
        public string Details
        {
            // We normalize line breaks for both get and set, since for a while we weren't normalizing when setting,
            // meaning that there's still string values using \r out there and Android would be displaying them incorrectly.
            // Windows uses \r from the TextBox control, hence why we're changing them to \n
            get { return StringTools.NormalizeLineBreaks(GetValue<string>(DetailsProperty, "")); }
            set { SetValue(DetailsProperty, StringTools.NormalizeLineBreaks(value)); }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.Details.ToString()] = Details;

            base.serialize(into);
        }

        protected void serialize(BaseItemWithDetails into)
        {
            into.Details = Details;

            base.serialize(into);
        }

        protected void deserialize(BaseItemWithDetails from, List<SyncPropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (!Details.Equals(from.Details))
                    changedProperties.Add(SyncPropertyNames.Details);
            }

            Details = from.Details;

            base.deserialize(from, changedProperties);
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.Details:
                    return Details;
            }

            return base.GetPropertyValue(p);
        }
    }
}
