using System;
using System.Linq;
using BareMvvm.Core.ViewModels;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using ToolsPortable;
using UIKit;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers
{
    public class PopupComponentViewController : PopupViewController<PopupComponentViewModel>
    {
        private object _tabBarHeightListener;
        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            Title = ViewModel.Title;

            var backOverride = ViewModel.BackOverride;
            if (backOverride != null)
            {
                BackButtonText = backOverride.Item1;
            }

            var primaryCommand = ViewModel.PrimaryCommand;
            if (primaryCommand != null)
            {
                PositiveNavBarButton = new PopupRightNavBarButtonItem(primaryCommand.Text, new WeakEventHandler<EventArgs>(PrimaryButton_Clicked).Handler);
            }

            var renderedComponent = ViewModel.Render();
            renderedComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            ContentView.Add(renderedComponent);
            renderedComponent.StretchWidthAndHeight(ContentView);

            //if (ViewModel.FindAncestor<SettingsViewModel>() != null)
            //{
            //    // Accomodate for bottom bar when in settings
            //    MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            //    {
            //        View.RemoveConstraints(View.Constraints);
            //        renderedComponent.StretchWidthAndHeight(View, 0, 0, 0, (float)MainScreenViewController.TAB_BAR_HEIGHT);
            //    });
            //}
        }

        private void PrimaryButton_Clicked(object sender, EventArgs e)
        {
            ViewModel.PrimaryCommand.Action?.Invoke();
        }

        protected override void BackButtonClicked()
        {
            if (ViewModel.BackOverride != null)
            {
                if (ViewModel.BackOverride.Item2 != null)
                {
                    ViewModel.BackOverride.Item2();
                    return;
                }
            }

            base.BackButtonClicked();
        }
    }
}
