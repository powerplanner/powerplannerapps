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

        private object _tabBarHeightListener;
        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            Title = ViewModel.Title;

            var renderedComponent = ViewModel.Render();
            renderedComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            View.Add(renderedComponent);

            // In general these views are used on the home screens where I need to adjust for the bottom tab bar
            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                View.RemoveConstraints(View.Constraints);
                renderedComponent.StretchWidthAndHeight(View, 0, 0, 0, (float)MainScreenViewController.TAB_BAR_HEIGHT);
            });
        }
    }
}
