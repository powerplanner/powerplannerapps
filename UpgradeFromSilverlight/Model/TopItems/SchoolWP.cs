using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class SchoolWP : BaseItemWithGPACreditsWP
    {
        public override ItemType ItemType
        {
            get { throw new NotImplementedException(); }
        }

        public SchoolWP()
        {
            Years = new FakeList<YearWP>();
        }

        [DataMember]
        public FakeList<YearWP> Years { get; set; }
        
        [DataMember]
        public Guid ActiveSemesterIdentifier { get; set; }

        protected override System.Collections.IEnumerable getGradedChildren()
        {
            return Years;
        }

        protected override PowerPlannerSending.BaseItem serialize(int offset)
        {
            throw new NotImplementedException();
        }

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<PropertyNames> changedProperties)
        {
            throw new NotImplementedException();
        }



        


        

        public override IEnumerable<BaseItemWP> GetChildren()
        {
            return Years;
        }

        internal override BaseItemWP FindFromSection(Guid identifier)
        {
            return Years.FirstOrDefault(i => i.Identifier.Equals(identifier));
        }

        internal override void Add(BaseItemWP item)
        {
            throw new NotImplementedException();
        }

        internal override void Remove(BaseItemWP item)
        {
            throw new NotImplementedException();
        }

        internal override void Delete(bool permanent)
        {
            throw new NotImplementedException("Delete should never be called on SchoolWP");
        }
    }
}
