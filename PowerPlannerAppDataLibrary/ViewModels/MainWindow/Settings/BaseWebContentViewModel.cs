using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public abstract class BaseWebContentViewModel : BaseViewModel
    {
        /// <summary>
        /// The title displayed on the popup
        /// </summary>
        public abstract string Title { get; }

        public abstract string WebsiteToDisplay { get; }

        /// <summary>
        /// Fallback text if offline
        /// </summary>
        public virtual string FallbackText { get; }

        public BaseWebContentViewModel(BaseViewModel parent) : base(parent)
        {
        }
    }
}
