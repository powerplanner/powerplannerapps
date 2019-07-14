using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace UpgradeFromWin8.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model")]
    public class ChangedItem : BindableBase
    {
        private static readonly DateTime NOT_DELETED = DateTime.MinValue;

        public ChangedItem() { }

        [DataMember]
        public bool HasBeenSent;

        [DataMember]
        public DateTime DeletedOn;

        public bool IsDeleted
        {
            get { return !IsUpdated; }
        }

        public bool IsUpdated
        {
            get { return DeletedOn == NOT_DELETED; }
        }

        public void SetUpdated()
        {
            HasBeenSent = false;
            DeletedOn = NOT_DELETED;
        }

        public void SetDeleted(DateTime deletedOn)
        {
            HasBeenSent = false;
            DeletedOn = deletedOn;
        }
    }
}
