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
    public abstract class BaseHomeworkWin : BaseHomeworkExamWin
    {
        private double _percentComplete;
        [DataMember]
        public double PercentComplete
        {
            get { return _percentComplete; }
            set { SetProperty(ref _percentComplete, value, "PercentComplete", "IsComplete"); }
        }

        public bool IsComplete
        {
            get { return PercentComplete == 1; }
            set
            {
                if (value)
                    PercentComplete = 1;
                else
                    PercentComplete = 0;
            }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.PercentComplete.ToString()] = PercentComplete;

            base.serialize(into);
        }

        protected void serialize(BaseHomework into)
        {
            into.PercentComplete = PercentComplete;

            base.serialize(into);
        }

        protected void deserialize(BaseHomework from, List<BaseItemWin.PropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (PercentComplete != from.PercentComplete)
                    changedProperties.Add(BaseItemWin.PropertyNames.PercentComplete);
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
    }
}
