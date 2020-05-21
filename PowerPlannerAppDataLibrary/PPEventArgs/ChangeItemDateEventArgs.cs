using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.PPEventArgs
{
    public class ChangeItemDateEventArgs
    {
        public ViewItemTaskOrEvent Item { get; private set; }

        public DateTime DesiredDate { get; private set; }

        public ChangeItemDateEventArgs(ViewItemTaskOrEvent item, DateTime desiredDate)
        {
            Item = item;
            DesiredDate = desiredDate;
        }
    }
}
