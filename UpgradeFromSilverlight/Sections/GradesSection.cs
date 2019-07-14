using PowerPlannerSending;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using UpgradeFromSilverlight.Model;

namespace UpgradeFromSilverlight.Sections
{
    public class GradesSection : LoadableSection<GradesSection.GradesStorage>
    {
        [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.LoadableSections")]
        public class GradesStorage
        {
            [DataMember]
            public GradeScale[] GradeScales = new GradeScale[0];

            [DataMember]
            public FakeList<WeightCategoryWP> WeightCategories = new FakeList<WeightCategoryWP>();
        }

        private ClassWP _class;
        private AccountSection _accountSection;


        public GradesSection(AccountSection accountSection, string fileName, ClassWP c)
            : base(fileName, accountSection)
        {
            _accountSection = accountSection;
            _class = c;
        }

        protected override void loaded(GradesSection.GradesStorage value)
        {
            Debug.WriteLine("Grades section for " + _class.Name + " loaded");

            foreach (WeightCategoryWP w in value.WeightCategories)
            {
                loaded(w, _class);

                foreach (GradeWP g in w.Grades)
                    loaded(g, w);
            }
        }
    }
}
