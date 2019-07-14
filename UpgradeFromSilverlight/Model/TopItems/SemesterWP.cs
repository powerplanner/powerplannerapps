using PowerPlannerSending;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ToolsPortable;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class SemesterWP : BaseItemWithGPACreditsWP, IGPACredits
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Semester; }
        }

        public SemesterWP()
        {
            Classes = new FakeList<ClassWP>();
            Tasks = new FakeList<TaskWP>();
        }
        
        [DataMember]
        public Guid YearIdentifier { get; set; }
        
        [DataMember]
        public DateTime Start { get; set; }
        
        [DataMember]
        public DateTime End { get; set; }
        

        [DataMember]
        public FakeList<ClassWP> Classes { get; set; }
        
        [DataMember]
        public FakeList<TaskWP> Tasks { get; set; }

        
        

        protected override BaseItem serialize(int offset)
        {
            Semester into = new Semester()
            {
                Start = Start,
                End = End
            };

            base.serialize(into, offset);

            return into;
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.Start.ToString()] = Start;
            into[PropertyNames.End.ToString()] = End;

            base.serialize(into);
        }

        protected override void deserialize(BaseItem item, List<PropertyNames> changedProperties)
        {
            Semester i = item as Semester;

            if (changedProperties != null)
            {
                if (Start != i.Start.ToUniversalTime())
                    changedProperties.Add(PropertyNames.Start);

                if (End != i.End.ToUniversalTime())
                    changedProperties.Add(PropertyNames.End);
            }

            Start = i.Start.ToUniversalTime();
            End = i.End.ToUniversalTime();

            base.deserialize(i, changedProperties);
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.Start:
                    return Start;

                case PropertyNames.End:
                    return End;
            }

            return base.GetPropertyValue(p);
        }

        protected override IEnumerable getGradedChildren()
        {
            return Classes;
        }

        public override IEnumerable<BaseItemWP> GetChildren()
        {
            return new IEnumerableLinker<BaseItemWP>(Classes, Tasks);
        }

        internal override void Delete(bool permanent)
        {
            if (permanent)
                PermanentDelete();

            base.Delete(permanent);
        }

        internal void PermanentDelete()
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
            foreach (ClassWP c in Classes)
            {
                if (c.Identifier.Equals(identifier))
                    return c;

                BaseItemWP found = c.FindFromSection(identifier);
                if (found != null)
                    return found;
            }

            foreach (TaskWP t in Tasks)
            {
                if (t.Identifier.Equals(identifier))
                    return t;
            }

            return null;
        }
    }
}
