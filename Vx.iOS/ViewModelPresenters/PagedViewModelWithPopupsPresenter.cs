using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BareMvvm.Core.ViewModels;
using Foundation;
using UIKit;
using System.Collections.Specialized;
using ToolsPortable;
using System.Threading.Tasks;
using System.ComponentModel;
using Vx;
using Vx.iOS.Views;

namespace InterfacesiOS.ViewModelPresenters
{
    public class PagedViewModelWithPopupsPresenter : PagedViewModelPresenter
    {
        private ListOfViewModelsPresenter _listPresenter;
        private bool _destroyed = false;

        // Used for in-place popup presentation (Mac Catalyst / Desktop), where popups are shown
        // embedded within this view (behind a dimming backdrop) instead of presented modally.
        private UIView _inPlaceBackdrop;
        private UIView _inPlacePopupView;
        private BaseViewModel _inPlacePopupViewModel;

        public new PagedViewModelWithPopups ViewModel
        {
            get { return base.ViewModel as PagedViewModelWithPopups; }
            set { base.ViewModel = value; }
        }

        public PagedViewModelWithPopupsPresenter()
        {
            _listPresenter = new ListOfViewModelsPresenter();
            _listPresenter.OnRemoved += _listPresenter_OnRemoved;
        }

        private void _listPresenter_OnRemoved(object sender, EventArgs e)
        {
            // This handles cases where user removed via gesture, so we have to update the model
            // Note that this is also fired when user removes by clicking
            _isShown = false;

            if (ViewModel.Popups.Count > 0)
            {
                ViewModel.Popups.Clear();
            }
        }

