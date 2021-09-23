using System;
using InterfacesiOS.Controllers;
using InterfacesiOS.Helpers;
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

            UpdateNookInsets();

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

        /// <summary>
        /// This method only exists on iOS 11+, not sure if this class will still work on iOS 10/9, but not sure how to dynamically overload a method...
        /// </summary>
        public override void ViewSafeAreaInsetsDidChange()
        {
            if (ViewModel != null)
            {
                UpdateNookInsets();
            }
        }

        private void UpdateNookInsets()
        {
            if (SdkSupportHelper.IsSafeAreaInsetsSupported)
            {
                ViewModel.UpdateNookInsets(new Vx.Views.Thickness((float)View.SafeAreaInsets.Left, 0, (float)View.SafeAreaInsets.Right, (float)View.SafeAreaInsets.Bottom));
            }
        }
    }
}
