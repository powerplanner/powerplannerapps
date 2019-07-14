using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public abstract class BaseHomeworkWP : BaseHomeworkExamWP
    {
        [DataMember]
        public double PercentComplete { get; set; }

        protected void serialize(BaseHomework into, int offset)
        {
            into.PercentComplete = PercentComplete;

            base.serialize(into, offset);
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.PercentComplete.ToString()] = PercentComplete;

            base.serialize(into);
        }

        protected void deserialize(BaseHomework from, List<PropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (PercentComplete != from.PercentComplete)
                    changedProperties.Add(PropertyNames.PercentComplete);
            }


            PercentComplete = from.PercentComplete;

            base.deserialize(from, changedProperties);
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.PercentComplete:
                    return PercentComplete;
            }

            return base.GetPropertyValue(p);
        }

        public override bool IsActive(DateTime today)
        {
            return true;
        }
    }
}
