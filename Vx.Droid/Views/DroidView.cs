﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid.Views
{
    public abstract class DroidView<V, N> : NativeView<V, N> where V : Vx.Views.View where N : View
    {
        public DroidView(N view)
        {
            // Note that we can't use reflection to create views, since in Release mode the constructors
            // get linked away and creating the views will fail at runtime.
            View = view;
        }

        protected override void ApplyProperties(V oldView, V newView)
        {
            if (VxParentView is Vx.Views.LinearLayout parentLinearLayout)
            {
                var weight = Vx.Views.LinearLayout.GetWeight(newView);
                var isVertical = parentLinearLayout.Orientation == Vx.Views.Orientation.Vertical;

                // For height, we always use WrapContent, it makes things behave correctly
                View.LayoutParameters = new LinearLayout.LayoutParams(
                    width: isVertical ? (newView.HorizontalAlignment == Vx.Views.HorizontalAlignment.Stretch ? LinearLayout.LayoutParams.MatchParent : LinearLayout.LayoutParams.WrapContent) : (weight == 0 ? LinearLayout.LayoutParams.WrapContent : 0),
                    height: isVertical ? (weight == 0 ? LinearLayout.LayoutParams.WrapContent : 0) : LinearLayout.LayoutParams.WrapContent)
                {
                    Gravity = isVertical ? newView.HorizontalAlignment.ToDroid() : newView.VerticalAlignment.ToDroid(),
                    MarginStart = AsPx(newView.Margin.Left),
                    TopMargin = AsPx(newView.Margin.Top),
                    MarginEnd = AsPx(newView.Margin.Right),
                    BottomMargin = AsPx(newView.Margin.Bottom),
                    Weight = weight
                };
            }

            else
            {
                // For transparent content buttons, we center-align
                View.LayoutParameters = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.MatchParent, VxParentView is Vx.Views.TransparentContentButton ? FrameLayout.LayoutParams.WrapContent : FrameLayout.LayoutParams.MatchParent)
                {
                    MarginStart = AsPx(newView.Margin.Left),
                    TopMargin = AsPx(newView.Margin.Top),
                    MarginEnd = AsPx(newView.Margin.Right),
                    BottomMargin = AsPx(newView.Margin.Bottom),
                    Gravity = VxParentView is Vx.Views.TransparentContentButton ? GravityFlags.Center : default(GravityFlags)
                };
            }

            View.Alpha = newView.Opacity;
        }

        /// <summary>
        /// Returns the absolute pixels from a given dp value
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dp"></param>
        /// <returns></returns>
        public static int AsPx(double dp)
        {
            return (int)Math.Round(AsPxPrecise(dp));
        }

        public static float AsPxPrecise(double dp)
        {
            var context = VxDroidExtensions.ApplicationContext;

            return TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)dp, context.Resources.DisplayMetrics);
        }

        internal void ReconcileChildren(IList<Vx.Views.View> oldList, IList<Vx.Views.View> newList, ViewGroup viewGroup)
        {
            ReconcileList(
                oldList,
                newList,
                insert: (i, v) => viewGroup.AddView(v.CreateDroidView(VxView), i),
                remove: i => viewGroup.RemoveViewAt(i),
                replace: (i, v) =>
                {
                    viewGroup.RemoveViewAt(i);
                    viewGroup.AddView(v.CreateDroidView(VxView), i);
                },
                clear: () => viewGroup.RemoveAllViews()
                );
        }
    }
}