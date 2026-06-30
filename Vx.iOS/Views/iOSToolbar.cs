using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using ToolsPortable;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    /// <summary>
    /// Root view for the toolbar. Hosts a native UINavigationBar (kept as a native control) plus a
    /// status-bar background, but lays them out MANUALLY by frame to fit the manual Vx layout engine
    /// (no Auto Layout / SystemLayoutSizeFittingSize, which was unreliable - it returned a zero
    /// height before the nav bar's intrinsic size / safe area resolved). Height = safe-area top +
    /// nav-bar height; we re-measure when the safe area changes.
    /// </summary>
    public class UIToolbarRootView : UIView
    {
        private UIView _statusBar;
        private UINavigationBar _navBar;

        public void Configure(UIView statusBar, UINavigationBar navBar)
        {
            _statusBar = statusBar;
            _navBar = navBar;
        }

        private nfloat NavBarHeight
        {
            get
            {
                nfloat h = _navBar?.IntrinsicContentSize.Height ?? 0;
                if (h <= 0)
                {
                    h = 44; // Standard navigation bar height.
                }
                return h;
            }
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            nfloat width = (size.Width > 0 && size.Width < UIViewWrapper.UnboundedSize)
                ? size.Width
                : UIScreen.MainScreen.Bounds.Width;

            return new CGSize(width, SafeAreaInsets.Top + NavBarHeight);
        }

        public override CGSize IntrinsicContentSize => new CGSize(NoIntrinsicMetric, SafeAreaInsets.Top + NavBarHeight);

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            nfloat top = SafeAreaInsets.Top;

            if (_statusBar != null)
            {
                _statusBar.Frame = new CGRect(0, 0, Bounds.Width, top);
            }

            if (_navBar != null)
            {
                _navBar.Frame = new CGRect(0, top, Bounds.Width, MaxF(0, Bounds.Height - top));
            }
        }

        public override void SafeAreaInsetsDidChange()
        {
            base.SafeAreaInsetsDidChange();

            // The toolbar height depends on the safe-area top inset, which resolves after the view
            // enters the hierarchy. Re-measure and notify the parent Vx layout to re-measure too.
            InvalidateIntrinsicContentSize();
            SetNeedsLayout();
            UIPanel.PropagateLayoutDirty(Superview);
        }

        private static nfloat MaxF(nfloat a, nfloat b) => a > b ? a : b;
    }

    public class iOSToolbar : iOSView<Vx.Views.Toolbar, UIToolbarRootView>
    {
        private UINavigationBar NavBar;
        private UIView _statusBarView;

        public iOSToolbar()
        {
            // Manual layout: status bar background + nav bar are positioned by frame in
            // UIToolbarRootView.LayoutSubviews (no Auto Layout constraints).
            _statusBarView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = true
            };
            View.AddSubview(_statusBarView);

            NavBar = new UINavigationBar()
            {
                TranslatesAutoresizingMaskIntoConstraints = true,
                Translucent = false,
                Items = new UINavigationItem[]
                {
                    new UINavigationItem()
                }
            };
            View.AddSubview(NavBar);

            View.Configure(_statusBarView, NavBar);
        }

        private UIBackBarButtonItem _backButton;

        protected override void ApplyProperties(Toolbar oldView, Toolbar newView)
        {
            base.ApplyProperties(oldView, newView);

            if (OperatingSystem.IsIOSVersionAtLeast(16))
            {
                NavBar.TopItem.Style = newView.AlignTitleToLeft ? UINavigationItemStyle.Editor : UINavigationItemStyle.Navigator;
            }

            View.BackgroundColor = newView.BackgroundColor.ToUI();
            _statusBarView.BackgroundColor = newView.BackgroundColor.ToUI();
            NavBar.BarTintColor = newView.BackgroundColor.ToUI();

            if (oldView?.ForegroundColor != newView.ForegroundColor)
            {
                NavBar.TintColor = newView.ForegroundColor.ToUI();

                NavBar.TitleTextAttributes = new UIStringAttributes()
                {
                    ForegroundColor = newView.ForegroundColor.ToUI()
                };
            }

            if (oldView?.BackgroundColor != newView.BackgroundColor || oldView?.ForegroundColor != newView.ForegroundColor)
            {
                SetNavigationBarAppearance(NavBar, View.BackgroundColor, NavBar.TintColor);
            }

            NavBar.TopItem.Title = newView.Title;

            if (!Vx.Views.MenuItem.AreSame(oldView?.PrimaryCommands, newView.PrimaryCommands)
                || !Vx.Views.MenuItem.AreSame(oldView?.SecondaryCommands, newView.SecondaryCommands))
            {
                NavBar.TopItem.RightBarButtonItems = GetRightBarButtonItems(newView).ToArray();
            }

            if (newView.OnBack != null)
            {
                if (_backButton == null)
                {
                    _backButton = new UIBackBarButtonItem();
                    _backButton.Clicked += _backButton_Clicked;
                }

                _backButton.Title = newView.BackText ?? "Back";

                if (NavBar.TopItem.LeftBarButtonItem == null)
                {
                    NavBar.TopItem.LeftBarButtonItem = _backButton;
                }
            }
            else
            {
                NavBar.TopItem.LeftBarButtonItem = null;
            }
        }

        private void _backButton_Clicked(object sender, EventArgs e)
        {
            VxView.OnBack?.Invoke();
        }

        private IEnumerable<UIBarButtonItem> GetRightBarButtonItems(Toolbar toolbar)
        {
            if (toolbar.SecondaryCommands != null && toolbar.SecondaryCommands.Any())
            {
                yield return new UIBarButtonItem(UIImage.FromBundle("MenuVerticalIcon").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), VxiOSContextMenu.CreateMenu(toolbar.SecondaryCommands));
            }

            if (toolbar.PrimaryCommands != null)
            {
                foreach (var command in toolbar.PrimaryCommands)
                {
                    var button = command.ToUIBarButtonItem(skipClickHandler: true);
                    if (command.Click != null)
                    {
                        button.Clicked += delegate
                        {
                            command.Click();
                        };
                    }
                    else if (command.SubItems != null && command.SubItems.Any(i => i != null))
                    {
                        button.Menu = VxiOSContextMenu.CreateMenu(command.SubItems);
                    }
                    yield return button;
                }
            }
        }

        private class UIBackBarButtonItem : UIBarButtonItem
        {
            private UIButton _button => base.CustomView as UIButton;

            public new event EventHandler Clicked;

            public new string Title
            {
                get => _button.Title(UIControlState.Normal);
                set
                {
                    if (value != Title)
                    {
                        _button.SetTitle(value, UIControlState.Normal);
                        _button.SizeToFit();
                    }
                }
            }

            public UIBackBarButtonItem() : base(CreateContent())
            {
                _button.TouchUpInside += _button_TouchUpInside;
            }

            private void _button_TouchUpInside(object sender, EventArgs e)
            {
                Clicked?.Invoke(this, e);
            }

            private static UIButton CreateContent()
            {
                var _backButtonContents = new UIButton(UIButtonType.Custom);
                _backButtonContents.SetImage(UIImage.FromBundle("ToolbarBack").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);

                _backButtonContents.SetTitle("Back", UIControlState.Normal);
                _backButtonContents.SizeToFit();
                return _backButtonContents;
            }
        }

        private static void SetNavigationBarAppearance(UINavigationBar navBar, UIColor backgroundColor, UIColor tintColor)
        {
            var appearance = new UINavigationBarAppearance();
            appearance.ConfigureWithOpaqueBackground();
            appearance.BackgroundColor = backgroundColor;
            appearance.TitleTextAttributes = new UIStringAttributes()
            {
                ForegroundColor = tintColor
            };

            navBar.StandardAppearance = appearance;
            navBar.ScrollEdgeAppearance = appearance;
        }
    }
}

