using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public class TeacherWP : BaseItemWithImagesWP
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Teacher; }
        }

        [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
        public class SerializedTeacher
        {
            [DataMember]
            public PhoneNumber[] PhoneNumbers;

            [DataMember]
            public EmailAddress[] EmailAddresses;

            [DataMember]
            public PostalAddress[] PostalAddresses;

            [DataMember]
            public string[] OfficeLocations;
        }
        
        [DataMember]
        public PhoneNumber[] PhoneNumbers { get; set; }
        
        [DataMember]
        public EmailAddress[] EmailAddresses { get; set; }
        
        [DataMember]
        public PostalAddress[] PostalAddresses { get; set; }

        [DataMember]
        public string[] OfficeLocations { get; set; }


        protected override PowerPlannerSending.BaseItem serialize(int offset)
        {
            Teacher into = new Teacher()
            {
                PhoneNumbers = PhoneNumbers,
                EmailAddresses = EmailAddresses,
                PostalAddresses = PostalAddresses,
                OfficeLocations = OfficeLocations
            };

            base.serialize(into, offset);

            return into;
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.PhoneNumbers.ToString()] = PhoneNumbers;
            into[PropertyNames.EmailAddresses.ToString()] = EmailAddresses;
            into[PropertyNames.PostalAddresses.ToString()] = PostalAddresses;
            into[PropertyNames.OfficeLocations.ToString()] = OfficeLocations;

            base.serialize(into);
        }

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<PropertyNames> changedProperties)
        {
            Teacher from = item as Teacher;

            if (changedProperties != null)
            {
                if (!EmailAddresses.SequenceEqual(from.EmailAddresses))
                    changedProperties.Add(PropertyNames.EmailAddresses);

                if (!OfficeLocations.SequenceEqual(from.OfficeLocations))
                    changedProperties.Add(PropertyNames.OfficeLocations);

                if (!PhoneNumbers.SequenceEqual(from.PhoneNumbers))
                    changedProperties.Add(PropertyNames.PhoneNumbers);

                if (!PostalAddresses.SequenceEqual(from.PostalAddresses))
                    changedProperties.Add(PropertyNames.PostalAddresses);
            }

            this.EmailAddresses = from.EmailAddresses;
            this.OfficeLocations = from.OfficeLocations;
            this.PhoneNumbers = from.PhoneNumbers;
            this.PostalAddresses = from.PostalAddresses;

            //guaranteed that upper temp and perm will be -1
            base.deserialize(from, changedProperties);
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.PhoneNumbers:
                    return PhoneNumbers;

                case PropertyNames.EmailAddresses:
                    return EmailAddresses;

                case PropertyNames.PostalAddresses:
                    return PostalAddresses;

                case PropertyNames.OfficeLocations:
                    return OfficeLocations;
            }

            return base.GetPropertyValue(p);
        }

        public override IEnumerable<BaseItemWP> GetChildren()
        {
            throw new NotImplementedException();
        }

        internal override void Remove(BaseItemWP item)
        {
            throw new NotImplementedException();
        }

        internal override void Add(BaseItemWP item)
        {
            throw new NotImplementedException();
        }

        internal override BaseItemWP FindFromSection(Guid identifier)
        {
            throw new NotImplementedException();
        }
    }
}
