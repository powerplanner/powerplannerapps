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

        private readonly List<ChildWrapper> _cachedChildren = new List<ChildWrapper>();

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

        private void PopulateCachedChildren()
        {
            _cachedChildren.Clear();
            for (int i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                if (child.Visibility != ViewStates.Gone)
                {
                    _cachedChildren.Add(new ChildWrapper(child, Orientation));
                }
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            PopulateCachedChildren();

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
            float totalWeight = 0;
            for (int i = 0; i < _cachedChildren.Count; i++)
            {
                totalWeight += _cachedChildren[i].Weight;
            }

            bool autoHeightsForAll = (isVert ? heightMode == MeasureSpecMode.Unspecified : widthMode == MeasureSpecMode.Unspecified) || totalWeight == 0;

            // StackPanel essentially
            if (autoHeightsForAll)
            {
                for (int i = 0; i < _cachedChildren.Count; i++)
                {
                    var child = _cachedChildren[i];
                    child.Measure(int.MaxValue, availableSize.Secondary, secondaryMode);

                    consumed += child.MeasuredPrimaryAxis;
                    maxOtherDimension = Math.Max(maxOtherDimension, child.MeasuredSecondaryAxis);
                }

                // Re-measure children that need to expand their secondary dimension to the max dimension
                for (int i = 0; i < _cachedChildren.Count; i++)
                {
                    var child = _cachedChildren[i];
                    if (child.MeasuredSecondaryAxis != maxOtherDimension)
                    {
                        child.Measure(child.MeasuredPrimaryAxis, maxOtherDimension, MeasureSpecMode.Exactly);
                    }
                }

                SetMeasuredDimensionForOrientation(isVert, widthMode, heightMode, availableSize, consumed, maxOtherDimension);
                return;
            }

            // We measure autos FIRST, since those get priority
            for (int i = 0; i < _cachedChildren.Count; i++)
            {
                var child = _cachedChildren[i];
                if (child.Weight == 0)
                {
                    child.Measure(Math.Max(0, availableSize.Primary - consumed), availableSize.Secondary, secondaryMode);

                    consumed += child.MeasuredPrimaryAxis;
                    maxOtherDimension = Math.Max(maxOtherDimension, child.MeasuredSecondaryAxis);
                }
            }

            double weightedAvailable = Math.Max(availableSize.Primary - consumed, 0);

            if (totalWeight > 0)
            {
                for (int i = 0; i < _cachedChildren.Count; i++)
                {
                    var child = _cachedChildren[i];
                    if (child.Weight != 0)
                    {
                        var childConsumed = (int)((child.Weight / totalWeight) * weightedAvailable);

                        child.Measure(childConsumed, availableSize.Secondary, secondaryMode);

                        consumed += child.MeasuredPrimaryAxis;
                        maxOtherDimension = Math.Max(maxOtherDimension, child.MeasuredSecondaryAxis);
                    }
                }
            }

            // Re-measure children that need to expand their secondary dimension to the max dimension
            for (int i = 0; i < _cachedChildren.Count; i++)
            {
                var child = _cachedChildren[i];
                if (child.MeasuredSecondaryAxis != maxOtherDimension)
                {
                    child.Measure(child.MeasuredPrimaryAxis, maxOtherDimension, MeasureSpecMode.Exactly);
                }
            }

            SetMeasuredDimensionForOrientation(isVert, widthMode, heightMode, availableSize, consumed, maxOtherDimension);
        }

        private void SetMeasuredDimensionForOrientation(bool isVert, MeasureSpecMode widthMode, MeasureSpecMode heightMode, SizeInt availableSize, int consumed, int maxOtherDimension)
        {
            if (isVert)
            {
                SetMeasuredDimension(widthMode == MeasureSpecMode.Exactly ? availableSize.Secondary : maxOtherDimension, heightMode == MeasureSpecMode.Exactly ? availableSize.Primary : consumed);
            }
            else
            {
                SetMeasuredDimension(widthMode == MeasureSpecMode.Exactly ? availableSize.Primary : consumed, heightMode == MeasureSpecMode.Exactly ? availableSize.Secondary : maxOtherDimension);
            }
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

                RawPrimaryAxis = Orientation == Vx.Views.Orientation.Vertical ? GetHeight(View) : GetWidth(View);
                RawSecondaryAxis = Orientation == Vx.Views.Orientation.Vertical ? GetWidth(View) : GetHeight(View);
                PrimaryAxis = RawPrimaryAxis;
                SecondaryAxis = RawSecondaryAxis;
                PrimaryMargin = Orientation == Vx.Views.Orientation.Vertical ? Margin.Height : Margin.Width;
                SecondaryMargin = Orientation == Vx.Views.Orientation.Vertical ? Margin.Width : Margin.Height;

                if (IsSpecific(PrimaryAxis) && !(IsWeighted && PrimaryAxis == 0)) // When weighted, size = 0, so ignore that
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

            public int RawPrimaryAxis { get; private set; }

            public int RawSecondaryAxis { get; private set; }

            public int PrimaryMargin { get; private set; }

            public int SecondaryMargin { get; private set; }

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
                if (IsSpecific(RawPrimaryAxis) && !(IsWeighted && PrimaryAxis == 0))
                {
                    primaryAxis = MeasureSpec.MakeMeasureSpec(RawPrimaryAxis, MeasureSpecMode.Exactly);
                }
                else if (IsSpecific(primaryAxis))
                {
                    // Accomodate for margin
                    primaryAxis = MeasureSpec.MakeMeasureSpec(primaryAxis - PrimaryMargin, PrimaryAlignment == Align.Stretch && IsWeighted ? MeasureSpecMode.Exactly : MeasureSpecMode.AtMost);
                }
                else
                {
                    primaryAxis = MeasureSpec.MakeMeasureSpec(int.MaxValue, MeasureSpecMode.Unspecified);
                }


                if (IsSpecific(RawSecondaryAxis))
                {
                    secondaryAxis = MeasureSpec.MakeMeasureSpec(RawSecondaryAxis, MeasureSpecMode.Exactly);
                }
                else if (IsSpecific(secondaryAxis))
                {
                    secondaryAxis = MeasureSpec.MakeMeasureSpec(secondaryAxis - SecondaryMargin, SecondaryAlignment == Align.Stretch && secondaryMode == MeasureSpecMode.Exactly ? MeasureSpecMode.Exactly : MeasureSpecMode.AtMost);
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
            bool isVert = Orientation == Vx.Views.Orientation.Vertical;

            var finalSize = new SizeInt(
                isVert ? b - t : r - l,
                isVert ? r - l : b - t);

            float totalWeight = 0;
            int consumedByAuto = 0;
            for (int i = 0; i < _cachedChildren.Count; i++)
            {
                var child = _cachedChildren[i];
                totalWeight += child.Weight;
                if (child.Weight == 0)
                {
                    consumedByAuto += child.MeasuredPrimaryAxis;
                }
            }

            int weightedAvailable = Math.Max(0, finalSize.Primary - consumedByAuto);
            int pos = 0;

            for (int i = 0; i < _cachedChildren.Count; i++)
            {
                var child = _cachedChildren[i];

                int consumed;
                if (child.Weight == 0)
                {
                    consumed = child.MeasuredPrimaryAxis;
                }
                else
                {
                    consumed = (int)((child.Weight / totalWeight) * weightedAvailable);
                }

                child.Layout(pos, new SizeInt(consumed, finalSize.Secondary));

                pos += consumed;
            }
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