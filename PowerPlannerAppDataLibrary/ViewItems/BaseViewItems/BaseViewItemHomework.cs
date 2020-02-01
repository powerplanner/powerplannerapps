using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewItems.BaseViewItems
{
    public abstract class BaseViewItemHomework : BaseViewItemHomeworkExam
    {
        public BaseViewItemHomework(DataItemMegaItem dataItem) : base(dataItem) { }

        private double _percentComplete;
        public double PercentComplete
        {
            get { return _percentComplete; }
            private set { SetProperty(ref _percentComplete, value, "PercentComplete", "IsComplete"); }
        }

        public new bool IsComplete
        {
            get { return PercentComplete >= 1; }
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            base.PopulateFromDataItemOverride(dataItem);

            DataItemMegaItem i = dataItem as DataItemMegaItem;

            PercentComplete = i.PercentComplete;
        }
    }
}
