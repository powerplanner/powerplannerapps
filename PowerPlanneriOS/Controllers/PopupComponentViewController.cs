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

            var commands = ViewModel.Commands;
            if (commands != null && commands.Length > 0)
            {
                NavItem.RightBarButtonItems = commands.Select(i => new UIBarButtonItem(i.Glyph.ToUIBarButtonSystemItem(), (obj, args) => i.Action())).ToArray();
            }

            UpdateNookInsets();

            _nativeComponent = ViewModel.Render(AfterViewChanged);
            _nativeComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            ContentView.Add(_nativeComponent);
            _nativeComponent.StretchWidthAndHeight(ContentView);
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
            ViewModel.UpdateNookInsets(new Vx.Views.Thickness((float)View.SafeAreaInsets.Left, 0, (float)View.SafeAreaInsets.Right, (float)View.SafeAreaInsets.Bottom));
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
