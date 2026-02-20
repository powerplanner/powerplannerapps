using PowerPlannerSending;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems
{
    public class DataItemTeacher : BaseDataItemWithImages
    {
        public override ItemType ItemType
        {
            get { return PowerPlannerSending.ItemType.Teacher; }
        }

        [DataContract(Namespace = "")]
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

        public static readonly DataItemProperty RawPhoneNumbersProperty = DataItemProperty.Register(SyncPropertyNames.PhoneNumbers);

        [Column("PhoneNumbers")]
        public string RawPhoneNumbers
        {
            get { return GetValue<string>(RawPhoneNumbersProperty); }
            set { SetValue(RawPhoneNumbersProperty, value); }
        }

        public PhoneNumber[] GetPhoneNumbers()
        {
            // Cannot cache these values since the underlying dictionary that stores the properties might change, and there'd be no way of this knowing that it changed
            return DeserializeFromString<PhoneNumber[]>(RawPhoneNumbers);
        }

        public void SetPhoneNumbers(PhoneNumber[] value)
        {
            RawPhoneNumbers = SerializeToString(value);
        }

        public static readonly DataItemProperty RawEmailAddressesProperty = DataItemProperty.Register(SyncPropertyNames.EmailAddresses);

        [Column("EmailAddresses")]
        public string RawEmailAddresses
        {
            get { return GetValue<string>(RawEmailAddressesProperty); }
            set { SetValue(RawEmailAddressesProperty, value); }
        }

        public EmailAddress[] GetEmailAddresses()
        {
            return DeserializeFromString<EmailAddress[]>(RawEmailAddresses);
        }

        public void SetEmailAddresses(EmailAddress[] value)
        {
            RawEmailAddresses = SerializeToString(value);
        }

        public static readonly DataItemProperty RawPostalAddressesProperty = DataItemProperty.Register(SyncPropertyNames.PostalAddresses);

        [Column("PostalAddresses")]
        public string RawPostalAddresses
        {
            get { return GetValue<string>(RawPostalAddressesProperty); }
            set { SetValue(RawPostalAddressesProperty, value); }
        }

        public PostalAddress[] GetPostalAddresses()
        {
            return DeserializeFromString<PostalAddress[]>(RawPostalAddresses);
        }

        public void SetPostalAddresses(PostalAddress[] value)
        {
            RawPostalAddresses = SerializeToString(value);
        }

        public static readonly DataItemProperty RawOfficeLocationsProperty = DataItemProperty.Register(SyncPropertyNames.OfficeLocations);

        [Column("OfficeLocations")]
        public string RawOfficeLocations
        {
            get { return GetValue<string>(RawOfficeLocationsProperty); }
            set { SetValue(RawOfficeLocationsProperty, value); }
        }

        public string[] GetOfficeLocations()
        {
            return DeserializeFromString<string[]>(RawOfficeLocations);
        }

        public void SetOfficeLocations(string[] value)
        {
            RawOfficeLocations = SerializeToString(value);
        }


        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.PhoneNumbers.ToString()] = GetPhoneNumbers();
            into[SyncPropertyNames.EmailAddresses.ToString()] = GetEmailAddresses();
            into[SyncPropertyNames.PostalAddresses.ToString()] = GetPostalAddresses();
            into[SyncPropertyNames.OfficeLocations.ToString()] = GetOfficeLocations();

            base.serialize(into);
        }

        protected override PowerPlannerSending.BaseItem serialize()
        {
            Teacher into = new Teacher()
            {
                PhoneNumbers = GetPhoneNumbers(),
                EmailAddresses = GetEmailAddresses(),
                PostalAddresses = GetPostalAddresses(),
                OfficeLocations = GetOfficeLocations()
            };

            base.serialize(into);

            return into;
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.PhoneNumbers:
                    return GetPhoneNumbers();

                case SyncPropertyNames.EmailAddresses:
                    return GetEmailAddresses();

                case SyncPropertyNames.PostalAddresses:
                    return GetPostalAddresses();

                case SyncPropertyNames.OfficeLocations:
                    return GetOfficeLocations();
            }

            return base.GetPropertyValue(p);
        }

        protected override void deserialize(PowerPlannerSending.BaseItem item, List<SyncPropertyNames> changedProperties)
        {
            Teacher from = item as Teacher;

            if (changedProperties != null)
            {
                if (!GetEmailAddresses().SequenceEqual(from.EmailAddresses))
                    changedProperties.Add(SyncPropertyNames.EmailAddresses);

                if (!GetOfficeLocations().SequenceEqual(from.OfficeLocations))
                    changedProperties.Add(SyncPropertyNames.OfficeLocations);

                if (!GetPhoneNumbers().SequenceEqual(from.PhoneNumbers))
                    changedProperties.Add(SyncPropertyNames.PhoneNumbers);

                if (!GetPostalAddresses().SequenceEqual(from.PostalAddresses))
                    changedProperties.Add(SyncPropertyNames.PostalAddresses);
            }

            SetEmailAddresses(from.EmailAddresses);
            SetOfficeLocations(from.OfficeLocations);
            SetPhoneNumbers(from.PhoneNumbers);
            SetPostalAddresses(from.PostalAddresses);

            //guaranteed that upper temp and perm will be -1
            base.deserialize(from, changedProperties);
        }
    }
}
