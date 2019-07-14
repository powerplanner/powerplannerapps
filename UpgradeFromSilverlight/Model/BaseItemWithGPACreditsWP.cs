using System.Collections;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public abstract class BaseItemWithGPACreditsWP : BaseItemWithImagesWP, IGPACredits
    {
        [DataMember]
        public double Credits { get; set; }
        
        [DataMember]
        public double GPA { get; set; }

        public override void Calculate()
        {
        }

        protected abstract IEnumerable getGradedChildren();
    }
}
