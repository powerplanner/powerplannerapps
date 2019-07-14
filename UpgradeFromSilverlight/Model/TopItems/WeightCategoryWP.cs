using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class WeightCategoryWP : BaseItemWithImagesWP
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.WeightCategory; }
        }

        public WeightCategoryWP()
        {

            Grades = new FakeList<GradeWP>();
        }
        
        
        [DataMember]
        public double WeightValue { get; set; }

        protected override BaseItem serialize(int offset)
        {
            WeightCategory into = new WeightCategory()
            {
                WeightValue = WeightValue
            };

            base.serialize(into, offset);

            return into;
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.WeightValue.ToString()] = WeightValue;

            base.serialize(into);
        }

        protected override void deserialize(BaseItem item, List<PropertyNames> changedProperties)
        {
            WeightCategory from = item as WeightCategory;

            if (changedProperties != null)
            {
                if (WeightValue != from.WeightValue)
                    changedProperties.Add(PropertyNames.WeightValue);
            }

            WeightValue = from.WeightValue;

            base.deserialize(from, changedProperties);
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.WeightValue:
                    return WeightValue;
            }

            return base.GetPropertyValue(p);
        }

        

        #region DisplayLists

        [DataMember]
        public FakeList<GradeWP> Grades { get; set; }

        #endregion


        

        public override IEnumerable<BaseItemWP> GetChildren()
        {
            return Grades;
        }

        internal override void Delete(bool permanent)
        {
            throw new NotImplementedException();
        }

        internal override void Add(BaseItemWP item)
        {
            throw new NotImplementedException();
        }

        internal override void Remove(BaseItemWP item)
        {
            throw new NotImplementedException();
        }

        internal override BaseItemWP FindFromSection(Guid identifier)
        {
            foreach (GradeWP g in Grades)
                if (g.Identifier.Equals(identifier))
                    return g;

            return null;
        }
    }
}
