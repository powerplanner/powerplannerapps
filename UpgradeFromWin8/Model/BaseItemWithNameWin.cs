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
    public abstract class BaseItemWithNameWin : BaseItemWin
    {
        private string _name;
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value, "Name"); }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.Name.ToString()] = Name;
        }

        protected void serialize(BaseItemWithName into)
        {
            into.Name = Name;
        }

        protected void deserialize(BaseItemWithName from, List<BaseItemWin.PropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (!Name.Equals(from.Name))
                    changedProperties.Add(BaseItemWin.PropertyNames.Name);
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
