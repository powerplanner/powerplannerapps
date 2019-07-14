using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UpgradeFromWin8.Model
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/IsolatedClass.Model")]
    public abstract class BaseItemWithImagesWin : BaseItemWithDetailsWin
    {
        private string[] _imageNames = new string[0];
        [DataMember]
        public string[] ImageNames
        {
            get { return _imageNames; }
            set { SetProperty(ref _imageNames, value, "ImageNames"); }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[PropertyNames.ImageNames.ToString()] = ImageNames.ToArray();

            base.serialize(into);
        }

        protected void serialize(BaseItemWithImages into)
        {
            if (ImageNames == null)
                into.ImageNames = new string[0];
            else
                into.ImageNames = ImageNames.ToArray<string>();

            base.serialize(into);
        }

        protected void deserialize(BaseItemWithImages from, List<PropertyNames> changedProperties)
        {
            if (changedProperties != null)
            { 
                if (!ImageNames.SequenceEqual(from.ImageNames))
                    changedProperties.Add(PropertyNames.ImageNames);
            }

            ImageNames = from.ImageNames;

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
    }
}
