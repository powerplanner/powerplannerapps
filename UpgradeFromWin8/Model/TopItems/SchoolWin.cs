using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsUniversal;

namespace UpgradeFromWin8.Model.TopItems
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model.TopItems")]
    public class SchoolWP : BaseItemWithGPACreditsWin
    {
        public override PowerPlannerSending.ItemType ItemType
        {
            get { throw new NotImplementedException(); }
        }

        public SchoolWP()
        {
            Years = new MyObservableList<YearWin>();
        }

        [DataMember]
        public MyObservableList<YearWin> Years { get; set; }

        private Guid _activeSemesterIdentifier;
        [DataMember]
        public Guid ActiveSemesterIdentifier
        {
            get { return _activeSemesterIdentifier; }
            set { SetProperty(ref _activeSemesterIdentifier, value, "ActiveSemesterIdentifier"); }
        }

        protected override System.Collections.IEnumerable getGradedChildren()
        {
            return Years;
        }

        protected override PowerPlannerSending.BaseItem serialize()
        {
            throw new NotImplementedException();
        }

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<BaseItemWin.PropertyNames> changedProperties)
        {
            throw new NotImplementedException();
        }







        public override IEnumerable<BaseItemWin> GetChildren()
        {
            return Years;
        }

        internal override void Add(BaseItemWin item)
        {
            Years.InsertSorted((YearWin)item);
        }

        internal override void Remove(BaseItemWin item)
        {
            Years.Remove((YearWin)item);
        }

        internal override void Delete(bool permanent)
        {
            throw new NotImplementedException("Delete should never be called on SchoolWP");
        }
    }
}
