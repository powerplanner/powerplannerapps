using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsUniversal;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PowerPlannerUWP.Views
{
    public class MainScreenContentViewHostGeneric : ViewHostGeneric
    {
        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            ViewModel.NavigatedTo += new WeakEventHandler(ViewModel_NavigatedTo).Handler;
            UpdateCommandBar();
        }

        private void ViewModel_NavigatedTo(object sender, EventArgs e)
        {
            UpdateCommandBar();
        }

        private void UpdateCommandBar()
        {
            //_mainScreenView = (ViewModel.FindAncestor<MainScreenViewModel>().GetNativeView() as MainScreenView);
            
            //if (_isCommandBarHidden)
            //{
            //    _mainScreenView.HideCommandBar();
            //}
            //else
            //{
            //    _mainScreenView.ShowCommandBar();
            //}
            //_mainScreenView.SetCommandBarContent(_commandBarContent);
            //_mainScreenView.SetCommandBarCommands(_primaryCommands, _secondaryCommands);
        }

        protected static AppBarButton CreateAppBarButton(Symbol symbol, string label, RoutedEventHandler onClick)
        {
            AppBarButton b = new AppBarButton()
            {
                Icon = new SymbolIcon(symbol),
                Label = label
            };

            b.Click += onClick;

            return b;
        }

        private ICommandBarElement[] _primaryCommands;
        private ICommandBarElement[] _secondaryCommands;
        private UIElement _commandBarContent;
        private bool _isCommandBarHidden;
        private MainScreenView _mainScreenView;

        protected void SetCommandBarCommands(ICommandBarElement[] commands, ICommandBarElement[] secondaryCommands)
        {
            //_primaryCommands = commands;
            //_secondaryCommands = secondaryCommands;

            //if (ViewModel != null && ViewModel.IsCurrentNavigatedPage)
            //{
            //    _mainScreenView.SetCommandBarCommands(commands, secondaryCommands);
            //}

            //ShowCommandBar();
        }
        
        protected void SetCommandBarPrimaryCommands(params ICommandBarElement[] commands)
        {
            SetCommandBarCommands(commands, null);
        }

        protected void SetCommandBarContent(UIElement content)
        {
            _commandBarContent = content;

            if (ViewModel != null && ViewModel.IsCurrentNavigatedPage)
            {
                _mainScreenView.SetCommandBarContent(content);
            }

            ShowCommandBar();
        }

        private void ShowCommandBar()
        {
            //_isCommandBarHidden = false;

            //if (ViewModel != null && ViewModel.IsCurrentNavigatedPage)
            //{
            //    _mainScreenView.ShowCommandBar();
            //}
        }

        protected void HideCommandBar()
        {
            //_isCommandBarHidden = true;

            //if (ViewModel != null && ViewModel.IsCurrentNavigatedPage)
            //{
            //    _mainScreenView.HideCommandBar();
            //}
        }
    }
}
