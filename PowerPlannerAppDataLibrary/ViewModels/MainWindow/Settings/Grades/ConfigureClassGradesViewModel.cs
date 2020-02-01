using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades
{
    public class ConfigureClassGradesViewModel : PagedViewModel
    {
        /// <summary>
        /// Windows (and potentially Android) will enable this, since all content will be displayed as popups rather than page navigation as iOS does.
        /// </summary>
        public static bool UsePopups { get; set; } = false;

        public ConfigureClassGradesViewModel(BaseViewModel parent, ViewItemClass c) : base(parent)
        {
            // Only reason this exists is to provide the navigation controls for these sub items
            Navigate(new ConfigureClassGradesListViewModel(this, c));
        }
    }
}
