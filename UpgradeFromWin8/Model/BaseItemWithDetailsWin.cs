using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromWin8.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model")]
    public abstract class BaseItemWithDetailsWin : BaseItemWithNameWin
    {
        private string _details;
        [DataMember]
        public string Details
        {
            get { return _details; }
            set { SetProperty(ref _details, value, "Details"); }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.Details.ToString()] = Details;

            base.serialize(into);
        }

        protected void serialize(BaseItemWithDetails into)
        {
            into.Details = Details;

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
