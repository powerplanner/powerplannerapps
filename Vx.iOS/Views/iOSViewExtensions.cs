using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace Vx.iOS.Views
{
    public static class iOSViewExtensions
    {
        public static UIView StretchWidthAndHeight(this UIView view, UIView parentView, float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            // https://gist.github.com/twostraws/a02d4cc09fc7bc16859c
            // http://commandshift.co.uk/blog/2013/01/31/visual-format-language-for-autolayout/
            //parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:|-({left})-[view]-({right})-|", NSLayoutFormatOptions.AlignAllCenterX, null, new NSDictionary("view", view)));
            //parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-({top})-[view]-({bottom})-|", NSLayoutFormatOptions.AlignAllCenterX, null, new NSDictionary("view", view)));

            view.StretchWidth(parentView, left: left, right: right);
            view.StretchHeight(parentView, top: top, bottom: bottom);

            return view;
        }
        public static UIView StretchWidth(this UIView view, UIView parentView, float left = 0, float right = 0)
        {
            parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"H:|-({left})-[view]-({right})-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("view", view)));

            return view;
        }

        public static UIView StretchHeight(this UIView view, UIView parentView, float top = 0, float bottom = 0)
        {
            parentView.AddConstraints(NSLayoutConstraint.FromVisualFormat($"V:|-({top})-[view]-({bottom})-|", NSLayoutFormatOptions.DirectionLeadingToTrailing, null, new NSDictionary("view", view)));

            return view;
        }
    }
}