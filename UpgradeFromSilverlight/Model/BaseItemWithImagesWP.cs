using PowerPlannerSending;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace UpgradeFromSilverlight.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PowerPlannerPortableWP.Model")]
    public abstract class BaseItemWithImagesWP : BaseItemWithDetailsWP
    {
        [DataMember]
        public string[] ImageNames { get; set; } = new string[0];

        protected void serialize(BaseItemWithImages into, int offset)
        {
            into.ImageNames = ImageNames.ToArray<string>();

            base.serialize(into, offset);
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.ImageNames.ToString()] = ImageNames.ToArray();

            base.serialize(into);
        }

        protected void deserialize(BaseItemWithImages from, List<PropertyNames> changedProperties)
        {
            if (!ImageNames.SequenceEqual(from.ImageNames))
            {
                if (changedProperties != null)
                    changedProperties.Add(PropertyNames.ImageNames);

                ImageNames = from.ImageNames;
            }


            base.deserialize(from, changedProperties);
        }

        public override object GetPropertyValue(PropertyNames p)
        {
            switch (p)
            {
                case PropertyNames.ImageNames:
                    return ImageNames.ToArray();
            }

            return base.GetPropertyValue(p);
        }

        public bool HasImages
        {
            get { return ImageNames != null && ImageNames.Length > 0; }
        }

        public string ImageAttachmentLeadingString
        {
            get
            {
                return HasImages ? "\uD83D\uDCF7 " : "";
            }
        }
    }
}
