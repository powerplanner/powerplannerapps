using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewItems.BaseViewItems
{
    public abstract class BaseViewItemWithName : BaseViewItem
    {
        public BaseViewItemWithName(Guid identifier) : base(identifier) { }
        public BaseViewItemWithName(BaseDataItemWithName dataItem) : base(dataItem)
        {

        }

        public string Name { get; set; }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            BaseDataItemWithName i = dataItem as BaseDataItemWithName;

            Name = i.Name;
        }
    }
}
