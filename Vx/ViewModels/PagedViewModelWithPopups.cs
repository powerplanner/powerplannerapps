using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace BareMvvm.Core.ViewModels
{
    public abstract class PagedViewModelWithPopups : PagedViewModel
    {
        public PagedViewModelWithPopups(BaseViewModel parent) : base(parent)
        {
            Popups.CollectionChanged += Popups_CollectionChanged;
        }

        public MyObservableList<BaseViewModel> Popups { get; private set; } = new MyObservableList<BaseViewModel>();

        private BaseViewModel _fullScreenPopup;
        public BaseViewModel FullScreenPopup
        {
            get => _fullScreenPopup;
            set
            {
                if (_fullScreenPopup != value)
                {
                    _fullScreenPopup = value;
                    UpdateRequestedBackButtonVisibility();
                    OnPopupsOrFullScreenPopupChanged();
                    OnPropertyChanged(nameof(FullScreenPopup));
                }
            }
        }

        public override void ShowPopup(BaseViewModel viewModel)
        {
            viewModel.ResumeRendering();
            Popups.Add(viewModel);
        }

        public void ShowFullScreenPopup(BaseViewModel viewModel)
        {
            viewModel.ResumeRendering();
            FullScreenPopup = viewModel;
        }

        protected override RequestedBackButtonVisibility CalculateThisBackButtonVisibility()
        {
            if (FullScreenPopup != null)
            {
                return RequestedBackButtonVisibility.Visible;
            }

            return base.CalculateThisBackButtonVisibility();
        }

        protected override BaseViewModel GetChildContent()
        {
            if (FullScreenPopup != null)
            {
                return FullScreenPopup;
            }

            if (Popups.Count > 0)
                return Popups.Last();

            return base.GetChildContent();
        }

        public override IEnumerable<BaseViewModel> GetChildren()
        {
            var list = new List<BaseViewModel>(base.GetChildren());
            list.AddRange(Popups);
            if (FullScreenPopup != null)
            {
                list.Add(FullScreenPopup);
            }
            return list;
        }

        public event EventHandler<bool> CurrentPopupAllowsLightDismissChanged;

        private bool _currentPopupAllowsLightDismiss = true;
        private bool _subscribedToCurrentPopupAllowsLightDismiss = false;
        /// <summary>
        /// Will send out change events
        /// </summary>
        public bool CurrentPopupAllowsLightDismiss
        {
            get
            {
                if (!_subscribedToCurrentPopupAllowsLightDismiss)
                {
                    _subscribedToCurrentPopupAllowsLightDismiss = true;

                    if (Popups.Count > 0)
                    {
                        HandlePopupAllowsLightDismissCurrentPopupChange(null, Popups.Last());
                        _currentPopupAllowsLightDismiss = Popups.Last().AllowLightDismiss;
                    }
                    else
                    {
                        _currentPopupAllowsLightDismiss = true;
                    }
                }

                return _currentPopupAllowsLightDismiss;
            }
            private set
            {
                if (value != _currentPopupAllowsLightDismiss)
                {
                    SetProperty(ref _currentPopupAllowsLightDismiss, value, nameof(CurrentPopupAllowsLightDismiss));
                    CurrentPopupAllowsLightDismissChanged?.Invoke(this, value);
                }
            }
        }

        private void Popups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPopupsOrFullScreenPopupChanged();
        }

        private BaseViewModel _prevLastPopup;
        private void OnPopupsOrFullScreenPopupChanged()
        {
            if (FullScreenPopup != null)
            {
                if (_prevLastPopup == FullScreenPopup)
                {
                    // One of the popups must have changed under the scenes, nothing to do
                    return;
                }
                else
                {
                    // We have a new fullscreen popup
                    var prevLastPopup = _prevLastPopup;
                    _prevLastPopup = FullScreenPopup;
                    var currPopup = FullScreenPopup;

                    // If we had a popup or fullscreen popup previously
                    if (prevLastPopup != null)
                    {
                        prevLastPopup.OnViewLostFocus();
                    }
                    else if (Content != null)
                    {
                        Content.OnViewLostFocus();
                    }

                    currPopup.OnViewFocused();
                    TriggerVisibleContentChanged();
                }
            }
            else if (Popups.Any())
            {
                if (_prevLastPopup == Popups.LastOrDefault())
                {
                    // One of the popups must have changed under the scenes, nothing to do
                    return;
                }
                else
                {
                    var prevLastPopup = _prevLastPopup;
                    _prevLastPopup = Popups.LastOrDefault();
                    var currPopup = _prevLastPopup;

                    // If we had a popup or fullscreen popup previously
                    if (prevLastPopup != null)
                    {
                        prevLastPopup.OnViewLostFocus();
                    }
                    else if (Content != null)
                    {
                        Content.OnViewLostFocus();
                    }

                    currPopup.OnViewFocused();

                    if (_subscribedToCurrentPopupAllowsLightDismiss)
                    {
                        HandlePopupAllowsLightDismissCurrentPopupChange(prevLastPopup, currPopup);
                        CurrentPopupAllowsLightDismiss = currPopup.AllowLightDismiss;
                    }

                    TriggerVisibleContentChanged();

                }
            }
            else
            {
                if (_prevLastPopup != null)
                {
                    var prevLastPopup = _prevLastPopup;
                    _prevLastPopup = null;
                    prevLastPopup.OnViewLostFocus();

                    if (Content != null)
                    {
                        Content.OnViewFocused();
                    }

                    TriggerVisibleContentChanged();
                }
            }
        }

        private void HandlePopupAllowsLightDismissCurrentPopupChange(BaseViewModel oldPopup, BaseViewModel currPopup)
        {
            if (oldPopup != null)
            {
                oldPopup.AllowLightDismissChanged -= CurrPopup_AllowLightDismissChanged;
            }

            if (currPopup != null)
            {
                currPopup.AllowLightDismissChanged += CurrPopup_AllowLightDismissChanged;
            }
        }

        private void CurrPopup_AllowLightDismissChanged(object sender, bool newValue)
        {
            CurrentPopupAllowsLightDismiss = newValue;
        }

        public override bool GoBack()
        {
            if (FullScreenPopup != null)
            {
                FullScreenPopup = null;
                return true;
            }

            if (Popups.Count > 0)
            {
                Popups.RemoveAt(Popups.Count - 1);
                return true;
            }

            return base.GoBack();
        }

        public override bool RemoveViewModel(BaseViewModel model)
        {
            if (FullScreenPopup == model)
            {
                FullScreenPopup = null;
                return true;
            }

            if (Popups.Remove(model))
            {
                return true;
            }

            return base.RemoveViewModel(model);
        }

        /// <summary>
        /// Tries to clear all popups under the user interaction lock. Won't throw exception.
        /// </summary>
        public async void TryClearPopupsViaUserInteraction()
        {
            try
            {
                await HandleUserInteractionAsync("ClearPopups", delegate
                {
                    FullScreenPopup = null;
                    Popups.Clear();
                });
            }
            catch (Exception ex)
            {
                ExceptionHelper.ReportHandledException(ex);
            }
        }

        public override bool CanGoBack
        {
            get
            {
                if (FullScreenPopup != null)
                {
                    return true;
                }

                if (Popups.Count > 0)
                {
                    return true;
                }

                return base.CanGoBack;
            }
        }
    }
}
