using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewItems.BaseViewItems
{
    public abstract class BaseViewItemWithDetails : BaseViewItemWithName
    {
        public BaseViewItemWithDetails(Guid identifier) : base(identifier) { }
        public BaseViewItemWithDetails(BaseDataItemWithDetails dataItem) : base(dataItem)
        {

        }

        private string _details;
        public string Details
        {
            get { return _details; }
            set { SetProperty(ref _details, value, "Details", "ListItemTertiaryText"); }
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            BaseDataItemWithDetails i = dataItem as BaseDataItemWithDetails;

            Details = i.Details;
        }
    }
}
