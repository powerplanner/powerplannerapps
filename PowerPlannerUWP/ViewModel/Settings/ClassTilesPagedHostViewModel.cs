using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerUWP.ViewModel.Settings
{
    public class ClassTilesPagedHostViewModel : PagedViewModel
    {
        public ClassTilesPagedHostViewModel(BaseViewModel parent) : base(parent)
        {
            Navigate(new ClassTilesViewModel(this));
        }
    }
}
