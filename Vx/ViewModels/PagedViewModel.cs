using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareMvvm.Core.ViewModels
{
    public class PagedViewModel : BaseViewModel
    {
        public event EventHandler OnPresenterNeedsToGoBack;
        public event EventHandler<BaseViewModel> OnPresenterNeedsToRemoveModelFromBackStack;
        public event EventHandler<BaseViewModel> OnPresenterNeedsToNavigate;
        public event EventHandler<Tuple<BaseViewModel, BaseViewModel>> OnPresenterNeedsToReplaceCurrent;
        public event EventHandler<Tuple<BaseViewModel, BaseViewModel>> OnPresenterNeedsToReplaceWithinBackStack;
        public event EventHandler OnPresenterNeedsToClearAll;
        public event EventHandler OnPresenterNeedsToClearBackStack;

        /// <summary>
        /// This value will represent the value of the current page content. It cannot be set.
        /// </summary>
        public override bool AllowLightDismiss
        {
            get => base.AllowLightDismiss;
            set => throw new InvalidOperationException("AllowLightDismiss cannot be set on PagedViewModel. It inherits the value from the current page.");
        }

        public PagedViewModel(BaseViewModel parent) : base(parent)
        {
            (BackStack as INotifyCollectionChanged).CollectionChanged += PagedViewModel_CollectionChanged;
        }

        private void PagedViewModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (BackStack.Count > 0)
            {
                ThisBackButtonVisibility = RequestedBackButtonVisibility.Visible;
            }
            else
            {
                ThisBackButtonVisibility = RequestedBackButtonVisibility.Collapsed;
            }
        }

        public IList<BaseViewModel> BackStack { get; set; } = new ObservableCollection<BaseViewModel>();

        private BaseViewModel _content;
        public BaseViewModel Content
        {
            get { return _content; }
            private set
            {
#if DEBUG
                try
                {
#endif
                    var oldContent = _content;
                    SetProperty(ref _content, value, "Content");
                    base.AllowLightDismiss = value != null ? value.AllowLightDismiss : InitialAllowLightDismissValue;
                    TriggerChildContentChanged(value);
                    TriggerVisibleContentChanged();
                    if (oldContent != null)
                    {
                        oldContent.AllowLightDismissChanged -= CurrentContent_AllowLightDismissChanged;
                        oldContent.OnNavigatedFrom();
                        oldContent.OnViewLostFocus();
                    }
                    if (value != null)
                    {
                        value.AllowLightDismissChanged += CurrentContent_AllowLightDismissChanged;
                        value.OnNavigatedTo();
                        value.OnViewFocused();
                    }
#if DEBUG
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    System.Diagnostics.Debugger.Break();
                }
#endif
            }
        }

        private void CurrentContent_AllowLightDismissChanged(object sender, bool newValue)
        {
            if (sender == Content)
            {
                base.AllowLightDismiss = newValue;
            }
        }

        public void ClearContentAndBackStack()
        {
            if (BackStack.Count > 0)
            {
                BackStack.Clear();
                UpdateRequestedBackButtonVisibility();
            }

            Content = null;
            OnPresenterNeedsToClearAll?.Invoke(this, null);
        }

        public void ClearBackStack()
        {
            if (BackStack.Count > 0)
            {
                BackStack.Clear();
                UpdateRequestedBackButtonVisibility();
                OnPresenterNeedsToClearBackStack?.Invoke(this, null);
            }
        }

        protected override BaseViewModel GetChildContent()
        {
            return Content;
        }

        public override IEnumerable<BaseViewModel> GetChildren()
        {
            return new List<BaseViewModel>(BackStack)
            {
                Content
            };
        }

        public BaseViewModel GetMostRecentFromBackStack()
        {
            return BackStack.LastOrDefault();
        }

        public override bool GoBack()
        {
            if (BackStack.Count > 0)
            {
                var viewModel = BackStack.Last();
                BackStack.RemoveAt(BackStack.Count - 1);
                UpdateRequestedBackButtonVisibility();
                Content = viewModel;
                OnPresenterNeedsToGoBack?.Invoke(this, null);
                return true;
            }

            return base.GoBack();
        }

        public override bool RemoveViewModel(BaseViewModel model)
        {
            if (Content == model)
            {
                if (GoBack())
                {
                    return true;
                }
                else
                {
                    Content = null;
                    OnPresenterNeedsToClearAll?.Invoke(this, null);
                    return true;
                }
            }
            else
            {
                if (BackStack.Remove(model))
                {
                    OnPresenterNeedsToRemoveModelFromBackStack?.Invoke(this, model);
                    return true;
                }
            }

            return base.RemoveViewModel(model);
        }

        public void Navigate(BaseViewModel viewModel)
        {
            if (Content != null)
                BackStack.Add(Content);

            Content = viewModel;
            OnPresenterNeedsToNavigate?.Invoke(this, viewModel);
        }

        /// <summary>
        /// Replaces the current content with the provided view model
        /// </summary>
        /// <param name="viewModel"></param>
        public void Replace(BaseViewModel viewModel)
        {
            var old = Content;
            Content = viewModel;

            if (old != null)
                OnPresenterNeedsToReplaceCurrent?.Invoke(this, new Tuple<BaseViewModel, BaseViewModel>(old, viewModel));
            else
                OnPresenterNeedsToNavigate?.Invoke(this, viewModel);
        }

        /// <summary>
        /// Replaces the specified model with the provided model (or if specified model wasn't found, no-op).
        /// </summary>
        /// <param name="modelToReplace"></param>
        /// <param name="newModel"></param>
        public void Replace(BaseViewModel modelToReplace, BaseViewModel newModel)
        {
            if (Content == modelToReplace)
            {
                Replace(newModel);
            }

            else
            {
                int index = BackStack.IndexOf(modelToReplace);
                if (index == -1)
                {
                    // modelToReplace wasn't found in back stack or current content
                    return;
                }
                BackStack[index] = newModel;
                OnPresenterNeedsToReplaceWithinBackStack?.Invoke(this, new Tuple<BaseViewModel, BaseViewModel>(modelToReplace, newModel));
            }
        }

        private void UpdateRequestedBackButtonVisibility()
        {
            if (BackStack.Count > 0)
                ThisBackButtonVisibility = RequestedBackButtonVisibility.Visible;
            else
                ThisBackButtonVisibility = RequestedBackButtonVisibility.Inherit;
        }
    }
}
