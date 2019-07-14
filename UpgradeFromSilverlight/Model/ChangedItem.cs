using System;
using System.Runtime.Serialization;



namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class ChangedItem
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
    }
}
