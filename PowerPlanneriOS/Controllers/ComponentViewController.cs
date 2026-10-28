using System;
using Foundation;
using UIKit;
using InterfacesiOS.Controllers;
using InterfacesiOS.Helpers;
using InterfacesiOS.Views;
using InterfacesiOS.ViewModelPresenters;
using PowerPlannerAppDataLibrary.ViewModels;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers
{
    public class ComponentViewController : BareMvvmUIViewController<ComponentViewModel>
    {
        public ComponentViewController() : base(renderViewModel: true)
        {
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            Title = ViewModel.Title;
        }
    }
}
