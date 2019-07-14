using PowerPlannerSending;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsUniversal;

namespace UpgradeFromWin8.Model.TopItems
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model.TopItems")]
    public class YearWin : BaseItemWithGPACreditsWin
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Year; }
        }

        private bool _needsRecalc;

        //public override BaseItemWin Parent
        //{
        //    get
        //    {
        //        return Account.School;
        //    }
        //    set
        //    {
        //        //nothing
        //    }
        //}

        public string SubTitle
        {
            get
            {
                string str = "Credits: ";

                if (Credits == -1)
                    str += "NONE";
                else
                    str += Credits.ToString("0.#");

                if (Semesters.Count > 0)
                    str += " -";

                for (int i = 0; i < Semesters.Count; i++)
                    str += ' ' + Semesters[i].Name + ",";

                return str.TrimEnd(',');
            }
        }






        #region DisplayLists

        //private MyObservableList<SemesterWin> _semesters;
        ///// <summary>
        ///// This will load each semester file. Have to load them all to display since the sorted order depends on values inside each semester
        ///// </summary>
        //public MyObservableList<SemesterWin> Semesters
        //{
        //    get
        //    {
        //        if (_semesters == null)
        //        {
        //            _semesters = new MyObservableList<SemesterWin>();
        //            _semesters.InsertSorted(semesterSections.Select(i => i.Value));
        //        }

        //        return _semesters;
        //    }
        //}

        //private List<SemesterSection> _semesterSections;
        //private List<SemesterSection> semesterSections
        //{
        //    get
        //    {
        //        if (_semesterSections == null)
        //        {
        //            _semesterSections = new List<SemesterSection>();

        //            //gets a list of semester folders, identified by their GUID
        //            foreach (string directory in IMyStorage.GetDirectoryNames(Files.YEAR_FOLDER(LocalAccountId, Identifier)))
        //            {
        //                //TODO - does the directory name include the slash at the end?
        //                _semesterSections.Add(new SemesterSection(LocalAccountId, this, Guid.Parse(directory)));
        //            }
        //        }

        //        return _semesterSections;
        //    }
        //}

        private MyObservableList<SemesterWin> _semesters = new MyObservableList<SemesterWin>();
        [DataMember]
        public MyObservableList<SemesterWin> Semesters
        {
            get
            {
                return _semesters;
            }
            set { _semesters = value; }
        }

        #endregion


        protected override BaseItem serialize()
        {
            Year into = new Year();

            base.serialize(into);

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
        
        public override IEnumerable<BaseItemWin> GetChildren()
        {
            return Semesters;
        }

        internal override void Remove(BaseItemWin item)
        {
            Semesters.Remove((SemesterWin)item);
        }

        internal override void Add(BaseItemWin item)
        {
            SemesterWin semester = (SemesterWin)item;
            semester.Parent = this;

            Semesters.InsertSorted(semester);
        }
    }
}
