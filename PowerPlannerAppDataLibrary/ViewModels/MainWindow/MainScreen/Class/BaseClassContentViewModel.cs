using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class
{
    public abstract class BaseClassContentViewModel : BaseMainScreenViewModelDescendant
    {
        public ClassViewModel ClassViewModel { get; private set; }

        public BaseClassContentViewModel(ClassViewModel parent) : base(parent)
        {
            ClassViewModel = parent;
        }
    }
}
