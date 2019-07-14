using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class GradeWP : BaseHomeworkExamGradeWP, IComparable
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Grade; }
        }
        
        [DataMember]
        public double GradeReceived { get; set; }
        
        [DataMember]
        public double GradeTotal { get; set; }
        
        [DataMember]
        public bool IsDropped { get; set; }
        
        [DataMember]
        public double IndividualWeight { get; set; }
        

        public override int CompareTo(object obj)
        {
            if (obj is GradeWP)
            {
                GradeWP other = obj as GradeWP;

                //oldest grades are shown first
                int compare = Date.Date.CompareTo(other.Date.Date);

                if (compare == 0)
                {
                    //dropped items go after
                    if (IsDropped && !other.IsDropped)
                        return 1;

                    //undropped items go before anything dropped
                    if (!IsDropped && other.IsDropped)
                        return -1;

                    //more recently created items appear first if on same day
                    return DateCreated.CompareTo((obj as GradeWP).DateCreated);
                }

                return compare;
            }

            return 0;
        }

        protected override BaseItem serialize(int offset)
        {
            Grade into = new Grade()
            {
                GradeReceived = GradeReceived,
                GradeTotal = GradeTotal,
                IsDropped = IsDropped,
                IndividualWeight = IndividualWeight
            };

            base.serialize(into, offset);

            return into;
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.GradeReceived.ToString()] = GradeReceived;
            into[PropertyNames.GradeTotal.ToString()] = GradeTotal;
            into[PropertyNames.IsDropped.ToString()] = IsDropped;
            into[PropertyNames.IndividualWeight.ToString()] = IndividualWeight;

            base.serialize(into);
        }

        protected override void deserialize(BaseItem item, List<PropertyNames> changedProperties)
        {
            Grade from = item as Grade;

            if (changedProperties != null)
            {
                if (GradeReceived != from.GradeReceived)
                    changedProperties.Add(PropertyNames.GradeReceived);

                if (GradeTotal != from.GradeTotal)
                    changedProperties.Add(PropertyNames.GradeTotal);

                if (IsDropped != from.IsDropped)
                    changedProperties.Add(PropertyNames.IsDropped);

                if (IndividualWeight != from.IndividualWeight)
                    changedProperties.Add(PropertyNames.IndividualWeight);
            }

            GradeReceived = from.GradeReceived;
            GradeTotal = from.GradeTotal;
            IsDropped = from.IsDropped;
            IndividualWeight = from.IndividualWeight;

            base.deserialize(from, changedProperties);
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.GradeReceived:
                    return GradeReceived;

                case PropertyNames.GradeTotal:
                    return GradeTotal;

                case PropertyNames.IsDropped:
                    return IsDropped;

                case PropertyNames.IndividualWeight:
                    return IndividualWeight;
            }

            return base.GetPropertyValue(p);
        }

        public override string ToString()
        {
            return Name;
        }

        public bool IsExtraCredit
        {
            get { return GradeTotal == 0; }
        }
    }
}
