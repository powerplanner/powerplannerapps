using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class CalendarIntegrationViewModel : BaseSettingsSplitViewModel
    {
        public CalendarIntegrationViewModel(BaseViewModel parent) : base(parent)
        {
            Items = new BaseViewModel[]
            {
                new CalendarIntegrationTasksViewModel(this),
                new CalendarIntegrationClassesViewModel(this)
            };
        }
    }
}
