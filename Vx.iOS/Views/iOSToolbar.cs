using System;
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

        protected override void ApplyProperties(Toolbar oldView, Toolbar newView)
        {
            base.ApplyProperties(oldView, newView);

            View.BackgroundColor = newView.BackgroundColor.ToUI();
            NavBar.BarTintColor = newView.BackgroundColor.ToUI();

            if (oldView?.ForegroundColor != newView.ForegroundColor)
            {
                NavBar.TintColor = newView.ForegroundColor.ToUI();

                if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                {
                    NavBar.TitleTextAttributes = new UIStringAttributes()
                    {
                        ForegroundColor = newView.ForegroundColor.ToUI()
                    };
                }
            }

            if (oldView?.BackgroundColor != newView.BackgroundColor || oldView?.ForegroundColor != newView.ForegroundColor)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    var appearance = new UINavigationBarAppearance();
                    appearance.ConfigureWithOpaqueBackground();
                    appearance.BackgroundColor = View.BackgroundColor;
                    appearance.TitleTextAttributes = new UIStringAttributes()
                    {
                        ForegroundColor = NavBar.TintColor
                    };

                    NavBar.StandardAppearance = appearance;
                    NavBar.ScrollEdgeAppearance = appearance;
                }
            }

            NavBar.TopItem.Title = newView.Title;
        }
    }
}

