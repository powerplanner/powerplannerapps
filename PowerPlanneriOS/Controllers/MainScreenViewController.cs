using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using ToolsPortable;
using System.ComponentModel;
using InterfacesiOS.ViewModelPresenters;
using InterfacesiOS.Views;
using System.Collections.Specialized;
using PowerPlanneriOS.Helpers;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using PowerPlannerAppDataLibrary;
using Vx.iOS;
using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.Components;
using PowerPlannerAppDataLibrary.ViewModels;
using System.Collections.ObjectModel;

namespace PowerPlanneriOS.Controllers
{
    public class MainScreenViewController : PagedViewModelWithPopupsPresenter
    {
        private readonly PopupViewHostComponent _popupHost = new PopupViewHostComponent();
        private iOSNativeComponent _nativePopupHost;
        private UIView _popupBackdrop;
        private PopupComponentViewModel _displayedPopup;

        public static nfloat TAB_BAR_HEIGHT = 0;

        /// <summary>
        /// Must provide a strong reference storage point in your child view controller so that the action will be persisted correctly
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static void ListenToTabBarHeightChanged(ref object strongReferenceStorage, Action action)
        {
            action.Invoke();
        }

        ~MainScreenViewController()
        {
            System.Diagnostics.Debug.WriteLine("Disposed main screen view controller");
        }

        public new MainScreenViewModel ViewModel
        {
            get { return base.ViewModel as MainScreenViewModel; }
            set
            {
                if (base.ViewModel != null)
                {
                    throw new InvalidOperationException("This view controller can't be recycled, a new one must be created when assigning a new view model");
                }

                if (value != null)
                {
                    base.ViewModel = value;

                    var renderedView = ViewModel.Render();
                    renderedView.TranslatesAutoresizingMaskIntoConstraints = false;
                    base.Add(renderedView);
                    renderedView.StretchWidthAndHeight(base.View);

                    ActivatePendingLaunchAction();
                    TryAskingForRatingIfNeeded();
                }
            }
        }

        private Func<PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainWindowViewModel, System.Threading.Tasks.Task> _pendingLaunchAction;
        private bool _hasRunPendingLaunchAction;

        private void ActivatePendingLaunchAction()
        {
            AppDelegate._hasActivatedWindow = true;

            // Capture the pending action, but don't run it yet. Running it here (during the
            // ViewModel setter) would try to present a popup before this view controller is in
            // the window hierarchy, which iOS silently drops. We defer it to ViewDidAppear.
            _pendingLaunchAction = AppDelegate._handleLaunchAction;
            AppDelegate._handleLaunchAction = null;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            UpdateSnackbarBottomOffset();
            RunPendingLaunchActionIfNeeded();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            UpdateInPlacePopupPresentation();
        }

        protected override bool DisplaysPopupsInPlace => VxDeviceType.Current == Vx.DeviceType.Desktop
            && ViewModel?.Popups.All(i => i is PopupComponentViewModel) != false;

        protected override void UpdateInPlacePopupPresentation()
        {
            if (!IsViewLoaded)
            {
                return;
            }

            var popup = ViewModel?.Popups.LastOrDefault() as PopupComponentViewModel;
            if (popup == null)
            {
                RemoveInPlacePopup();
                return;
            }

            EnsureInPlacePopup();

            if (_displayedPopup != popup)
            {
                if (_displayedPopup != null)
                {
                    _displayedPopup.PropertyChanged -= Popup_PropertyChanged;
                }

                _displayedPopup = popup;
                _displayedPopup.PropertyChanged += Popup_PropertyChanged;
                _popupHost.NativeContent = popup.Render();
                _popupHost.OnClose = popup.TryRemoveViewModelViaUserInteraction;
            }

            UpdatePopupHost();
        }

        private void EnsureInPlacePopup()
        {
            if (_popupBackdrop == null)
            {
                _popupBackdrop = new UIView
                {
                    BackgroundColor = UIColor.Black,
                    Alpha = 0.3f,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                _popupBackdrop.AddGestureRecognizer(new UITapGestureRecognizer(() => _ = ViewModel.TryDismissCurrentPopupViaUserInteractionAsync()));
            }

            if (_popupBackdrop.Superview == null)
            {
                base.View.Add(_popupBackdrop);
                _popupBackdrop.StretchWidthAndHeight(base.View);
            }

            if (_nativePopupHost == null)
            {
                _nativePopupHost = _popupHost.Render();
                _nativePopupHost.TranslatesAutoresizingMaskIntoConstraints = false;
            }

            if (_nativePopupHost.Superview == null)
            {
                base.View.Add(_nativePopupHost);
                _nativePopupHost.CenterXAnchor.ConstraintEqualTo(base.View.CenterXAnchor).Active = true;
                _nativePopupHost.CenterYAnchor.ConstraintEqualTo(base.View.CenterYAnchor).Active = true;
                _nativePopupHost.WidthAnchor.ConstraintLessThanOrEqualTo(550).Active = true;
                _nativePopupHost.HeightAnchor.ConstraintLessThanOrEqualTo(700).Active = true;
                _nativePopupHost.WidthAnchor.ConstraintLessThanOrEqualTo(base.View.WidthAnchor).Active = true;
                _nativePopupHost.HeightAnchor.ConstraintLessThanOrEqualTo(base.View.HeightAnchor).Active = true;
            }
        }

        private void RemoveInPlacePopup()
        {
            _popupBackdrop?.RemoveFromSuperview();
            _nativePopupHost?.RemoveFromSuperview();

            if (_displayedPopup != null)
            {
                _displayedPopup.PropertyChanged -= Popup_PropertyChanged;
                _displayedPopup = null;
            }
        }

        private void Popup_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender == _displayedPopup)
            {
                UpdatePopupHost();
            }
        }

