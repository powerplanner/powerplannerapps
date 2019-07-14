using PowerPlannerSending;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public abstract class BaseItemWithDetailsWP : BaseItemWithNameWP
    {
        [DataMember]
        public string Details { get; set; }

        protected void serialize(BaseItemWithDetails into, int offset)
        {
            into.Details = Details;

            base.serialize(into, offset);
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.Details.ToString()] = Details;

            base.serialize(into);
        }

        protected void deserialize(BaseItemWithDetails from, List<PropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (!Details.Equals(from.Details))
                    changedProperties.Add(PropertyNames.Details);
            }

            Details = from.Details;

            base.deserialize(from, changedProperties);
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.Details:
                    return Details;
            }

            return base.GetPropertyValue(p);
        }
    }
}
