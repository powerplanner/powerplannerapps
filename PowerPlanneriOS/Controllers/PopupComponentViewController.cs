using System;
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
    public class PopupComponentViewController : BareMvvmUIViewController<PopupComponentViewModel>
    {
        private object _tabBarHeightListener;
        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            Title = ViewModel.Title;

            var backOverride = ViewModel.BackOverride;
            var cancelButton = new UIBarButtonItem()
            {
                Title = backOverride != null ? backOverride.Item1 : "Back"
            };
            cancelButton.Clicked += new WeakEventHandler<EventArgs>(CancelButton_Clicked).Handler;
            NavigationItem.LeftBarButtonItem = cancelButton;

            var primaryCommand = ViewModel.PrimaryCommand;
            if (primaryCommand != null)
            {
                var saveButton = new UIBarButtonItem()
                {
                    Title = primaryCommand.Text
                };
                saveButton.Clicked += new WeakEventHandler<EventArgs>(PrimaryButton_Clicked).Handler;
                NavigationItem.RightBarButtonItem = saveButton;
            }

            var renderedComponent = ViewModel.Render();
            renderedComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            View.Add(renderedComponent);
            renderedComponent.StretchWidthAndHeight(View);

            if (ViewModel.FindAncestor<SettingsViewModel>() != null)
            {
                // Accomodate for bottom bar when in settings
                MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
                {
                    View.RemoveConstraints(View.Constraints);
                    renderedComponent.StretchWidthAndHeight(View, 0, 0, 0, (float)MainScreenViewController.TAB_BAR_HEIGHT);
                });
            }
        }

        private void PrimaryButton_Clicked(object sender, EventArgs e)
        {
            ViewModel.PrimaryCommand.Action?.Invoke();
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            var backOverride = ViewModel.BackOverride;
            if (backOverride?.Item2 != null)
            {
                backOverride.Item2();
            }
            else
            {
                ViewModel.TryRemoveViewModelViaUserInteraction();
            }
        }
    }
}
