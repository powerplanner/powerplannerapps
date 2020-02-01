using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public class ViewItemHoliday : BaseViewItemHomeworkExamGrade
    {
        public ViewItemHoliday(BaseDataItemHomeworkExamGrade dataItem) : base(dataItem)
        {

        }

        private DateTime _endTime;
        public DateTime EndTime
        {
            get { return _endTime; }
            private set { SetProperty(ref _endTime, value, nameof(EndTime)); }
        }

        public bool IsOnDay(DateTime day)
        {
            return day <= EndTime.Date && day >= Date.Date;
        }

        protected override void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            var megaItem = dataItem as DataItemMegaItem;

            EndTime = DateTime.SpecifyKind(megaItem.EndTime, DateTimeKind.Local);

            base.PopulateFromDataItemOverride(dataItem);
        }
    }
}
