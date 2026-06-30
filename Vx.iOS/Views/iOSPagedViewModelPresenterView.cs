using System;
using BareMvvm.Core.ViewModels;
using CoreGraphics;
using InterfacesiOS.ViewModelPresenters;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSPagedViewModelPresenterView : iOSView<PagedViewModelPresenterView, UIPagedViewModelPresenterHost>
    {
        protected override void ApplyProperties(PagedViewModelPresenterView oldView, PagedViewModelPresenterView newView)
        {
            base.ApplyProperties(oldView, newView);

            View.ViewModel = newView.ViewModel;
        }
    }

    /// <summary>
    /// Hosts the native <see cref="PagedViewModelPresenter"/> (a <see cref="UIViewController"/>)
    /// inside the Vx manual-layout tree. The controller's view is positioned by frame, and the
    /// controller is wired up as a child view controller of the nearest hosting controller so
    /// navigation lifecycle events propagate correctly.
    /// </summary>
    public class UIPagedViewModelPresenterHost : UIView
    {
        private readonly PagedViewModelPresenter _presenter;
        private bool _addedToParent;

        public UIPagedViewModelPresenterHost()
        {
            _presenter = new PagedViewModelPresenter();
            _presenter.View.TranslatesAutoresizingMaskIntoConstraints = true;
            AddSubview(_presenter.View);
        }

        public PagedViewModel ViewModel
        {
            get => _presenter.ViewModel;
            set => _presenter.ViewModel = value;
        }

        public override void MovedToWindow()
        {
            base.MovedToWindow();

            if (Window != null)
            {
                if (!_addedToParent)
                {
                    var parentViewController = this.GetViewController();
                    if (parentViewController != null)
                    {
                        parentViewController.AddChildViewController(_presenter);
                        _presenter.DidMoveToParentViewController(parentViewController);
                        _addedToParent = true;
                    }
                }
            }
            else
            {
                if (_addedToParent)
                {
                    _presenter.WillMoveToParentViewController(null);
                    _presenter.RemoveFromParentViewController();
                    _addedToParent = false;
                }
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _presenter.View.Frame = Bounds;
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            // Fill whatever space the parent layout offers (used with LinearLayoutWeight(1)).
            return size;
        }
    }
}
