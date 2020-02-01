using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.PPEventArgs
{
    public class ChangeItemDateEventArgs
    {
        public BaseViewItemHomeworkExam Item { get; private set; }

        public DateTime DesiredDate { get; private set; }

        public ChangeItemDateEventArgs(BaseViewItemHomeworkExam item, DateTime desiredDate)
        {
            Item = item;
            DesiredDate = desiredDate;
        }
    }
}
