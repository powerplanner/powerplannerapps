using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromWin8.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model")]
    public abstract class BaseItemWithGPACreditsWin : BaseItemWithImagesWin, IGPACredits
    {
        private double _credits;
        [DataMember]
        public double Credits
        {
            get { return _credits; }
            set { SetProperty(ref _credits, value, "Credits"); }
        }

        private double _gpa;
        [DataMember]
        public double GPA
        {
            get { return _gpa; }
            set { SetProperty(ref _gpa, value, "GPA"); }
        }

        public override void Calculate()
        {
        }

        protected abstract IEnumerable getGradedChildren();
    }
}
