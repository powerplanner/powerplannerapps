using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BareMvvm.Core.Windows;
using InterfacesDroid.ViewModelPresenters;
using InterfacesDroid.Activities;
using System.ComponentModel;
using BareMvvm.Core.Snackbar;
using Google.Android.Material.Snackbar;
using AndroidX.CoordinatorLayout.Widget;

namespace InterfacesDroid.Windows
{
    public class NativeDroidAppWindow : INativeAppWindow
    {
        private GenericViewModelPresenter _presenter;
        private CoordinatorLayout _coordinatorLayout;

        public event EventHandler<CancelEventArgs> BackPressed;

        public BareActivity Activity { get; private set; }

        public BareSnackbarManager SnackbarManager { get; private set; } = new BareSnackbarManager();

        public NativeDroidAppWindow(BareActivity activity)
        {
            Activity = activity;
            _presenter = new GenericViewModelPresenter(activity)
            {
                LayoutParameters = new CoordinatorLayout.LayoutParams(CoordinatorLayout.LayoutParams.MatchParent, CoordinatorLayout.LayoutParams.MatchParent)
            };

            // CoordinatorLayout is needed so snackbars can be swipeable
            _coordinatorLayout = new CoordinatorLayout(activity);
            _coordinatorLayout.AddView(_presenter);
            activity.SetContentView(_coordinatorLayout);

            activity.BackPressed += Activity_BackPressed;

            SnackbarManager.OnShow += SnackbarManager_OnShow;
            SnackbarManager.OnClose += SnackbarManager_OnClose;
        }

        private void SnackbarManager_OnShow(object sender, BareSnackbar snackbar)
        {
            try
            {
                var nativeSnackbar = Snackbar.Make(_coordinatorLayout, snackbar.Message, snackbar.Duration);

                var anchorView = _presenter.GetSnackbarAnchorView();
                if (anchorView != null)
                {
                    nativeSnackbar.SetAnchorView(anchorView);
                }

                if (snackbar.ButtonText != null)
                {
                    nativeSnackbar.SetAction(snackbar.ButtonText, delegate
                    {
                        try
                        {
                            snackbar.ButtonCallback();
                        }
                        catch { }
                    });
                }

                snackbar.NativeSnackbar = nativeSnackbar;
                nativeSnackbar.Show();
            }
            catch { }
        }

        private void SnackbarManager_OnClose(object sender, BareSnackbar snackbar)
        {
            try
            {
                (snackbar.NativeSnackbar as Snackbar).Dismiss();
            }
            catch { }
        }

        private void Activity_BackPressed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BackPressed?.Invoke(this, e);
        }

        public void Register(PortableAppWindow portableWindow)
        {
            portableWindow.PropertyChanged += PortableWindow_PropertyChanged;
            if (portableWindow.ViewModel != null)
                _presenter.ViewModel = portableWindow.ViewModel;
        }

        private void PortableWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ViewModel":
                    _presenter.ViewModel = (sender as PortableAppWindow).ViewModel;
                    break;
            }
        }
    }
}