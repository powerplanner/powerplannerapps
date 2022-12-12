using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using ToolsPortable;

namespace BareMvvm.Core.Windows
{
    public class PortableAppWindow : BindableBase
    {
        public event EventHandler<System.ComponentModel.CancelEventArgs> BackPressed;
        public PortableDispatcher Dispatcher { get; private set; } = PortableDispatcher.GetCurrentDispatcher();
        public INativeAppWindow NativeAppWindow { get; private set; }

        private RequestedBackButtonVisibility _backButtonVisibility = RequestedBackButtonVisibility.Collapsed;
        public RequestedBackButtonVisibility BackButtonVisibility
        {
            get { return _backButtonVisibility; }
            set { SetProperty(ref _backButtonVisibility, value, nameof(BackButtonVisibility)); }
        }

        /// <summary>
        /// Constructor must be called on Window's UI thread, so that the correct dispatcher can be linked up
        /// </summary>
        public PortableAppWindow()
        {
        }

        internal void Register(INativeAppWindow nativeWindow)
        {
            NativeAppWindow = nativeWindow;
            NativeAppWindow.Register(this);
            NativeAppWindow.BackPressed += NativeAppWindow_BackPressed;
        }

        internal void Unregister()
        {
            NativeAppWindow.BackPressed -= NativeAppWindow_BackPressed;
            NativeAppWindow = null;
        }

        private async void NativeAppWindow_BackPressed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BackPressed?.Invoke(this, e);

            if (e.Cancel)
            {
                return;
            }

            var currentContent = ViewModel?.GetFinalContent();
            if (currentContent != null)
            {
                // We assume going back succeeded
                bool wentBack = true;

                var task = ViewModel.HandleUserInteractionAsync("GoBack", delegate
                {
                    // Sometimes this might happen asynchronously if there was another pending UI action,
                    // but in those cases we would want to cancel the back press anyways. And for the case
                    // of exiting the app, as long as no other pending UI action was occurring, this would
                    // happen synchronously, and return false, and not cancel the back press
                    wentBack = currentContent.GoBack();
                });

                e.Cancel = wentBack;

                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    ExceptionHelper.ReportHandledException(ex);
                }
            }
        }

        private BaseViewModel _viewModel;
        public BaseViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                if (_viewModel != null)
                {
                    _viewModel.SetOwningWindow(null);
                    _viewModel.PropertyChanged -= _viewModel_PropertyChanged;
                }

                if (value != null)
                {
                    value.SetOwningWindow(this);
                    value.PropertyChanged += _viewModel_PropertyChanged;
                }

                SetProperty(ref _viewModel, value, "ViewModel");
            }
        }

        private bool _canGoBack;
        public bool CanGoBack
        {
            get => _canGoBack;
            set => SetProperty(ref _canGoBack, value, nameof(CanGoBack));
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(ViewModel.FinalBackButtonVisibility):
                    BackButtonVisibility = ViewModel.FinalBackButtonVisibility;
                    break;

                case nameof(ViewModel.FinalContent):
                    var final = ViewModel?.FinalContent;
                    CanGoBack = final != null ? final.CanGoBack : false;
                    break;
            }
        }
    }
}
