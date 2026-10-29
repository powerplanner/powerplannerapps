using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class
{
    public abstract class BaseClassContentViewModel : ComponentViewModel
    {
        public ClassViewModel ClassViewModel { get; private set; }

        public BaseClassContentViewModel(ClassViewModel parent) : base(parent)
        {
            ClassViewModel = parent;
        }

        /// <summary>
        /// Additionally exclude top inset, which is handled by the main class view.
        /// </summary>
        public override Vx.Views.Thickness NookInsets
        {
            get
            {
                var insets = base.NookInsets;
                return new Vx.Views.Thickness(insets.Left, 0, insets.Right, insets.Bottom);
            }
        }
    }
}
