using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Uwp;

namespace PowerPlannerUWP.Views
{
    public class ComponentView : ViewHostGeneric
    {
        public new ComponentViewModel ViewModel
        {
            get => base.ViewModel as ComponentViewModel;
            set => base.ViewModel = value;
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            Content = ViewModel.Render();
        }
    }
}
