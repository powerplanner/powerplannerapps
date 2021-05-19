using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using BareMvvm.Core.ViewModels;
using ToolsPortable;

namespace InterfacesiOS.ViewModelPresenters
{
    public class PagedViewModelPresenter : UIViewController
    {
        protected UINavigationController MyNavigationController { get; private set; }
        public PagedViewModelPresenter()
        {
            MyNavigationController = new UINavigationController()
            {
                NavigationBarHidden = true
            };

            // We add the View of the navigation controller, since ViewControllers are simply view *WRAPPERS*,
            // they contain a View object which they manipulate, they aren't views themselves.
            // AddChildViewContainer wires up events so they propogate downwards correctly
            base.AddChildViewController(MyNavigationController);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Add simply adds the view to the display
            base.Add(MyNavigationController.View);
        }

        // https://developer.apple.com/documentation/uikit/uinavigationcontroller

        private EventHandler _viewModelOnPresenterNeedsToGoBackHandler;
        private EventHandler<BaseViewModel> _viewModelOnPresenterNeedsToRemoveModelFromBackStack;
        private EventHandler<BaseViewModel> _viewModelOnPresenterNeedsToNavigate;
        private EventHandler<Tuple<BaseViewModel, BaseViewModel>> _viewModelOnPresenterNeedsToReplaceCurrent;
        private EventHandler<Tuple<BaseViewModel, BaseViewModel>> _viewModelOnPresenterNeedsToReplaceWithinBackStack;
        private EventHandler _viewModelOnPresenterNeedsToClearAll;
        private EventHandler _viewModelOnPresenterNeedsToClearBackStack;
        private List<Tuple<BaseViewModel, UIViewController>> _liveViews = new List<Tuple<BaseViewModel, UIViewController>>();
        private PagedViewModel _viewModel;
        public PagedViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                try
                {
                    if (_viewModel == value)
                    {
                        return;
                    }

                    _liveViews.Clear();

                    if (_viewModel != null)
                    {
                        _viewModel.OnPresenterNeedsToGoBack -= _viewModelOnPresenterNeedsToGoBackHandler;
                        _viewModel.OnPresenterNeedsToRemoveModelFromBackStack -= _viewModelOnPresenterNeedsToRemoveModelFromBackStack;
                        _viewModel.OnPresenterNeedsToNavigate -= _viewModelOnPresenterNeedsToNavigate;
                        _viewModel.OnPresenterNeedsToReplaceCurrent -= _viewModelOnPresenterNeedsToReplaceCurrent;
                        _viewModel.OnPresenterNeedsToReplaceWithinBackStack -= _viewModelOnPresenterNeedsToReplaceWithinBackStack;
                        _viewModel.OnPresenterNeedsToClearAll -= _viewModelOnPresenterNeedsToClearAll;
                        _viewModel.OnPresenterNeedsToClearBackStack -= _viewModelOnPresenterNeedsToClearBackStack;
                    }

                    else
                    {
                        _viewModelOnPresenterNeedsToGoBackHandler = new WeakEventHandler<EventArgs>(ViewModel_OnPresenterNeedsToGoBack).Handler;
                        _viewModelOnPresenterNeedsToRemoveModelFromBackStack = new WeakEventHandler<BaseViewModel>(ViewModel_OnPresenterNeedsToRemoveModelFromBackStack).Handler;
                        _viewModelOnPresenterNeedsToNavigate = new WeakEventHandler<BaseViewModel>(ViewModel_OnPresenterNeedsToNavigate).Handler;
                        _viewModelOnPresenterNeedsToReplaceCurrent = new WeakEventHandler<Tuple<BaseViewModel, BaseViewModel>>(ViewModel_OnPresenterNeedsToReplaceCurrent).Handler;
                        _viewModelOnPresenterNeedsToReplaceWithinBackStack = new WeakEventHandler<Tuple<BaseViewModel, BaseViewModel>>(ViewModel_OnPresenterNeedsToReplaceWithinBackStack).Handler;
                        _viewModelOnPresenterNeedsToClearAll = new WeakEventHandler<EventArgs>(ViewModel_OnPresenterNeedsToClearAll).Handler;
                        _viewModelOnPresenterNeedsToClearBackStack = new WeakEventHandler<EventArgs>(ViewModel_OnPresenterNeedsToClearBackStack).Handler;
                    }

                    if (value != null)
                    {
                        value.OnPresenterNeedsToGoBack += _viewModelOnPresenterNeedsToGoBackHandler;
                        value.OnPresenterNeedsToRemoveModelFromBackStack += _viewModelOnPresenterNeedsToRemoveModelFromBackStack;
                        value.OnPresenterNeedsToNavigate += _viewModelOnPresenterNeedsToNavigate;
                        value.OnPresenterNeedsToReplaceCurrent += _viewModelOnPresenterNeedsToReplaceCurrent;
                        value.OnPresenterNeedsToReplaceWithinBackStack += _viewModelOnPresenterNeedsToReplaceWithinBackStack;
                        value.OnPresenterNeedsToClearAll += _viewModelOnPresenterNeedsToClearAll;
                        value.OnPresenterNeedsToClearBackStack += _viewModelOnPresenterNeedsToClearBackStack;

                        // Populate the views
                        List<UIViewController> newControllers = new List<UIViewController>();
                        foreach (var backStackEntry in value.BackStack)
                        {
                            var view = ViewModelToViewConverter.Convert(backStackEntry);
                            newControllers.Add(view);
                            _liveViews.Add(new Tuple<BaseViewModel, UIViewController>(backStackEntry, view));
                        }
                        if (value.Content != null)
                        {
                            var view = ViewModelToViewConverter.Convert(value.Content);
                            newControllers.Add(view);
                            _liveViews.Add(new Tuple<BaseViewModel, UIViewController>(value.Content, view));
                        }
                        MyNavigationController.ViewControllers = newControllers.ToArray();
                    }

                    var oldViewModel = _viewModel;
                    _viewModel = value;
                    OnViewModelChanged(oldViewModel, _viewModel);
                }
                catch (Exception ex)
                {
#if DEBUG
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
#endif
                    throw ex;
                }
            }
        }

        private void ViewModel_OnPresenterNeedsToClearBackStack(object sender, EventArgs e)
        {
            foreach (var viewController in MyNavigationController.ViewControllers.Where(i => i != MyNavigationController.TopViewController))
            {
                viewController.WillMoveToParentViewController(null);
                viewController.View.RemoveFromSuperview();
                viewController.RemoveFromParentViewController();
                _liveViews.RemoveWhere(i => i.Item2 == viewController);
            }
        }

        protected virtual void OnViewModelChanged(PagedViewModel oldViewModel, PagedViewModel currentViewModel)
        {
            // Nothing
        }

        private void ViewModel_OnPresenterNeedsToClearAll(object sender, EventArgs e)
        {
            foreach (var viewController in MyNavigationController.ViewControllers)
            {
                viewController.WillMoveToParentViewController(null);
                viewController.View.RemoveFromSuperview();
                viewController.RemoveFromParentViewController();
            }
            _liveViews.Clear();
        }

        private void ViewModel_OnPresenterNeedsToReplaceWithinBackStack(object sender, Tuple<BaseViewModel, BaseViewModel> e)
        {
            var matchingView = FindViewMatchingViewModel(e.Item1);
            if (matchingView != null)
            {
                var updatedControllers = MyNavigationController.ViewControllers.ToArray();
                int index = updatedControllers.ToList().IndexOf(matchingView);

                UIViewController viewController = ViewModelToViewConverter.Convert(e.Item2);
                updatedControllers[index] = viewController;

                MyNavigationController.ViewControllers = updatedControllers;

                _liveViews.RemoveWhere(i => i.Item1 == e.Item1);
                _liveViews.Add(new Tuple<BaseViewModel, UIViewController>(e.Item2, viewController));
            }
        }

        private void ViewModel_OnPresenterNeedsToReplaceCurrent(object sender, Tuple<BaseViewModel, BaseViewModel> e)
        {
            ViewModel_OnPresenterNeedsToNavigate(sender, e.Item2);
            ViewModel_OnPresenterNeedsToRemoveModelFromBackStack(sender, e.Item1);
        }

        private void ViewModel_OnPresenterNeedsToNavigate(object sender, BaseViewModel e)
        {
            UIViewController viewController = ViewModelToViewConverter.Convert(e);
            MyNavigationController.ShowViewController(viewController, null);
            _liveViews.Add(new Tuple<BaseViewModel, UIViewController>(e, viewController));
        }

        private void ViewModel_OnPresenterNeedsToRemoveModelFromBackStack(object sender, BaseViewModel e)
        {
            UIViewController toRemove = FindViewMatchingViewModel(e);
            if (toRemove != null)
            {
                toRemove.WillMoveToParentViewController(null);
                toRemove.View.RemoveFromSuperview();
                toRemove.RemoveFromParentViewController();
                toRemove.DidMoveToParentViewController(null);
                if (toRemove is PagedViewModelPresenter)
                {
                    (toRemove as PagedViewModelPresenter).Destroy();
                }
                _liveViews.RemoveWhere(i => i.Item1 == e);
            }
        }

        internal virtual void Destroy()
        {
            // Destroying here fixes the case where SettingsListViewModel and MyAccountViewModel aren't disposed
            // when you log out
            RecursivelyDestroyDescendants(this);
            ViewModel.SetNativeView(null);
            ViewModel_OnPresenterNeedsToClearAll(null, null);
        }

        private static void RecursivelyDestroyDescendants(UIViewController controller)
        {
            foreach (var c in controller.ChildViewControllers)
            {
                if (c is PagedViewModelPresenter)
                {
                    (c as PagedViewModelPresenter).Destroy();
                }
                else
                {
                    var viewModelProp = c.GetType().GetProperty("ViewModel");
                    if (viewModelProp != null)
                    {
                        BaseViewModel viewModel = viewModelProp.GetValue(c) as BaseViewModel;
                        if (viewModel != null)
                        {
                            viewModel.SetNativeView(null);
                        }
                    }
                    RecursivelyDestroyDescendants(c);
                }
            }
        }

        private void ViewModel_OnPresenterNeedsToGoBack(object sender, EventArgs e)
        {
            var removedViewController = MyNavigationController.PopViewController(true);
            _liveViews.RemoveWhere(i => i.Item2 == removedViewController);
        }

        private UIViewController FindViewMatchingViewModel(BaseViewModel viewModel)
        {
            return _liveViews.FirstOrDefault(i => i.Item1 == viewModel)?.Item2;
        }
    }
}