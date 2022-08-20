using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Droid.DroidViews
{
    public class DroidVxLinearLayout : ViewGroup
    {
        public new class LayoutParams : ViewGroup.LayoutParams
        {
            public LayoutParams(int width, int height) : base(width, height)
            {
            }

            public float Weight { get; set; }

            public HorizontalAlignment HorizontalAlignment { get; set; }

            public VerticalAlignment VerticalAlignment { get; set; }

            public ThicknessInt Margin { get; set; }
        }

        public DroidVxLinearLayout(Context context) : base(context)
        {
        }

        private Vx.Views.Orientation _orientation;
        public Vx.Views.Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    Invalidate();
                }
            }
        }
        private struct SizeInt
        {
            public SizeInt(int primary, int secondary)
            {
                Primary = primary;
                Secondary = secondary;
            }

            public int Primary { get; set; }
            public int Secondary { get; set; }

            public override string ToString()
            {
                return $"{Primary}, {Secondary}";
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            // MeasureOverride is supposed to report the SMALLEST possible size that will fit the content. If availableSize.Height is 700
            // but content could fit in 200, then should return 200. This effectively means that the weighted heights are treated as
            // auto. This ensures that for the ViewTask dialog, even though it uses a weighted height, if the content is smaller, the
            // window will stay smaller. And I've matched this to the behavior of UWP grids, where when there's less content, the Star row
            // definitions behave as Auto. If the area can't fit all of the content, the Auto columns get first priority.

            // However this does NOT work for calendar grid when each square has an excess amount of items, since the earlier rows will
            // report they need more space and the later rows will not get enough space...
            // We could measure all weighted with infinite height, to see how little they need?
            // Think I solved it below...

            var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            var heightMode = MeasureSpec.GetMode(heightMeasureSpec);

            SizeInt availableSize;
            MeasureSpecMode secondaryMode;

            if (Orientation == Vx.Views.Orientation.Vertical)
            {
                availableSize = new SizeInt(
                    heightMode == MeasureSpecMode.Unspecified ? int.MaxValue : MeasureSpec.GetSize(heightMeasureSpec),
                    widthMode == MeasureSpecMode.Unspecified ? int.MaxValue : MeasureSpec.GetSize(widthMeasureSpec));

                secondaryMode = widthMode;
            }
            else
            {
                availableSize = new SizeInt(
                    widthMode == MeasureSpecMode.Unspecified ? int.MaxValue : MeasureSpec.GetSize(widthMeasureSpec),
                    heightMode == MeasureSpecMode.Unspecified ? int.MaxValue : MeasureSpec.GetSize(heightMeasureSpec));

                secondaryMode = heightMode;
            }

            bool isVert = Orientation == Vx.Views.Orientation.Vertical;

            int consumed = 0;
            int maxOtherDimension = 0;
            float totalWeight = GetTotalWeight();

            bool autoHeightsForAll = (isVert ? heightMode == MeasureSpecMode.Unspecified : widthMode == MeasureSpecMode.Unspecified) || totalWeight == 0;

            Action setMeasuredDimension = () =>
            {
                if (isVert)
                {
                    SetMeasuredDimension(widthMode == MeasureSpecMode.Exactly ? availableSize.Secondary : maxOtherDimension, heightMode == MeasureSpecMode.Exactly ? availableSize.Primary : consumed);
                }
                else
                {
                    SetMeasuredDimension(widthMode == MeasureSpecMode.Exactly ? availableSize.Primary : consumed, heightMode == MeasureSpecMode.Exactly ? availableSize.Secondary : maxOtherDimension);
                }
            };

            // StackPanel essentially
            if (autoHeightsForAll)
            {
                foreach (var child in WrappedVisibleChildren)
                {
                    child.Measure(int.MaxValue, availableSize.Secondary, secondaryMode);

                    consumed += child.MeasuredPrimaryAxis;
                    maxOtherDimension = Math.Max(maxOtherDimension, child.MeasuredSecondaryAxis);
                }

                setMeasuredDimension();
                return;
            }

            // We measure autos FIRST, since those get priority
            foreach (var child in WrappedVisibleChildren.Where(i => i.Weight == 0))
            {
                child.Measure(Math.Max(0, availableSize.Primary - consumed), availableSize.Secondary, secondaryMode);

                consumed += child.MeasuredPrimaryAxis;
                maxOtherDimension = Math.Max(maxOtherDimension, child.MeasuredSecondaryAxis);
            }

            double weightedAvailable = Math.Max(availableSize.Primary - consumed, 0);

            if (totalWeight > 0)
            {
                foreach (var child in WrappedVisibleChildren.Where(i => i.Weight != 0))
                {
                    var weight = child.Weight;
                    var childConsumed = (int)((weight / totalWeight) * weightedAvailable);

                    child.Measure(childConsumed, availableSize.Secondary, secondaryMode);

                    consumed += child.MeasuredPrimaryAxis;
                    maxOtherDimension = Math.Max(maxOtherDimension, child.MeasuredSecondaryAxis);
                }
            }

            setMeasuredDimension();
        }

        private enum Align
        {
            Stretch = 0,
            Start = 1,
            Center = 2,
            End = 3
        }

        private class ChildWrapper
        {
            public Android.Views.View View { get; private set; }
            public Vx.Views.Orientation Orientation { get; private set; }

            public ChildWrapper(Android.Views.View view, Vx.Views.Orientation orientation)
            {
                View = view;
                Orientation = orientation;

                var lp = view.LayoutParameters as LayoutParams;

                if (lp != null)
                {
                    PrimaryAlignment = orientation == Vx.Views.Orientation.Vertical ? (Align)lp.VerticalAlignment : (Align)lp.HorizontalAlignment;
                    SecondaryAlignment = orientation == Vx.Views.Orientation.Vertical ? (Align)lp.HorizontalAlignment : (Align)lp.VerticalAlignment;
                    Margin = lp.Margin;
                    Weight = lp.Weight;
                }

                PrimaryAxis = Orientation == Vx.Views.Orientation.Vertical ? GetHeight(View) : GetWidth(View);
                SecondaryAxis = Orientation == Vx.Views.Orientation.Vertical ? GetWidth(View) : GetHeight(View);

                if (IsSpecific(PrimaryAxis) && !(Weight != 0 && PrimaryAxis == 0)) // When weighted, size = 0, so ignore that
                {
                    PrimaryAxis += Orientation == Vx.Views.Orientation.Vertical ? Margin.Height : Margin.Width;
                }

                if (IsSpecific(SecondaryAxis))
                {
                    SecondaryAxis += Orientation == Vx.Views.Orientation.Vertical ? Margin.Width : Margin.Height;
                }
            }

            /// <summary>
            /// If vertical orientation, returns height, otherwise returns width. Includes margin.
            /// </summary>
            public int PrimaryAxis { get; private set; }

            public int SecondaryAxis { get; private set; }

            public float Weight { get; private set; }

            public bool IsWeighted => Weight > 0;

            public ThicknessInt Margin { get; private set; }

            /// <summary>
            /// If vertical orientation, returns vertical alignment
            /// </summary>
            public Align PrimaryAlignment { get; private set; }

            public Align SecondaryAlignment { get; private set; }

            /// <summary>
            /// Includes margin.
            /// </summary>
            public int MeasuredPrimaryAxis
            {
                get
                {
                    if (Orientation == Vx.Views.Orientation.Vertical)
                    {
                        return View.MeasuredHeight + Margin.Height;
                    }
                    else
                    {
                        return View.MeasuredWidth + Margin.Width;
                    }
                }
            }

            public int MeasuredSecondaryAxis
            {
                get
                {
                    if (Orientation == Vx.Views.Orientation.Vertical)
                    {
                        return View.MeasuredWidth + Margin.Width;
                    }
                    else
                    {
                        return View.MeasuredHeight + Margin.Height;
                    }
                }
            }

            /// <summary>
            /// Auto-accomodates for margin (if you pass in 60, it'll auto-reduce to 50 if there's 10 of margin)
            /// </summary>
            /// <param name="primaryAxis"></param>
            /// <param name="secondaryAxis"></param>
            public void Measure(int primaryAxis, int secondaryAxis, MeasureSpecMode secondaryMode)
            {
                // If child has a specific size, use that size (when weighted, size = 0, so ignore that)
                if (IsSpecific(PrimaryAxis) && !(IsWeighted && PrimaryAxis == 0))
                {
                    primaryAxis = MeasureSpec.MakeMeasureSpec(PrimaryAxis, MeasureSpecMode.Exactly);
                }
                else if (IsSpecific(primaryAxis))
                {
                    // Accomodate for margin
                    primaryAxis = MeasureSpec.MakeMeasureSpec(primaryAxis - (Orientation == Vx.Views.Orientation.Vertical ? Margin.Height : Margin.Width), PrimaryAlignment == Align.Stretch && IsWeighted ? MeasureSpecMode.Exactly : MeasureSpecMode.AtMost);
                }
                else
                {
                    primaryAxis = MeasureSpec.MakeMeasureSpec(int.MaxValue, MeasureSpecMode.Unspecified);
                }


                if (IsSpecific(SecondaryAxis))
                {
                    secondaryAxis = MeasureSpec.MakeMeasureSpec(SecondaryAxis, MeasureSpecMode.Exactly);
                }
                else if (IsSpecific(secondaryAxis))
                {
                    secondaryAxis = MeasureSpec.MakeMeasureSpec(secondaryAxis - (Orientation == Vx.Views.Orientation.Vertical ? Margin.Width : Margin.Height), SecondaryAlignment == Align.Stretch && secondaryMode == MeasureSpecMode.Exactly ? MeasureSpecMode.Exactly : MeasureSpecMode.AtMost);
                }
                else
                {
                    secondaryAxis = MeasureSpec.MakeMeasureSpec(int.MaxValue, MeasureSpecMode.Unspecified);
                }

                if (Orientation == Vx.Views.Orientation.Vertical)
                {
                    View.Measure(secondaryAxis, primaryAxis);
                }
                else
                {
                    View.Measure(primaryAxis, secondaryAxis);
                }
            }

            private bool IsSpecific(int axis)
            {
                return axis != LayoutParams.WrapContent && axis != LayoutParams.MatchParent && axis != int.MaxValue;
            }

            public void Layout(int pos, SizeInt finalSize)
            {
                if (Orientation == Vx.Views.Orientation.Vertical)
                {
                    int childT = pos + Margin.Top;
                    int childB = childT + View.MeasuredHeight;

                    // Horizontal alignment
                    switch (SecondaryAlignment)
                    {
                        case Align.Stretch:
                            View.Layout(Margin.Left, childT, finalSize.Secondary - Margin.Right, childB);
                            break;

                        case Align.Start:
                            View.Layout(Margin.Left, childT, View.MeasuredWidth + Margin.Left, childB);
                            break;

                        case Align.Center:
                            var center = (int)((finalSize.Secondary + Margin.Left - Margin.Right) / 2.0); // Account for margins
                            int offset = (int)(View.MeasuredWidth / 2.0);
                            View.Layout(center - offset, childT, center + offset, childB);
                            break;

                        case Align.End:
                            View.Layout(finalSize.Secondary - View.MeasuredWidth - Margin.Right, childT, finalSize.Secondary - Margin.Right, childB);
                            break;
                    }
                }
                else
                {
                    int childL = pos + Margin.Left;
                    int childR = childL + View.MeasuredWidth;

                    // Vertical alignment
                    switch (SecondaryAlignment)
                    {
                        case Align.Stretch:
                            View.Layout(childL, Margin.Top, childR, finalSize.Secondary - Margin.Bottom);
                            break;

                        case Align.Start:
                            View.Layout(childL, Margin.Top, childR, View.MeasuredHeight + Margin.Top);
                            break;

                        case Align.Center:
                            var center = (int)((finalSize.Secondary + Margin.Top - Margin.Bottom) / 2.0); // Account for margins
                            int offset = (int)(View.MeasuredHeight / 2.0);
                            View.Layout(childL, center - offset, childR, center + offset);
                            break;

                        case Align.End:
                            View.Layout(childL, finalSize.Secondary - View.MeasuredHeight - Margin.Bottom, childR, finalSize.Secondary - Margin.Bottom);
                            break;
                    }
                }
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            float totalWeight = GetTotalWeight();
            bool isVert = Orientation == Vx.Views.Orientation.Vertical;

            var finalSize = new SizeInt(
                isVert ? b - t : r - l,
                isVert ? r - l : b - t);

            //bool autoHeightsForAll = double.IsPositiveInfinity(isVert ? finalSize.Height : finalSize.Width) || totalWeight == 0;


            int pos = 0;

            //if (autoHeightsForAll)
            //{
            //    foreach (var child in Children.Where(i => i.Visibility == Visibility.Visible))
            //    {
            //        child.Arrange(new Rect(isVert ? new Point(0, pos) : new Point(pos, 0), isVert ? new Size(finalSize.Width, child.MeasuredHeight) : new Size(child.MeasuredWidth, finalSize.Height)));
            //        pos += isVert ? child.MeasuredHeight : child.MeasuredWidth;
            //    }
            //}

            //else
            {
                int consumedByAuto = WrappedVisibleChildren.Where(i => i.Weight == 0).Sum(i => i.MeasuredPrimaryAxis);
                int weightedAvailable = Math.Max(0, finalSize.Primary - consumedByAuto);

                foreach (var child in WrappedVisibleChildren)
                {
                    var weight = child.Weight;

                    int consumed;
                    if (weight == 0)
                    {
                        consumed = child.MeasuredPrimaryAxis;
                    }
                    else
                    {
                        consumed = (int)((weight / totalWeight) * weightedAvailable);
                    }

                    child.Layout(pos, new SizeInt(consumed, finalSize.Secondary));

                    pos += consumed;
                }
            }
        }

        private float GetTotalWeight()
        {
            return Children.Where(i => i.Visibility != ViewStates.Gone).Sum(i => GetWeight(i));
        }

        private IEnumerable<Android.Views.View> Children
        {
            get
            {
                for (int i = 0; i < ChildCount; i++)
                {
                    yield return GetChildAt(i);
                }
            }
        }

        private IEnumerable<ChildWrapper> WrappedVisibleChildren
        {
            get
            {
                for (int i = 0; i < ChildCount; i++)
                {
                    var child = GetChildAt(i);
                    if (child.Visibility != ViewStates.Gone)
                    {
                        yield return new ChildWrapper(child, Orientation);
                    }
                }
            }
        }

        private static HorizontalAlignment GetHorizontalAlignment(Android.Views.View view)
        {
            return (view.LayoutParameters as LayoutParams)?.HorizontalAlignment ?? HorizontalAlignment.Stretch;
        }

        private static VerticalAlignment GetVerticalAlignment(Android.Views.View view)
        {
            return (view.LayoutParameters as LayoutParams)?.VerticalAlignment ?? VerticalAlignment.Stretch;
        }

        private static float GetWeight(Android.Views.View view)
        {
            if (view.LayoutParameters is LayoutParams lp)
            {
                return lp.Weight;
            }

            return 0;
        }

        private static ThicknessInt GetMargin(Android.Views.View view)
        {
            if (view.LayoutParameters is LayoutParams lp)
            {
                return lp.Margin;
            }

            return new ThicknessInt();
        }

        private static int GetHeight(Android.Views.View view)
        {
            return view.LayoutParameters?.Height ?? ViewGroup.LayoutParams.WrapContent;
        }

        private static int GetWidth(Android.Views.View view)
        {
            return view.LayoutParameters?.Width ?? ViewGroup.LayoutParams.WrapContent;
        }
    }
}