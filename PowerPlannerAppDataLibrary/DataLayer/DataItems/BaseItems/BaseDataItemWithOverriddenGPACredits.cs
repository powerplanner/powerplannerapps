using PowerPlannerSending;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems
{
    /// <summary>
    /// This class doesn't serialize/deserialize any properties, since Credits and GPA might be calculated properties (like for Semesters/years, they're calculated)
    /// </summary>
    public abstract class BaseDataItemWithOverriddenGPACredits : BaseDataItemWithImages
    {
        public static readonly DataItemProperty OverriddenCreditsProperty = DataItemProperty.Register(SyncPropertyNames.OverriddenCredits);

        [Column("OverriddenCredits")]
        public double OverriddenCredits
        {
            get { return GetValue<double>(OverriddenCreditsProperty, Grade.UNGRADED); }
            set { SetValue(OverriddenCreditsProperty, value); }
        }

        public static readonly DataItemProperty OverriddenGPAProperty = DataItemProperty.Register(SyncPropertyNames.OverriddenGPA);

        [Column("OverriddenGPA")]
        public double OverriddenGPA
        {
            get { return GetValue<double>(OverriddenGPAProperty, Grade.UNGRADED); }
            set { SetValue(OverriddenGPAProperty, value); }
        }

        protected override void serialize(Dictionary<string, object> into)
        {
            into[SyncPropertyNames.OverriddenGPA.ToString()] = OverriddenGPA;
            into[SyncPropertyNames.OverriddenCredits.ToString()] = OverriddenCredits;

            base.serialize(into);
        }

        protected void deserialize(BaseItemWithOverriddenGPACredits from, List<SyncPropertyNames> changedProperties)
        {
            if (changedProperties != null)
            {
                if (OverriddenGPA != from.OverriddenGPA)
                    changedProperties.Add(SyncPropertyNames.OverriddenGPA);

                if (OverriddenCredits != from.OverriddenCredits)
                    changedProperties.Add(SyncPropertyNames.OverriddenCredits);
            }

            OverriddenGPA = from.OverriddenGPA;
            OverriddenCredits = from.OverriddenCredits;

            base.deserialize(from, changedProperties);
        }

        public override object GetPropertyValue(SyncPropertyNames p)
        {
            switch (p)
            {
                case SyncPropertyNames.OverriddenGPA:
                    return OverriddenGPA;

                case SyncPropertyNames.OverriddenCredits:
                    return OverriddenCredits;
            }

            return base.GetPropertyValue(p);
        }
    }
}
