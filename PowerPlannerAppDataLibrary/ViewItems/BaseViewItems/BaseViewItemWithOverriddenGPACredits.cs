using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewItems.BaseViewItems
{
    public abstract class BaseViewItemWithOverriddenGPACredits : BaseViewItemWithImages, IGPACredits
    {
        public BaseViewItemWithOverriddenGPACredits(Guid identifier) : base(identifier) { }
        public BaseViewItemWithOverriddenGPACredits(BaseDataItemWithOverriddenGPACredits dataItem) : base(dataItem) { }

        private double _calculatedGPA = PowerPlannerSending.Grade.UNGRADED;
        public double CalculatedGPA
        {
            get { return _calculatedGPA; }
            protected set { SetProperty(ref _calculatedGPA, value, nameof(CalculatedGPA), nameof(GPA)); }
        }

        public double GPA
        {
            get
            {
                if (OverriddenGPA != PowerPlannerSending.Grade.UNGRADED)
                    return OverriddenGPA;

                return CalculatedGPA;
            }
        }

        private double _calculatedCreditsEarned = PowerPlannerSending.Grade.UNGRADED;
        public double CalculatedCreditsEarned
        {
            get { return _calculatedCreditsEarned; }
            protected set { SetProperty(ref _calculatedCreditsEarned, value, nameof(CalculatedCreditsEarned), nameof(CreditsEarned)); }
        }

        private double _calculatedCreditsAffectingGpa = PowerPlannerSending.Grade.UNGRADED;
        public double CalculatedCreditsAffectingGpa
        {
            get { return _calculatedCreditsAffectingGpa; }
            protected set { SetProperty(ref _calculatedCreditsAffectingGpa, value, nameof(CalculatedCreditsAffectingGpa), nameof(CreditsAffectingGpa)); }
        }

        public double CreditsEarned
        {
            get
            {
                if (OverriddenCredits != PowerPlannerSending.Grade.UNGRADED)
                    return OverriddenCredits;

                return CalculatedCreditsEarned;
            }
        }

        public double CreditsAffectingGpa
        {
            get
            {
                if (OverriddenCredits != PowerPlannerSending.Grade.UNGRADED)
                    return OverriddenCredits;

                return CalculatedCreditsAffectingGpa;
            }
        }

        private double _overriddenGPA = PowerPlannerSending.Grade.UNGRADED;
        public double OverriddenGPA
        {
            get { return _overriddenGPA; }
            protected set { SetProperty(ref _overriddenGPA, value, nameof(OverriddenGPA)); }
        }

        private double _overriddenCredits = PowerPlannerSending.Grade.UNGRADED;
        public double OverriddenCredits
        {
            get { return _overriddenCredits; }
            protected set { SetProperty(ref _overriddenCredits, value, nameof(OverriddenCredits), nameof(CreditsEarned), nameof(CreditsAffectingGpa)); }
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            BaseDataItemWithOverriddenGPACredits i = dataItem as BaseDataItemWithOverriddenGPACredits;

            OverriddenGPA = i.OverriddenGPA;
            OverriddenCredits = i.OverriddenCredits;
        }
    }
}