        private NotifyCollectionChangedEventHandler _popupsCollectionChangedHandler;
        private PropertyChangedEventHandler _propertyChangedEventHandler;
        protected override void OnViewModelChanged(PagedViewModel oldViewModel, PagedViewModel currentViewModel)
        {
            _listPresenter.ViewModels = DisplaysPopupsInPlace ? null : ViewModel?.Popups;

            Deregister(oldViewModel);

            if (_popupsCollectionChangedHandler == null)
            {
                _popupsCollectionChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(Popups_CollectionChanged).Handler;
            }

            if (_propertyChangedEventHandler == null)
            {
                _propertyChangedEventHandler = new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
            }

            PagedViewModelWithPopups newModel = currentViewModel as PagedViewModelWithPopups;
            if (newModel != null)
            {
                newModel.PropertyChanged += _propertyChangedEventHandler;
                newModel.Popups.CollectionChanged += _popupsCollectionChangedHandler;
                newModel.CurrentPopupAllowsLightDismissChanged += CurrentPopupAllowsLightDismissChanged;
            }

            UpdateVisibility();
            UpdateFullScreenPopup();
            UpdateLightDismiss();

            base.OnViewModelChanged(oldViewModel, currentViewModel);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.FullScreenPopup))
            {
                UpdateFullScreenPopup();
            }
        }

        private UIViewController _prevFullScreenController;
        private BaseViewModel _prevFullScreenViewModel;
        private void UpdateFullScreenPopup()
        {
            // If there shouldn't be any full screen content
            if (ViewModel == null || ViewModel.FullScreenPopup == null)
            {
                // If there was full screen content
                if (_prevFullScreenController != null)
                {
                    // Dismiss it and update current
                    _prevFullScreenController.DismissViewController(true, null);
                    _prevFullScreenController = null;
                    _prevFullScreenViewModel = null;
                }

                return;
            }

            // Otherwise, if the full screen content is the same
            if (ViewModel.FullScreenPopup == _prevFullScreenViewModel)
            {
                // Do nothing
                return;
            }

            // Otherwise, the full screen content must be initialized and is different
            if (_prevFullScreenController != null)
            {
                _prevFullScreenController.DismissViewController(false, null);
            }

            var newController = ViewModelToViewConverter.Convert(ViewModel.FullScreenPopup);
            newController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            ShowDetailViewController(newController, null);
            _prevFullScreenController = newController;
            _prevFullScreenViewModel = ViewModel.FullScreenPopup;
        }

        private void CurrentPopupAllowsLightDismissChanged(object sender, bool newValue)
        {
            UpdateLightDismiss();
        }

        private void Deregister(BaseViewModel oldViewModel)
        {
            PagedViewModelWithPopups old = oldViewModel as PagedViewModelWithPopups;

            if (old != null)
            {
                old.PropertyChanged -= _propertyChangedEventHandler;
                old.Popups.CollectionChanged -= _popupsCollectionChangedHandler;
                old.CurrentPopupAllowsLightDismissChanged -= CurrentPopupAllowsLightDismissChanged;
            }
        }

        private bool _isShown;
        private bool _isDismissing;
        private void UpdateVisibility()
        {
            if (DisplaysPopupsInPlace)
            {
                if (_listPresenter.ViewModels != null)
                {
                    _listPresenter.ViewModels = null;
                }

                UpdateInPlacePopupPresentation();
                return;
            }

            if (_listPresenter.ViewModels != ViewModel?.Popups)
            {
                _listPresenter.ViewModels = ViewModel?.Popups;
            }

            if (ViewModel == null || ViewModel.Popups.Count == 0 || _destroyed)
            {
                if (_isShown && !_isDismissing)
                {
                    _isDismissing = true;
                    _listPresenter.DismissViewController(true, delegate
                    {
                        _isShown = false;
                        _isDismissing = false;
                        UpdateVisibility();
                    });
                }
            }
            else
            {
                if (!_isShown && !_isDismissing)
                {
                    ShowDetailViewController(_listPresenter, null);
                    _isShown = true;
                }
            }
        }

        private void UpdateLightDismiss()
        {
            if (ViewModel == null || _destroyed)
            {
                return;
            }
            else
            {
                _listPresenter.ModalInPresentation = !ViewModel.CurrentPopupAllowsLightDismiss;
            }
        }

        private void Popups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ViewModel?.Popups != sender)
            {
                return;
            }

            UpdateVisibility();
        }

        internal override void Destroy()
        {
            // For handling the case where the parent view gets swapped out somewhere underneath us
            Deregister(ViewModel);
            _destroyed = true;
            UpdateVisibility();
            _listPresenter.ViewModels = null;

            base.Destroy();
        }

        protected virtual bool DisplaysPopupsInPlace => VxDeviceType.Current == DeviceType.Desktop;

        protected virtual void UpdateInPlacePopupPresentation()
        {
            // Only the top-most popup is ever shown (matching the other platforms' behavior of
            // stacking popups and only displaying the last one), centered behind a dimming backdrop.
            var targetViewModel = ViewModel != null && !_destroyed && ViewModel.Popups.Count > 0
                ? ViewModel.Popups[ViewModel.Popups.Count - 1]
                : null;

            if (targetViewModel == _inPlacePopupViewModel)
            {
                return;
            }

            _inPlacePopupViewModel = targetViewModel;

            // Remove any existing popup content
            if (_inPlacePopupView != null)
            {
                _inPlacePopupView.RemoveFromSuperview();
                _inPlacePopupView = null;
            }

            if (targetViewModel == null)
            {
                if (_inPlaceBackdrop != null)
                {
                    _inPlaceBackdrop.RemoveFromSuperview();
                    _inPlaceBackdrop = null;
                }

                return;
            }

            if (_inPlaceBackdrop == null)
            {
                _inPlaceBackdrop = new UIView
                {
                    BackgroundColor = UIColor.Black.ColorWithAlpha(0.3f),
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                _inPlaceBackdrop.AddGestureRecognizer(new UITapGestureRecognizer(OnInPlaceBackdropTapped));

                View.AddSubview(_inPlaceBackdrop);
                _inPlaceBackdrop.StretchWidthAndHeight(View);
            }
            else
            {
                View.BringSubviewToFront(_inPlaceBackdrop);
            }

            var popupView = CreateInPlacePopupView(targetViewModel);
            popupView.TranslatesAutoresizingMaskIntoConstraints = false;
            popupView.Layer.CornerRadius = 8;
            popupView.ClipsToBounds = true;
            View.AddSubview(popupView);

            const float MaxWidth = 460f;
            const float HorizontalPadding = 24f;
            const float VerticalPadding = 24f;

            // Center the popup, with a max width of 460px but shrinking to keep at least 24px
            // padding on each side. The popup hugs its content's natural height, but is capped to
            // the available window height minus 24px padding on top and bottom (beyond which its
            // content scrolls internally).
            var widthConstraint = popupView.WidthAnchor.ConstraintEqualTo(MaxWidth);
            widthConstraint.Priority = (float)UILayoutPriority.DefaultHigh;

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                popupView.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                popupView.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
                popupView.WidthAnchor.ConstraintLessThanOrEqualTo(MaxWidth),
                popupView.LeadingAnchor.ConstraintGreaterThanOrEqualTo(View.LeadingAnchor, HorizontalPadding),
                popupView.TrailingAnchor.ConstraintLessThanOrEqualTo(View.TrailingAnchor, -HorizontalPadding),
                popupView.TopAnchor.ConstraintGreaterThanOrEqualTo(View.SafeAreaLayoutGuide.TopAnchor, VerticalPadding),
                popupView.BottomAnchor.ConstraintLessThanOrEqualTo(View.SafeAreaLayoutGuide.BottomAnchor, -VerticalPadding),
                widthConstraint
            });

            _inPlacePopupView = popupView;
        }

        /// <summary>
        /// Creates the native view used to display an in-place popup. The default implementation
        /// renders the view model directly as a component (without any popup chrome); app-level
        /// subclasses can override this to wrap the content with a themed popup host.
        /// </summary>
        protected virtual UIView CreateInPlacePopupView(BaseViewModel viewModel)
        {
            return Vx.iOS.VxiOSExtensions.Render(viewModel);
        }

        private void OnInPlaceBackdropTapped()
        {
            _ = ViewModel?.TryDismissCurrentPopupViaUserInteractionAsync();
        }
    }
}