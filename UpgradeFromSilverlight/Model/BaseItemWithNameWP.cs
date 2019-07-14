using PowerPlannerSending;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public abstract class BaseItemWithNameWP : BaseItemWP
    {
        [DataMember]
        public string Name { get; set; }

        protected void serialize(BaseItemWithName into, int offset)
        {
            into.Name = Name;
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.Name.ToString()] = Name;
        }

        protected void deserialize(BaseItemWithName from, List<PropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (!Name.Equals(from.Name))
                    changedProperties.Add(PropertyNames.Name);
            }

            Name = from.Name;
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.Name:
                    return Name;
            }

            return base.GetPropertyValue(p);
        }
    }
}
