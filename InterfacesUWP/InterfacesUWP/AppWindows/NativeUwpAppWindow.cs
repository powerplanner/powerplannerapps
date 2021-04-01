using BareMvvm.Core.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using InterfacesUWP.ViewModelPresenters;
using Windows.UI.Core;
using ToolsPortable;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
using InterfacesUWP.Snackbar;
using BareMvvm.Core.Snackbar;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

namespace InterfacesUWP.AppWindows
{
    public class MainPageAlt : WindowsPage
    {
        public MainPageAlt()
        {
            LoadApplication(Activator.CreateInstance(BareMvvm.Forms.App.FormsApp.Current.GetAppShellType()) as Xamarin.Forms.Application);
        }
    }

    public class NativeUwpAppWindow : ContentPage, INativeAppWindow
    {
        private BareMvvm.Forms.ViewModelPresenters.GenericViewModelPresenter _presenter;
        private BareSnackbarPresenter _snackbarPresenter = new BareSnackbarPresenter();
        private SystemNavigationManagerEnhanced _navigationManager;

        public Window Window { get; private set; }

        public BareSnackbarManager SnackbarManager => _snackbarPresenter.Manager;

        public NativeUwpAppWindow()
        {
            //_presenter = new BareMvvm.Forms.ViewModelPresenters.GenericViewModelPresenter();
            //Content = _presenter;
            //Children.Add(_presenter);

            //Children.Add(_snackbarPresenter);

            Window = Window.Current;

            var rootFrame = new Windows.UI.Xaml.Controls.Frame();
            rootFrame.Navigate(typeof(MainPageAlt));

            Window.Content = rootFrame;

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
            //_presenter.SetBinding(BareMvvm.Forms.ViewModelPresenters.GenericViewModelPresenter.ViewModelProperty, new Xamarin.Forms.Binding()
            //{
            //    Path = nameof(portableWindow.ViewModel),
            //    Source = portableWindow
            //});

            //portableWindow.PropertyChanged += PortableWindow_PropertyChanged;
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
