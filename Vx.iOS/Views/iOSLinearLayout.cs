using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    /*
     * iOS views have a few key concepts...
     * 
     * IntrinsicContentSize - The minimum size a view intrinsicly needs. Some views, like UIView, don't have an intrinsic size of their own. But views like a Button or Label do (however much space they need to render their content).
     * 
     * On settings page, I have...
     * 
     * - ScrollView
     *   - LinearLayout (vertical)
     *     - Current semester
     *     - Sync status
     *     - ListItemButton (9 of these)
     *       - LinearLayout (horizontal)
     *         - FontIcon
     *         - LinearLayout (vertical)
     *           - TextBlock
     *           - TextBlock
     *     - ListItemButton
     *       - LinearLayout (horizontal)
     *         - FontIcon
     *         - LinearLayout (vertical)
     *           - TextBlock
     *           - TextBlock
     *           
     *           
     * SystemLayoutSizeFittingSize is getting called 18 times, never once is Orientation horizontal though. Render only gets called once, so each of these are individual LinearLayouts, but for some reason SystemLayoutSizeFittingSize is getting called before Orientation is set.
     * 
     * When a view is added to a parent view that's using Auto Layout, the parent view does NOT call SystemLayoutSizeFittingSize. For example, ListItemButton doesn't call that when determining its own size. It DOES call IntrinsicContentSize.
     * 
     * A UIView that isn't a primitive like UILabel or UIButton will return IntrinsicContentSize of -1, -1. For example, a UIView that has a few child views and uses auto layout to arrange those will return -1, -1. However, that's okay, the UIStackView can still figure out the proper height of that view... somehow.
     * 
     * UIStackView applies constraints on the subviews... it sets TranslatesAutoresizingMaskIntoConstraints to false and then adds constraints accordingly.
     * 
     * So I have...
     * 
     * |[weight=auto][weight=1]| (The content hugging should make the weight=1 one fill)
     * 
     * ContentHugging/Resistance ONLY works when the view has an intrinsic size (which any of the UIViews don't).
     * 
     * Views inside auto layouts need to have an IntrinsicContentSize otherwise auto layout will consider them 0.
     * 
     * 
     * SetNeedsLayout does NOT seem to propogate to parent views. If a child calls SetNeedsLayout, it'll subsequently call LayoutSubviews, but the parent view never gets laid out.
     * 
     * When a child UILabel's text changes, both LayoutSubviews() and InvalidateIntrinsicContentSize() gets called... but nothing ever propagates upwards, and I can't figure out how to make that propagate upwards.
     * */
    public class iOSLinearLayout : iOSView<LinearLayout, UILinearLayout>
    {
        protected override void ApplyProperties(LinearLayout oldView, LinearLayout newView)
        {
            base.ApplyProperties(oldView, newView);

            // Temp background color for debugging purposes
            //View.BackgroundColor = UIColor.FromRGBA(0, 0, 255, 15);
            View.BackgroundColor = newView.BackgroundColor.ToUI();

            bool changed = false;

            if (View.Orientation != newView.Orientation)
            {
                View.Orientation = newView.Orientation;
                changed = true;
            }

            changed = View.SetHorizontalAlignment(newView.HorizontalAlignment) || changed;
            changed = View.SetVerticalAlignment(newView.VerticalAlignment) || changed;

            ReconcileList(
                oldView?.Children,
                newView.Children,
                insert: (i, v) =>
                {
                    var childView = v.CreateUIView(VxParentView);
                    View.InsertArrangedSubview(childView, i);
                    changed = true;
                },
                remove: (i) =>
                {
                    View.RemoveArrangedSubview(i);
                    changed = true;
                },
                replace: (i, v) =>
                {
                    View.RemoveArrangedSubview(i);

                    var childView = v.CreateUIView(VxParentView);
                    View.InsertArrangedSubview(childView, i);

                    changed = true;
                },
                clear: () =>
                {
                    View.ClearArrangedSubviews();

                    changed = true;
                }
                );

            var children = newView.Children.Where(i => i != null).ToList();
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                changed = View.SetWeight(i, LinearLayout.GetWeight(child)) || changed;
            }

            if (changed)
            {
                View.UpdateAllConstraints();
            }
        }
    }

    public class StretchingUIView : UIView
    {
        private Orientation? _stretchingOrientation;
        public Orientation? StretchingOrientation
        {
            get => _stretchingOrientation;
            set
            {
                if (!object.Equals(_stretchingOrientation, value))
                {
                    _stretchingOrientation = value;
                    InvalidateIntrinsicContentSize();
                }
            }
        }

        public override CoreGraphics.CGSize IntrinsicContentSize
        {
            get
            {
                if (StretchingOrientation == null)
                {
                    return new CoreGraphics.CGSize(-1, -1);
                }

                if (StretchingOrientation == Orientation.Vertical)
                {
                    return new CoreGraphics.CGSize(0, 50000);
                }
                else
                {
                    return new CoreGraphics.CGSize(50000, 0);
                }
            }
        }
    }

    public class UILinearLayout : UIView
    {
        private Orientation _orientation;
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (value != _orientation)
                {
                    _orientation = value;
                }
            }
        }

        public HorizontalAlignment HorizontalAlignment { get; private set; }

        /// <summary>
        /// Sets the horizontal alignment of the linearlayout itself
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetHorizontalAlignment(HorizontalAlignment value)
        {
            if (HorizontalAlignment != value)
            {
                HorizontalAlignment = value;
                return true;
            }

            return false;
        }

        public VerticalAlignment VerticalAlignment { get; private set; }

        /// <summary>
        /// Sets the vertical alignment of the linearlayout itself
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetVerticalAlignment(VerticalAlignment value)
        {
            if (VerticalAlignment != value)
            {
                VerticalAlignment = value;
                return true;
            }

            return false;
        }

        public bool SetWeight(int index, float weight)
        {
            if (_arrangedSubviews[index].Weight != weight)
            {
                _arrangedSubviews[index].Weight = weight;
                return true;
            }

            return false;
        }

        public void ClearArrangedSubviews()
        {
            _arrangedSubviews.Clear();
            foreach (var subview in Subviews)
            {
                subview.RemoveFromSuperview();
            }
            RemoveConstraints(Constraints);
        }

        public void RemoveArrangedSubview(int index)
        {
            _arrangedSubviews.RemoveAt(index);
            Subviews[index].RemoveFromSuperview();
        }

        public void InsertArrangedSubview(UIViewWrapper subview, int index)
        {
            InsertArrangedSubview(subview, index, 0);
        }

        private class ArrangedSubview
        {
            public UILinearLayout Parent { get; set; }
            public UIViewWrapper Subview { get; set; }
            public float Weight { get; set; }   

            public ArrangedSubview(UILinearLayout parent, UIViewWrapper subview, float weight)
            {
                Parent = parent;
                Subview = subview;
                Weight = weight;
            }

            private bool IsVertical => Parent.Orientation == Orientation.Vertical;

            public nfloat MeasuredPrimaryAxis => IsVertical ? GetSize(Subview.IntrinsicContentSize.Height) : GetSize(Subview.IntrinsicContentSize.Width);

            public SizeF DesiredSize { get; private set; }

            public SizeF Measure(SizeF availableSize)
            {
                if (IsVertical)
                {
                    var measured = Subview.Measure(new CGSize(availableSize.Secondary, availableSize.Primary));
                    DesiredSize = new SizeF(measured.Height, measured.Width);
                    return DesiredSize;
                }
                else
                {
                    var measured = Subview.Measure(new CGSize(availableSize.Primary, availableSize.Secondary));
                    DesiredSize = new SizeF(measured.Width, measured.Height);
                    return DesiredSize;
                }
            }

            public void Layout(nfloat pos, SizeF size)
            {
                if (IsVertical)
                {
                    Subview.Arrange(new CGPoint(0, pos), new CGSize(size.Secondary, size.Primary));
                }
                else
                {
                    Subview.Arrange(new CGPoint(pos, 0), new CGSize(size.Primary, size.Secondary));
                }
            }
        }

        private List<ArrangedSubview> _arrangedSubviews = new List<ArrangedSubview>();

        public void InsertArrangedSubview(UIViewWrapper subview, int index, float weight)
        {
            InsertSubview(subview.View, index);

            ArrangedSubview curr = new ArrangedSubview(this, subview, weight);

            _arrangedSubviews.Insert(index, curr);
        }

        private struct SizeF
        {
            public SizeF(nfloat primary, nfloat secondary)
            {
                Primary = primary;
                Secondary = secondary;
            }

            public nfloat Primary { get; set; }
            public nfloat Secondary { get; set; }

            public override string ToString()
            {
                return $"{Primary}, {Secondary}";
            }
        }

        /// <summary>
        /// Returns the appropriate size for the content. Returns UIViewNoIntrinsicMetric for a dimension if doesn't have a preferred size.
        /// </summary>
        public override CGSize IntrinsicContentSize => CalculateMinSize();

        private float GetTotalWeight()
        {
            return _arrangedSubviews.Sum(i => i.Weight);
        }

        private CGSize CalculateMinSize()
        {
            var availableSize = new SizeF(float.MaxValue, float.MaxValue);

            bool isVert = Orientation == Vx.Views.Orientation.Vertical;

            nfloat consumed = 0;
            nfloat maxOtherDimension = 0;
            bool consumedHasIntrinsic = false;
            bool otherDimensionHasIntrinsic = false;
            float totalWeight = GetTotalWeight();

            // StackPanel essentially
            {
                int children = _arrangedSubviews.Count;
                foreach (var child in _arrangedSubviews)
                {
                    var childSize = GetSize(child.Subview.IntrinsicContentSize);

                    if (childSize.Primary != UIView.NoIntrinsicMetric)
                    {
                        consumed += childSize.Primary;
                        consumedHasIntrinsic = true;
                    }

                    if (childSize.Secondary != UIView.NoIntrinsicMetric && childSize.Secondary >= maxOtherDimension)
                    {
                        maxOtherDimension = childSize.Secondary;
                        otherDimensionHasIntrinsic = true;
                    }
                }

                if (!consumedHasIntrinsic && _arrangedSubviews.Count > 0)
                {
                    consumed = UIView.NoIntrinsicMetric;
                }
                if (!otherDimensionHasIntrinsic && _arrangedSubviews.Count > 0)
                {
                    maxOtherDimension = UIView.NoIntrinsicMetric;
                }

                return isVert ? new CGSize(maxOtherDimension, consumed) : new CGSize(consumed, maxOtherDimension);
            }
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            bool isVert = Orientation == Vx.Views.Orientation.Vertical;
            var availableSize = isVert ? new SizeF(size.Height, size.Width) : new SizeF(size.Width, size.Height);

            nfloat consumed = 0;
            nfloat maxOtherDimension = 0;
            float totalWeight = GetTotalWeight();

            bool autoHeightsForAll = totalWeight == 0;

            // StackPanel essentially
            if (autoHeightsForAll)
            {
                availableSize = new SizeF(nfloat.MaxValue, availableSize.Secondary);
                foreach (var child in _arrangedSubviews)
                {
                    var childSize = child.Measure(availableSize);

                    if (childSize.Primary == 0)
                    {

                    }

                    consumed += childSize.Primary;

                    if (childSize.Secondary >= maxOtherDimension)
                    {
                        maxOtherDimension = childSize.Secondary;
                    }
                }

                return isVert ? new CGSize(maxOtherDimension, consumed) : new CGSize(consumed, maxOtherDimension);
            }

            // We measure autos FIRST, since those get priority
            foreach (var child in _arrangedSubviews.Where(i => i.Weight == 0))
            {
                var measured = child.Measure(new SizeF(MaxF(0f, availableSize.Primary - consumed), availableSize.Secondary));

                consumed += measured.Primary;
                maxOtherDimension = MaxF(maxOtherDimension, measured.Secondary);
            }

            double weightedAvailable = Math.Max(availableSize.Primary - consumed, 0);

            if (totalWeight > 0)
            {
                foreach (var child in _arrangedSubviews.Where(i => i.Weight != 0))
                {
                    var weight = child.Weight;
                    var childConsumed = (int)((weight / totalWeight) * weightedAvailable);

                    var measured = child.Measure(new SizeF(childConsumed, availableSize.Secondary));

                    consumed += measured.Primary;
                    maxOtherDimension = MaxF(maxOtherDimension, measured.Secondary);
                }
            }

            return isVert ? new CGSize(maxOtherDimension, consumed) : new CGSize(consumed, maxOtherDimension);
        }

        private static nfloat MaxF(nfloat f1, nfloat f2)
        {
            if (f1 > f2)
            {
                return f1;
            }
            return f2;
        }

        public override void LayoutSubviews()
        {
            var totalWeight = GetTotalWeight();
            bool isVert = Orientation == Vx.Views.Orientation.Vertical;
            var finalSize = isVert ? new SizeF(Frame.Height, Frame.Width) : new SizeF(Frame.Width, Frame.Height);

            nfloat pos = 0;

            nfloat consumedByAuto = 0;
            foreach (var child in _arrangedSubviews.Where(i => i.Weight == 0))
            {
                consumedByAuto += child.Measure(new SizeF(nfloat.MaxValue, finalSize.Secondary)).Primary;
            }

            double weightedAvailable = Math.Max(0, finalSize.Primary - consumedByAuto);

            foreach (var child in _arrangedSubviews)
            {
                var weight = child.Weight;

                nfloat consumed;
                if (weight == 0)
                {
                    consumed = child.DesiredSize.Primary;
                }
                else
                {
                    consumed = (int)((weight / totalWeight) * weightedAvailable);
                }

                child.Layout(pos, new SizeF(consumed, finalSize.Secondary));

                pos += consumed;
            }
        }

        private SizeF GetSize(CGSize size)
        {
            if (Orientation == Orientation.Vertical)
            {
                return new SizeF(size.Height, size.Width);
            }
            else
            {
                return new SizeF(size.Width, size.Height);
            }
        }

        private static nfloat GetSize(nfloat size)
        {
            return size == UIView.NoIntrinsicMetric ? 0 : size;
        }

        /// <summary>
        /// You must call this after modifying anything
        /// </summary>
        public void UpdateAllConstraints()
        {
            InvalidateIntrinsicContentSize();
            SetNeedsLayout();
            return;
        }

        //public override void UpdateConstraints()
        //{
        //    ArrangedSubview prev = null;
        //    ArrangedSubview curr = null;
        //    ArrangedSubview firstWeighted = null;

        //    bool usingWeights = _arrangedSubviews.Any(i => i.Weight > 0);

        //    for (int i = 0; i < _arrangedSubviews.Count; i++)
        //    {
        //        curr = _arrangedSubviews[i];
        //        ArrangedSubview next = _arrangedSubviews.ElementAtOrDefault(i + 1);

        //        curr.UpdateConstraints(prev, next, usingWeights, firstWeighted);

        //        if (firstWeighted == null && curr.Weight > 0)
        //        {
        //            firstWeighted = curr;
        //        }

        //        prev = curr;
        //    }

        //    // Need to call base when done
        //    base.UpdateConstraints();
        //}
    }
}