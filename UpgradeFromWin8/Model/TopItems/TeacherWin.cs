using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromWin8.Model.TopItems
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model.TopItems")]
    public class TeacherWin : BaseItemWithImagesWin
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Teacher; }
        }

        [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model.TopItems")]
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

        private PhoneNumber[] _phoneNumbers;
        [DataMember]
        public PhoneNumber[] PhoneNumbers
        {
            get { return _phoneNumbers; }
            set { SetProperty(ref _phoneNumbers, value, "PhoneNumbers"); }
        }

        private EmailAddress[] _emailAddresses;
        [DataMember]
        public EmailAddress[] EmailAddresses
        {
            get { return _emailAddresses; }
            set { SetProperty(ref _emailAddresses, value, "EmailAddresses"); }
        }

        private PostalAddress[] _postalAddresses;
        [DataMember]
        public PostalAddress[] PostalAddresses
        {
            get { return _postalAddresses; }
            set { SetProperty(ref _postalAddresses, value, "PostalAddresses"); }
        }

        private string[] _officeLocations;
        [DataMember]
        public string[] OfficeLocations
        {
            get { return _officeLocations; }
            set { SetProperty(ref _officeLocations, value, "OfficeLocations"); }
        }


        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.PhoneNumbers.ToString()] = PhoneNumbers;
            into[PropertyNames.EmailAddresses.ToString()] = EmailAddresses;
            into[PropertyNames.PostalAddresses.ToString()] = PostalAddresses;
            into[PropertyNames.OfficeLocations.ToString()] = OfficeLocations;

            base.serialize(into);
        }

        protected override PowerPlannerSending.BaseItem serialize()
        {
            Teacher into = new Teacher()
            {
                PhoneNumbers = PhoneNumbers,
                EmailAddresses = EmailAddresses,
                PostalAddresses = PostalAddresses,
                OfficeLocations = OfficeLocations
            };

            base.serialize(into);

            return into;
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

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<PropertyNames> changedProperties)
        {
            Teacher from = item as Teacher;

            if (changedProperties != null)
            {
                if (!EmailAddresses.SequenceEqual(from.EmailAddresses))
                    changedProperties.Add(BaseItemWin.PropertyNames.EmailAddresses);

                if (!OfficeLocations.SequenceEqual(from.OfficeLocations))
                    changedProperties.Add(BaseItemWin.PropertyNames.OfficeLocations);

                if (!PhoneNumbers.SequenceEqual(from.PhoneNumbers))
                    changedProperties.Add(BaseItemWin.PropertyNames.PhoneNumbers);

                if (!PostalAddresses.SequenceEqual(from.PostalAddresses))
                    changedProperties.Add(BaseItemWin.PropertyNames.PostalAddresses);
            }

            this.EmailAddresses = from.EmailAddresses;
            this.OfficeLocations = from.OfficeLocations;
            this.PhoneNumbers = from.PhoneNumbers;
            this.PostalAddresses = from.PostalAddresses;

            //guaranteed that upper temp and perm will be -1
            base.deserialize(from, changedProperties);
        }

        public override IEnumerable<BaseItemWin> GetChildren()
        {
            throw new NotImplementedException();
        }

        internal override void Remove(BaseItemWin item)
        {
            throw new NotImplementedException();
        }

        internal override void Add(BaseItemWin item)
        {
            throw new NotImplementedException();
        }
    }
}
