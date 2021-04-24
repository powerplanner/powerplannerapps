using BareMvvm.Core.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System.ComponentModel;
using InterfacesUWP.ViewModelPresenters;
using Windows.UI.Core;
using ToolsPortable;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
using InterfacesUWP.Snackbar;
using BareMvvm.Core.Snackbar;

namespace InterfacesUWP.AppWindows
{
    public class NativeUwpAppWindow : Grid, INativeAppWindow
    {
        private GenericViewModelPresenter _presenter;
        private BareSnackbarPresenter _snackbarPresenter = new BareSnackbarPresenter();
        private SystemNavigationManagerEnhanced _navigationManager;

        public Window Window { get; private set; }

        public BareSnackbarManager SnackbarManager => _snackbarPresenter.Manager;

        public NativeUwpAppWindow()
        {
            _presenter = new GenericViewModelPresenter();
            Children.Add(_presenter);

            Children.Add(_snackbarPresenter);

            Window = Window.Current;

            Window.Content = this;

            // Back button
            _navigationManager = SystemNavigationManagerEnhanced.GetForCurrentView();
            _navigationManager.BackRequested += new WeakEventHandler<BackRequestedEventArgs>(_navigationManager_BackRequested).Handler;
        }

        private void _navigationManager_BackRequested(object sender, global::Windows.UI.Core.BackRequestedEventArgs e)
        {
            var cancelEventArgs = new CancelEventArgs();
            BackPressed?.Invoke(sender, cancelEventArgs);
            if (cancelEventArgs.Cancel)
            {
                e.Handled = true;
            }
        }

        public event EventHandler<CancelEventArgs> BackPressed;

        public void Register(PortableAppWindow portableWindow)
        {
            _presenter.SetBinding(GenericViewModelPresenter.ViewModelProperty, new Binding()
            {
                Path = new PropertyPath(nameof(portableWindow.ViewModel)),
                Source = portableWindow
            });

            portableWindow.PropertyChanged += PortableWindow_PropertyChanged;
        }

        private void PortableWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PortableAppWindow.BackButtonVisibility):
                    UpdateBackButtonVisibility(sender as PortableAppWindow);
                    break;
            }
        }

        private void UpdateBackButtonVisibility(PortableAppWindow window)
        {
            switch (window.BackButtonVisibility)
            {
                case BareMvvm.Core.ViewModels.RequestedBackButtonVisibility.Collapsed:
                case BareMvvm.Core.ViewModels.RequestedBackButtonVisibility.Inherit:
                    _navigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                    break;

                case BareMvvm.Core.ViewModels.RequestedBackButtonVisibility.Visible:
                    _navigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                    break;
            }
        }
    }
}
