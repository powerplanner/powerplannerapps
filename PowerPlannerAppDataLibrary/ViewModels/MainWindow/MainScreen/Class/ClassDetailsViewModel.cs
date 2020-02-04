using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewItems;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class
{
    public class ClassDetailsViewModel : BaseClassContentViewModel
    {
        private BindablePropertyWatcher _detailsPropertyWatcher;
        public string Details { get; set; }

        public ClassDetailsViewModel(ClassViewModel parent) : base(parent)
        {
            _detailsPropertyWatcher = new BindablePropertyWatcher(parent.ViewItemsGroupClass.Class, nameof(ViewItemClass.Details), delegate
            {
                Details = parent.ViewItemsGroupClass.Class.Details;
            });

            Details = parent.ViewItemsGroupClass.Class.Details;
        }
    }
}
