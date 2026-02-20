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

namespace InterfacesiOS.ViewModelPresenters
{
    public class PagedViewModelWithPopupsPresenter : PagedViewModelPresenter
    {
        private ListOfViewModelsPresenter _listPresenter;
        private bool _destroyed = false;

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
            _listPresenter.ViewModels = ViewModel?.Popups;

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
        private void UpdateVisibility()
        {
            if (ViewModel == null || ViewModel.Popups.Count == 0 || _destroyed)
            {
                if (_isShown)
                {
                    _listPresenter.DismissViewController(true, null);
                    _isShown = false;
                }
            }
            else
            {
                if (!_isShown)
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
    }
}