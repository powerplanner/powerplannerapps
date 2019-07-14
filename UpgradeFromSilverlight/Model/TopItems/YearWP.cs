using PowerPlannerSending;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UpgradeFromSilverlight.Sections;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class YearWP : BaseItemWithGPACreditsWP
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Year; }
        }

        


        #region DisplayLists
        

        private List<SemesterWP> _semesters;
        public List<SemesterWP> Semesters
        {
            get
            {
                if (_semesters == null)
                {
                    _semesters = new List<SemesterWP>();

                    foreach (SemesterSection section in AccountSection.SemesterSections)
                    {
                        if (section.Value.YearIdentifier == Identifier)
                            _semesters.Add(section.Value);
                    }
                }

                return _semesters;
            }
        }

        #endregion


        protected override BaseItem serialize(int offset)
        {
            Year into = new Year();

            base.serialize(into, offset);

            return into;
        }

        protected override void deserialize(BaseItem item, List<PropertyNames> changedProperties)
        {
            Year i = item as Year;

            base.deserialize(i, changedProperties);
        }


        protected override IEnumerable getGradedChildren()
        {
            return Semesters;
        }

        public override IEnumerable<BaseItemWP> GetChildren()
        {
            return Semesters;
        }

        internal override void Remove(BaseItemWP item)
        {
            //doesn't need to mark changed

            if (_semesters != null)
                _semesters.Remove((SemesterWP)item);
        }

        internal override void Add(BaseItemWP item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Should never be called
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        internal override BaseItemWP FindFromSection(Guid identifier)
        {
            throw new NotImplementedException();
        }

        internal override void Delete(bool permanent)
        {
            base.Delete(permanent);

            //if it's a permanent delete, we need to recursively delete so that their data actually gets deleted
            if (permanent)
                foreach (SemesterWP s in Semesters)
                    s.PermanentDelete();
        }
    }
}
