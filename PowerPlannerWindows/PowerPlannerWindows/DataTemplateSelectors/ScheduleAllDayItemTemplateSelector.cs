using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PowerPlannerUWP.DataTemplateSelectors
{
    public class ScheduleAllDayItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate HolidayTemplate { get; set; }
        public DataTemplate ItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is ViewItemHoliday)
            {
                return HolidayTemplate;
            }

            return ItemTemplate;
        }
    }
}
