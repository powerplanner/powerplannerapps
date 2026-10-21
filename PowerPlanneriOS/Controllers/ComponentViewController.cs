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
            if (ViewModel == null)
            {
                return;
            }

            // Use the WINDOW's safe-area insets rather than this view's. The view's insets depend on
            // where it sits in the hierarchy (a view that doesn't reach the screen edge reports 0),
            // whereas the window always reports the true device insets (notch, home indicator, etc.).
            var insets = GetWindowSafeAreaInsets();
            ViewModel.UpdateNookInsets(new Vx.Views.Thickness((float)insets.Left, (float)insets.Top, (float)insets.Right, (float)insets.Bottom));
        }


        private UIEdgeInsets GetWindowSafeAreaInsets()
        {
            var window = View?.Window ?? PagedViewModelPresenter.GetKeyWindow();
            return window?.SafeAreaInsets ?? UIEdgeInsets.Zero;
        }
    }
}
