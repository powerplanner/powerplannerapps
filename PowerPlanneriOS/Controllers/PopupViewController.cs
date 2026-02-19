using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using BareMvvm.Core.ViewModels;
using InterfacesiOS.Controllers;
using InterfacesiOS.Views;
using PowerPlanneriOS.Helpers;
using ToolsPortable;
using PowerPlanneriOS.Views;
using PowerPlannerAppDataLibrary;

namespace PowerPlanneriOS.Controllers
{
    public abstract class PopupViewController<T> : BareMvvmUIViewController<T> where T : BaseViewModel
    {
        protected UINavigationBar NavBar { get; private set; }
        private UIBarButtonItem _backButton;
        public UINavigationItem NavItem { get; private set; }

        private UIView _contentView;
        /// <summary>
        /// Where your content should be placed. Displays below the nav bar.
        /// </summary>
        public virtual UIView ContentView { get { return _contentView; } }

        public PopupViewController()
        {
            _contentView = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            base.View.Add(_contentView);
            _contentView.StretchWidth(base.View);

            var statusBarView = UIStatusBarView.CreateAndAddTo(View);

            NavBar = new UINavigationBar()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ColorResources.ConfigureNavBar(NavBar);
            base.View.Add(NavBar);
            NavBar.StretchWidth(base.View);

            _backButton = new UIBarButtonItem()
            {
                Title = PowerPlannerResources.GetString("String_Back")
            };
            _backButton.Clicked += new WeakEventHandler<EventArgs>(_backButton_Clicked).Handler;

            NavItem = new UINavigationItem();
            NavItem.LeftBarButtonItem = _backButton;
            NavBar.Items = new UINavigationItem[] { NavItem };

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[statusBar][navBar][contentView]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "statusBar", statusBarView,
                "navBar", NavBar,
                "contentView", _contentView));
        }

        protected virtual void BackButtonClicked()
        {
            ViewModel.TryRemoveViewModelViaUserInteraction();
        }

        private void _backButton_Clicked(object sender, EventArgs e)
        {
            BackButtonClicked();
        }

        public override string Title
        {
            get { return base.Title; }
            set
            {
                NavBar.TopItem.Title = value;
                base.Title = value;
            }
        }

        public string BackButtonText
        {
            get { return _backButton.Title; }
            set { _backButton.Title = value; }
        }

        public void HideBackButton()
        {
            NavItem.LeftBarButtonItem = null;
        }

        private PopupRightNavBarButtonItem _positiveNavBarButton;
        public PopupRightNavBarButtonItem PositiveNavBarButton
        {
            get { return _positiveNavBarButton; }
            set
            {
                _positiveNavBarButton = value;

                if (value != null)
                {
                    var uiButton = new UIBarButtonItem
                    {
                        Title = value.Title
                    };
                    uiButton.Clicked += new WeakEventHandler(value.ClickAction).Handler;
                    NavItem.RightBarButtonItem = uiButton;
                }
                else
                {
                    NavItem.RightBarButtonItem = null;
                }
            }
        }
    }

    public class PopupRightNavBarButtonItem
    {
        public string Title { get; set; }
        public EventHandler<EventArgs> ClickAction { get; set; }

        public PopupRightNavBarButtonItem(string title, EventHandler<EventArgs> clickAction)
        {
            Title = title;
            ClickAction = clickAction;
        }
    }
}