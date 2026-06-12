using System;
using System.Collections.Generic;
using System.Linq;
using ToolsPortable;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public class iOSToolbar : iOSView<Vx.Views.Toolbar, UIView>
    {
        private UINavigationBar NavBar;


        public iOSToolbar()
        {
            var statusBarView = UIStatusBarView.CreateAndAddTo(View);
            statusBarView.StretchWidth(View);

            NavBar = new UINavigationBar()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Translucent = false,
                Items = new UINavigationItem[]
                {
                    new UINavigationItem()
                }
            };
            View.AddSubview(NavBar);
            NavBar.StretchWidth(View);

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[statusBar][navBar]|", NSLayoutFormatOptions.DirectionLeadingToTrailing,
                "statusBar", statusBarView,
                "navBar", NavBar));
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

