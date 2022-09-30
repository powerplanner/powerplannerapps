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
     * DEFINITIVE OVERRIDING LAYOUTSUBVIEWS GUIDE...
     * 
     * 1. How does SystemLayoutSizeFittingSize / SizeThatFits work?
     *      
     *      - It returns the BEST SIZE for a given size (does not return min size).
     *      - If you pass in a width or height of 0, that's equivalent to "Auto height" or "Auto width" (and 0,0 is "Min size in general")
     *      
     * 2. What does a custom control need to implement?
     * 
     *      - Override IntrinsicContentSize
     *      - Override SizeThatFits
     *      - Override LayoutSubviews
     *      
     * 3. What should a custom panel call when measuring size of children?
     * 
     *      - Call SystemLayoutSizeFittingSize(), if you want smallest size, pass in 0 for width and/or heights
     * */

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

            View.Orientation = newView.Orientation;

            ReconcileList(
                oldView?.Children,
                newView.Children,
                insert: (i, v) =>
                {
                    var childView = v.CreateUIView(VxParentView);
                    View.InsertArrangedSubview(childView, i);
                },
                remove: (i) =>
                {
                    View.RemoveArrangedSubviewAt(i);
                },
                replace: (i, v) =>
                {
                    View.RemoveArrangedSubviewAt(i);

                    var childView = v.CreateUIView(VxParentView);
                    View.InsertArrangedSubview(childView, i);
                },
                clear: () =>
                {
                    View.ClearArrangedSubviews();
                }
                );
        }
    }

    public class UILinearLayout : UIPanel
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
                    InvalidateIntrinsicContentSize();
                    SetNeedsLayout();
                }
            }
        }

        private const string WEIGHT = "Weight";

        public static void SetWeight(UIViewWrapper view, float weight)
        {
            view.SetValue(WEIGHT, weight);
        }

        public static float GetWeight(UIViewWrapper view)
        {
            return view.GetValueOrDefault<float>(WEIGHT, 0);
        }

        private SizeF ChildSizeThatFits(UIViewWrapper view, SizeF size)
        {
            return ToRelativeSize(view.SizeThatFits(ToAbsoluteSize(size)));
        }

        private CGSize ToAbsoluteSize(SizeF size)
        {
            return Orientation == Orientation.Horizontal ? new CGSize(size.Primary, size.Secondary) : new CGSize(size.Secondary, size.Primary);
        }

        private SizeF ToRelativeSize(CGSize size)
        {
            return Orientation == Orientation.Horizontal ? new SizeF(size.Width, size.Height) : new SizeF(size.Height, size.Width);
        }

        private void SetChildFrame(UIViewWrapper view, nfloat pos, SizeF size)
        {
            if (Orientation == Orientation.Horizontal)
            {
                view.Frame = new CGRect(new CGPoint(pos, 0), ToAbsoluteSize(size));
            }
            else
            {
                view.Frame = new CGRect(new CGPoint(0, pos), ToAbsoluteSize(size));
            }
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

        private float GetTotalWeight()
        {
            return ArrangedSubviews.Sum(i => GetWeight(i));
        }

        // TODO: Almost fully working except the Scroll Viewer test page, height of LinearLayout isn't being calculated correctly, maybe it's issue with IntrinsicContentSize?
        // I wonder what Apple's arranged views do for IntrinsicContentSize

        public override CGSize SizeThatFits(CGSize size)
        {
            bool isVert = Orientation == Vx.Views.Orientation.Vertical;
            var availableSize = ToRelativeSize(size);

            nfloat consumed = 0;
            nfloat maxOtherDimension = 0;
            float totalWeight = GetTotalWeight();

            bool autoHeightsForAll = totalWeight == 0 || availableSize.Primary == 0;

            // StackPanel essentially
            if (autoHeightsForAll)
            {
                availableSize = new SizeF(0, availableSize.Secondary);
                foreach (var child in ArrangedSubviews)
                {
                    var childSize = ChildSizeThatFits(child, availableSize);

                    consumed += childSize.Primary;
                    maxOtherDimension = MaxF(maxOtherDimension, childSize.Secondary);
                }

                return ToAbsoluteSize(new SizeF(consumed, maxOtherDimension));
            }

            // We measure autos FIRST, since those get priority
            foreach (var child in ArrangedSubviews.Where(i => GetWeight(i) == 0))
            {
                var childSize = ChildSizeThatFits(child, new SizeF(0, availableSize.Secondary));

                consumed += childSize.Primary;
                maxOtherDimension = MaxF(maxOtherDimension, childSize.Secondary);
            }

            double weightedAvailable = Math.Max(availableSize.Primary - consumed, 0);

            if (totalWeight > 0)
            {
                foreach (var child in ArrangedSubviews.Where(i => GetWeight(i) != 0))
                {
                    var weight = GetWeight(child);
                    var childConsumed = (int)((weight / totalWeight) * weightedAvailable);

                    var childSize = ChildSizeThatFits(child, new SizeF(childConsumed, availableSize.Secondary));

                    consumed += childConsumed;
                    maxOtherDimension = MaxF(maxOtherDimension, childSize.Secondary);
                }
            }

            return ToAbsoluteSize(new SizeF(consumed, maxOtherDimension));
        }

        public override void LayoutSubviews()
        {
            var totalWeight = GetTotalWeight();
            bool isVert = Orientation == Vx.Views.Orientation.Vertical;
            var finalSize = ToRelativeSize(Frame.Size);

            nfloat pos = 0;

            Dictionary<UIViewWrapper, nfloat> autoChildPrimaries = new Dictionary<UIViewWrapper, nfloat>();
            nfloat consumedByAuto = 0;
            foreach (var child in ArrangedSubviews.Where(i => GetWeight(i) == 0))
            {
                var childSize = ChildSizeThatFits(child, new SizeF(0, finalSize.Secondary));

                consumedByAuto += childSize.Primary;
                autoChildPrimaries[child] = childSize.Primary;
            }

            double weightedAvailable = Math.Max(0, finalSize.Primary - consumedByAuto);

            foreach (var child in ArrangedSubviews)
            {
                var weight = GetWeight(child);

                nfloat consumed;
                if (weight == 0)
                {
                    consumed = autoChildPrimaries[child];
                }
                else
                {
                    consumed = (int)((weight / totalWeight) * weightedAvailable);
                }

                SetChildFrame(child, pos, new SizeF(consumed, finalSize.Secondary));

                pos += consumed;
            }
        }
    }
}