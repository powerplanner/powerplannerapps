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
    public abstract class BaseItemUnderTwoWin : BaseItemWin
    {
        private Guid _secondUpperIdentifier;
        [DataMember]
        public Guid SecondUpperIdentifier
        {
            get { return _secondUpperIdentifier; }
            set { SetProperty(ref _secondUpperIdentifier, value, "SecondUpperIdentifier"); }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.SecondUpperIdentifier.ToString()] = SecondUpperIdentifier;
        }

        protected void serialize(BaseItemUnderTwo into)
        {
            into.SecondUpperIdentifier = SecondUpperIdentifier;
        }

        protected void deserialize(BaseItemUnderTwo from, List<BaseItemWin.PropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (SecondUpperIdentifier != from.SecondUpperIdentifier)
                    changedProperties.Add(BaseItemWin.PropertyNames.SecondUpperIdentifier);
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
