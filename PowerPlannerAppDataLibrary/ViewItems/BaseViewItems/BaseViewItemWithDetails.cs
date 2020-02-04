using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PropertyChanged;
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

        [DependsOn("ListItemTertiaryText")]
        public string Details { get; set; }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            BaseDataItemWithDetails i = dataItem as BaseDataItemWithDetails;

            Details = i.Details;
        }
    }
}
