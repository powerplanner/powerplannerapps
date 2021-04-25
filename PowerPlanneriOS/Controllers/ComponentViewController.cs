using System;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewModels;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers
{
    public class ComponentViewController : BareMvvmUIViewController<ComponentViewModel>
    {
        public ComponentViewController()
        {
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            Title = ViewModel.Title;

            var renderedComponent = ViewModel.Render();
            renderedComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            View.Add(renderedComponent);
            renderedComponent.StretchWidthAndHeight(View);
        }
    }
}
