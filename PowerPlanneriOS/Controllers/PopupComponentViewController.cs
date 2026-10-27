using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BareMvvm.Core.ViewModels;
using InterfacesiOS.Controllers;
using InterfacesiOS.Helpers;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using UIKit;
using Vx.iOS;
using Vx.Views;

namespace PowerPlanneriOS.Controllers
{
    public class PopupComponentViewController : PopupViewController<PopupComponentViewModel>
    {
        private iOSNativeComponent _nativeComponent;
        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            Title = ViewModel.Title;

            var backOverride = ViewModel.BackOverride;
            if (backOverride != null)
            {
                BackButtonText = backOverride.Item1;
            }

            UpdateRightBarButtonItems();

            _nativeComponent = ViewModel.Render(AfterViewChanged);
            _nativeComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            ContentView.Add(_nativeComponent);
            _nativeComponent.StretchWidthAndHeight(ContentView);

            // The content host fills the available space and scrolls internally, so it must NOT
            // impose its (now potentially tall) intrinsic content size on the surrounding Auto
            // Layout chain. Otherwise it competes with the nav bar's height at the same default
            // compression-resistance priority and forces the nav bar to collapse. Lowering its
            // compression resistance lets the nav bar keep its natural intrinsic height.
            _nativeComponent.SetContentCompressionResistancePriority((float)UILayoutPriority.DefaultLow, UILayoutConstraintAxis.Vertical);
            _nativeComponent.SetContentCompressionResistancePriority((float)UILayoutPriority.DefaultLow, UILayoutConstraintAxis.Horizontal);

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
        }

        private void UpdateRightBarButtonItems()
        {
            if (ViewModel.Commands != null)
            {
                Toolbar.PrimaryCommands = ViewModel.Commands.OfType<Vx.Views.MenuItem>().ToList();
            }
            else
            {
                Toolbar.PrimaryCommands = new();
            }

            if (ViewModel.SecondaryCommands != null)
            {
                Toolbar.SecondaryCommands = ViewModel.SecondaryCommands.OfType<Vx.Views.MenuItem>().ToList();
            }
            else
            {
                Toolbar.SecondaryCommands = new();
            }

            Toolbar.RenderOnDemand();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.Title):
                    Title = ViewModel.Title;
                    break;

                case nameof(ViewModel.Commands):
                case nameof(ViewModel.SecondaryCommands):
                    UpdateRightBarButtonItems();
                    break;
            }
        }

        protected override void UpdateNookInsetsOnViewModel(Thickness nookInsets)
        {
            ViewModel.UpdateNookInsets(new Vx.Views.Thickness(nookInsets.Left, 0, nookInsets.Right, nookInsets.Bottom));
        }

        private void AfterViewChanged(UIView view)
        {
            if (view is UIScrollView scrollView)
            {
                EnableKeyboardScrollOffsetHandling(scrollView, 0);
            }
        }

        private void PrimaryButton_Clicked(object sender, EventArgs e)
        {
            ViewModel.PrimaryCommand.Click?.Invoke();
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

            ViewModel.GoBack();
        }
    }
}
