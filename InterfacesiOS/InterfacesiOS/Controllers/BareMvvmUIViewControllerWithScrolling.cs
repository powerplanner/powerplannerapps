using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using BareMvvm.Core.ViewModels;
using InterfacesiOS.Views;

namespace InterfacesiOS.Controllers
{
    public class BareMvvmUIViewControllerWithScrolling<T> : BareMvvmUIViewController<T> where T : BaseViewModel
    {
        public UIScrollView ScrollView { get; private set; }
        public UIStackView StackView { get; private set; }
        public UIView StackViewContainer { get; private set; }

        public BareMvvmUIViewControllerWithScrolling()
        {
            ScrollView = new UIScrollView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ShowsHorizontalScrollIndicator = false
            };
            this.View.AddSubview(ScrollView);
            ScrollView.StretchWidthAndHeight(this.View);

            // This is here simply to allow assigning a background to the stack view
            StackViewContainer = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ScrollView.AddSubview(StackViewContainer);
            StackViewContainer.ConfigureForVerticalScrolling(ScrollView, top: TopPadding, bottom: 16, left: LeftPadding, right: RightPadding);

            StackView = new UIStackView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Vertical
            };
            StackViewContainer.AddSubview(StackView);
            StackView.StretchWidthAndHeight(StackViewContainer);
        }

        protected virtual int TopPadding { get; }
        protected virtual int BottomPadding { get; } = 16;
        protected virtual int LeftPadding { get; }
        protected virtual int RightPadding { get; }
    }
}