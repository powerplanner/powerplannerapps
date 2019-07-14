using PowerPlannerSending;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems
{
    [DataContract]
    public abstract class BaseDataItemWithImages : BaseDataItemWithDetails
    {
        public static readonly DataItemProperty RawImageNamesProperty = DataItemProperty.Register(SyncPropertyNames.ImageNames);


        /// <summary>
        /// DON'T USE THIS, it's simply here for storing in the DB. Use ImageNames instead.
        /// </summary>
        [Column("ImageNames")]
        public string RawImageNames
        {
            get { return GetValue<string>(RawImageNamesProperty); }
            set { SetValue(RawImageNamesProperty, value); }
        }


        [SQLite.Ignore]
        public string[] ImageNames
        {
            get
            {
                if (string.IsNullOrEmpty(RawImageNames))
                    return new string[0];

                return RawImageNames.Split(',');
            }

            set
            {
                if (value == null || value.Length == 0)
                    RawImageNames = null;

                else
                    RawImageNames = StringTools.ToString(value, ",");
            }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.ImageNames.ToString()] = ImageNames.ToArray();

            base.serialize(into);
        }

        protected void serialize(BaseItemWithImages into)
        {
            into.ImageNames = ImageNames.ToArray();

            base.serialize(into);
        }

        protected void deserialize(BaseItemWithImages from, List<SyncPropertyNames> changedProperties)
        {
            if (changedProperties != null)
            { 
                if (!ImageNames.SequenceEqual(from.ImageNames))
                    changedProperties.Add(SyncPropertyNames.ImageNames);
            }

            ImageNames = from.ImageNames.ToArray();

            base.deserialize(from, changedProperties);
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.ImageNames:
                    return ImageNames.ToArray();
            }

            return base.GetPropertyValue(p);
        }
    }
}
