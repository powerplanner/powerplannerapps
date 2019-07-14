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

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value, "Name"); }
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            BaseDataItemWithName i = dataItem as BaseDataItemWithName;

            Name = i.Name;
        }
    }
}