        private void UpdatePopupHost()
        {
            _popupHost.Title = _displayedPopup.Title;
            _popupHost.PrimaryCommands = new ObservableCollection<PopupCommand>(_displayedPopup.Commands ?? Array.Empty<PopupCommand>());
            _popupHost.SecondaryCommands = new ObservableCollection<PopupCommand>(_displayedPopup.SecondaryCommands ?? Array.Empty<PopupCommand>());
            _popupHost.RenderOnDemand();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            UpdateSnackbarBottomOffset();
        }

        public override void ViewWillDisappear(bool animated)
        {
            BareSnackbarPresenter.BottomOffset = 0;
            base.ViewWillDisappear(animated);
        }

        private void UpdateSnackbarBottomOffset()
        {
            BareSnackbarPresenter.BottomOffset = ViewModel?.IsCompactMode == true
                ? (nfloat)(64 + Math.Max(0, (double)View.SafeAreaInsets.Bottom - 15))
                : 0;
        }

        private async void RunPendingLaunchActionIfNeeded()
        {
            if (_hasRunPendingLaunchAction)
            {
                return;
            }

            var action = _pendingLaunchAction;
            _pendingLaunchAction = null;
            if (action == null)
            {
                return;
            }

            _hasRunPendingLaunchAction = true;

            try
            {
                var mainWindowViewModel = (ViewModel.GetAppWindow() as PowerPlannerAppDataLibrary.Windows.MainAppWindow)?.GetViewModel();
                if (mainWindowViewModel != null)
                {
                    await action(mainWindowViewModel);
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        protected override void SetHostedViewModel(BaseViewModel viewModel)
        {
            ViewModel = (MainScreenViewModel)viewModel;
        }

        private async void TryAskingForRatingIfNeeded()
        {
            try
            {
                // If we haven't asked for rating yet
                if (!PowerPlannerAppDataLibrary.Helpers.Settings.HasAskedForRating)
                {
                    if (ViewModel.CurrentAccount != null)
                    {
                        var dataStore = await AccountDataStore.Get(ViewModel.CurrentLocalAccountId);

                        // If they actually have a decent amount of tasks
                        if (await System.Threading.Tasks.Task.Run(async delegate
                        {
                            using (await Locks.LockDataForReadAsync())
                            {
                                return dataStore.HasManyOldMegaItems();
                            }
                        }))
                        {
                            var alert = UIAlertController.Create(
                                title: "★ Review App ★",
                                message: "Thanks for using Power Planner! If you love the app, please leave a rating in the Store! If you have any suggestions or issues, please email me!",
                                preferredStyle: UIAlertControllerStyle.Alert);

                            alert.AddAction(UIAlertAction.Create("Review", UIAlertActionStyle.Default, delegate
                            {
                                PowerPlannerAppDataLibrary.Helpers.Settings.HasAskedForRating = true;
                                PowerPlannerAppDataLibrary.Helpers.Settings.HasReviewedOrEmailedDev = true;
                                TelemetryExtension.Current?.TrackEvent("PromptReviewApp_ClickedReview");

                                OpenStoreReview();
                            }));

                            alert.AddAction(UIAlertAction.Create("Email dev", UIAlertActionStyle.Default, delegate
                            {
                                PowerPlannerAppDataLibrary.Helpers.Settings.HasAskedForRating = true;
                                PowerPlannerAppDataLibrary.Helpers.Settings.HasReviewedOrEmailedDev = true;
                                TelemetryExtension.Current?.TrackEvent("PromptReviewApp_ClickedEmailDev");

                                AboutViewModel.EmailDeveloper();
                            }));

                            alert.AddAction(UIAlertAction.Create("No thanks", UIAlertActionStyle.Cancel, delegate
                            {
                                PowerPlannerAppDataLibrary.Helpers.Settings.HasAskedForRating = true;
                                TelemetryExtension.Current?.TrackEvent("PromptReviewApp_ClickedNoThanks");
                            }));

                            PresentViewController(alert, true, null);
                        }
                    }
                }

                // If the user previously clicked No thanks, we'll try the new in-app review dialog
                else if (!PowerPlannerAppDataLibrary.Helpers.Settings.HasReviewedOrEmailedDev)
                {
#if !DEBUG
                    // This will only sometimes show a dialog, at most 3 times a year
                    // It will still display if they already rated, meaning users who previously clicked
                    // No thanks on my own dialog will persistently get this dialog, but that should be ok
                    var windowScene = View?.Window?.WindowScene;
                    if (windowScene != null)
                    {
                        if (OperatingSystem.IsIOSVersionAtLeast(18))
                        {
                            StoreKit.AppStore.RequestReview(windowScene);
                        }
                        else
                        {
#pragma warning disable CA1422 // RequestReview(UIWindowScene) is obsoleted on iOS 18.0
                            StoreKit.SKStoreReviewController.RequestReview(windowScene);
#pragma warning restore CA1422
                        }
                    }
#endif
                }
            }

            catch { }
        }

        private static async void OpenStoreReview()
        {
            try
            {
                await ReviewAppExtension.Current?.ReviewAppAsync();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}