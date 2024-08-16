using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid.Views
{
    public abstract class DroidView<V, N> : NativeView<V, N> where V : Vx.Views.View where N : View
    {
        private bool _hasRegisteredTappedEvent, _hasRegisteredContextClick;

        public DroidView(N view)
        {
            // Note that we can't use reflection to create views, since in Release mode the constructors
            // get linked away and creating the views will fail at runtime.
            View = view;
        }

        protected override void ApplyProperties(V oldView, V newView)
        {
            if (newView.Tapped != null && !_hasRegisteredTappedEvent)
            {
                View.Click += View_Click;
                _hasRegisteredTappedEvent = true;
            }
            else if (newView.Tapped == null && _hasRegisteredTappedEvent)
            {
                View.Click -= View_Click;
                _hasRegisteredTappedEvent = false;
            }

            if (newView.ContextMenu != null && !_hasRegisteredContextClick)
            {
                View.LongClick += View_LongClick;
                _hasRegisteredContextClick = true;
            }
            else if (newView.ContextMenu == null && _hasRegisteredContextClick)
            {
                View.LongClick -= View_LongClick;
                _hasRegisteredContextClick = false;
            }

            if (VxParentView is Vx.Views.LinearLayout parentLinearLayout)
            {
                var weight = Vx.Views.LinearLayout.GetWeight(newView);
                var isVertical = parentLinearLayout.Orientation == Vx.Views.Orientation.Vertical;

                int width;
                int height;

                if (!float.IsNaN(newView.Width))
                {
                    width = ThemeHelper.AsPx(View.Context, newView.Width);
                }
                else
                {
                    width = isVertical ? (newView.HorizontalAlignment == Vx.Views.HorizontalAlignment.Stretch ? LinearLayout.LayoutParams.MatchParent : LinearLayout.LayoutParams.WrapContent) : (weight == 0 ? LinearLayout.LayoutParams.WrapContent : 0);
                }

                if (!float.IsNaN(newView.Height))
                {
                    height = ThemeHelper.AsPx(View.Context, newView.Height);
                }
                else
                {
                    if (isVertical)
                    {
                        height = weight == 0 ? LinearLayout.LayoutParams.WrapContent : 0;
                    }
                    else
                    {
                        // Scenarios to test
                        // - What If header UI
                        if (newView.VerticalAlignment == Vx.Views.VerticalAlignment.Stretch)
                        {
                            if (float.IsNaN(parentLinearLayout.Height))
                            {
                                // This enables What If header UI
                                height = LinearLayout.LayoutParams.WrapContent;
                            }
                            else
                            {
                                height = LinearLayout.LayoutParams.MatchParent;
                            }
                        }
                        else
                        {
                            height = LinearLayout.LayoutParams.WrapContent;
                        }
                    }
                }

                View.LayoutParameters = new DroidViews.DroidVxLinearLayout.LayoutParams(width, height)
                {
                    HorizontalAlignment = newView.HorizontalAlignment,
                    VerticalAlignment = newView.VerticalAlignment,
                    Margin = new DroidViews.ThicknessInt(AsPx(newView.Margin.Left), AsPx(newView.Margin.Top), AsPx(newView.Margin.Right), AsPx(newView.Margin.Bottom)),
                    Weight = weight
                };
            }

            else if (VxParentView is Vx.Views.FrameLayout parentFrameLayout || VxParentView is Vx.Views.Border || VxParentView == null) // Null when it's top-level view
            {
                int width;
                int height;

                if (!float.IsNaN(newView.Width))
                {
                    width = ThemeHelper.AsPx(View.Context, newView.Width);
                }
                else
                {
                    width = newView.HorizontalAlignment == Vx.Views.HorizontalAlignment.Stretch ? FrameLayout.LayoutParams.MatchParent : FrameLayout.LayoutParams.WrapContent;
                }

                if (!float.IsNaN(newView.Height))
                {
                    height = ThemeHelper.AsPx(View.Context, newView.Height);
                }
                else
                {
                    height = newView.VerticalAlignment == Vx.Views.VerticalAlignment.Stretch ? FrameLayout.LayoutParams.MatchParent : FrameLayout.LayoutParams.WrapContent;
                }

                View.LayoutParameters = new FrameLayout.LayoutParams(width, height)
                {
                    Gravity = CombineGravity(newView.HorizontalAlignment.ToDroid(width), newView.VerticalAlignment.ToDroid(height)),
                    MarginStart = AsPx(newView.Margin.Left),
                    TopMargin = AsPx(newView.Margin.Top),
                    MarginEnd = AsPx(newView.Margin.Right),
                    BottomMargin = AsPx(newView.Margin.Bottom)
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

        private GravityFlags CombineGravity(GravityFlags horizontal, GravityFlags vertical)
        {
            if (horizontal == (GravityFlags)(-1) && vertical == (GravityFlags)(-1))
            {
                return (GravityFlags)(-1);
            }

            if (horizontal == (GravityFlags)(-1))
            {
                return vertical;
            }

            if (vertical == (GravityFlags)(-1))
            {
                return horizontal;
            }

            return horizontal | vertical;
        }

        private void View_LongClick(object sender, View.LongClickEventArgs e)
        {
            var cmFunc = VxView?.ContextMenu;
            if (cmFunc != null)
            {
                var cm = cmFunc();
                if (cm != null)
                {
                    cm.Show(VxViewRef);
                }
            }
        }

        private void View_Click(object sender, EventArgs e)
        {
            VxView.Tapped?.Invoke();
        }

        /// <summary>
        /// Returns the absolute pixels from a given dp value
        /// </summary>
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