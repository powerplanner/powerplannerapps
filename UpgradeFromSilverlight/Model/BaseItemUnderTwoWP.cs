using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public abstract class BaseItemUnderTwoWP : BaseItemWP
    {
        [DataMember]
        public Guid SecondUpperIdentifier { get; set; }

        protected void serialize(BaseItemUnderTwo into, int offset)
        {
            into.SecondUpperIdentifier = SecondUpperIdentifier;
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.SecondUpperIdentifier.ToString()] = SecondUpperIdentifier;
        }

        protected void deserialize(BaseItemUnderTwo from, List<PropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (SecondUpperIdentifier != from.SecondUpperIdentifier)
                    changedProperties.Add(PropertyNames.SecondUpperIdentifier);
            }

            SecondUpperIdentifier = from.SecondUpperIdentifier;
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.SecondUpperIdentifier:
                    return SecondUpperIdentifier;
            }

            return base.GetPropertyValue(p);
        }
    }
}
