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

            _nativeComponent = ViewModel.Render(AfterViewChanged);
            _nativeComponent.TranslatesAutoresizingMaskIntoConstraints = false;
            ContentView.Add(_nativeComponent);
            _nativeComponent.StretchWidthAndHeight(ContentView);
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
