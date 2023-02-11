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
            View.HoldOffApplyingChanges();

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

            View.ApplyAnyHeldChanges();
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
                    SetNeedsUpdateConstraints();
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

        public override void ArrangeSubviews()
        {
            bool usingWeights = ArrangedSubviews.Any(i => GetWeight(i) > 0);

            UIViewWrapper prev = null;
            UIViewWrapper curr = null;
            UIViewWrapper firstWeighted = null;

            for (int i = 0; i < ArrangedSubviews.Count; i++)
            {
                curr = ArrangedSubviews[i];
                UIViewWrapper next = ArrangedSubviews.ElementAtOrDefault(i + 1);

                UpdateConstraints(curr, prev, next, usingWeights, firstWeighted);

                if (firstWeighted == null && GetWeight(curr) > 0)
                {
                    firstWeighted = curr;
                }

                prev = curr;
            }
        }

#if DEBUG
        private string DebugDumpConstraints()
        {
            string str = "";
            for (int i = 0; i < ArrangedSubviews.Count; i++)
            {
                var curr = ArrangedSubviews[i];

                str += "ITEM " + i + ": " + curr.DebugConstraintsString + "\n\n";
            }
            return str;
        }
#endif

        private bool IsVertical => Orientation == Orientation.Vertical;

        private void UpdateConstraints(UIViewWrapper curr, UIViewWrapper prev, UIViewWrapper next, bool usingWeights, UIViewWrapper firstWeighted)
        {
            var weight = GetWeight(curr);

            // https://betterprogramming.pub/what-are-content-hugging-and-compression-resistance-in-swift-60275f6dc69e
            // ContentHugging: Defaults to 250. Higher value means hug the view (don't expand unnecessarily beyond intrinsic size).
            // CompressionResistance: Defaults to 750. Higher value means resist shrinking beyond the view's intrinsic size.
            curr.View.SetContentHuggingPriority(weight == 0 ? 1000 : 1, IsVertical ? UILayoutConstraintAxis.Vertical : UILayoutConstraintAxis.Horizontal);
            curr.View.SetContentHuggingPriority(250, IsVertical ? UILayoutConstraintAxis.Horizontal : UILayoutConstraintAxis.Vertical); // Reset other direction to default
            //curr.View.SetContentCompressionResistancePriority(weight == 0 ? 0 : 1000, IsVertical ? UILayoutConstraintAxis.Vertical : UILayoutConstraintAxis.Horizontal);
            //curr.View.SetContentCompressionResistancePriority(750, IsVertical ? UILayoutConstraintAxis.Horizontal : UILayoutConstraintAxis.Vertical); // Reset other direction to default

            WrapperConstraint? leftConstraint, topConstraint, rightConstraint, bottomConstraint, widthConstraint = null, heightConstraint = null;

            if (IsVertical)
            {
                if (prev == null)
                {
                    topConstraint = new WrapperConstraint(
                        this,
                        NSLayoutAttribute.Top);
                }
                else
                {
                    topConstraint = new WrapperConstraint(
                        prev.View,
                        NSLayoutAttribute.Bottom,
                        1,
                        prev.Margin.Bottom);
                }

                if (next == null)
                {
                    bottomConstraint = new WrapperConstraint(
                        this,
                        NSLayoutAttribute.Bottom)
                    {
                        GreaterThanOrEqual = !usingWeights
                    };
                }
                else
                {
                    bottomConstraint = null;
                }

                leftConstraint = new WrapperConstraint(
                    this,
                    NSLayoutAttribute.Left);

                rightConstraint = new WrapperConstraint(
                    this,
                    NSLayoutAttribute.Right);
            }
            else // Horizontal
            {
                if (curr.View is UILabel label && label.Lines == 0 && weight == 0 && float.IsNaN(curr.Width))
                {
                    throw new NotImplementedException("Cannot have multiline text blocks within a horizontal linear layout unless it has a fixed width or weight. Disable text wrapping on your TextBlock.");
                }
                if (prev == null)
                {
                    leftConstraint = new WrapperConstraint(
                        this,
                        NSLayoutAttribute.Left);
                }
                else
                {
                    leftConstraint = new WrapperConstraint(
                        prev.View,
                        NSLayoutAttribute.Right,
                        1,
                        prev.Margin.Right);
                }

                if (next == null)
                {
                    rightConstraint = new WrapperConstraint(
                        this,
                        NSLayoutAttribute.Right)
                    {
                        GreaterThanOrEqual = !usingWeights
                    };
                }
                else
                {
                    rightConstraint = null;
                }

                topConstraint = new WrapperConstraint(
                    this,
                    NSLayoutAttribute.Top);

                bottomConstraint = new WrapperConstraint(
                    this,
                    NSLayoutAttribute.Bottom);
            }

            if (usingWeights && firstWeighted != null && weight > 0)
            {
                if (IsVertical)
                {
                    widthConstraint = null;
                    heightConstraint = new WrapperConstraint(
                        firstWeighted.View,
                        NSLayoutAttribute.Height,
                        weight / GetWeight(firstWeighted),
                        0);
                }
                else
                {
                    heightConstraint = null;
                    widthConstraint = new WrapperConstraint(
                        firstWeighted.View,
                        NSLayoutAttribute.Width,
                        weight / GetWeight(firstWeighted),
                        0);
                }
            }

            UIView centeringHorizontalView = IsVertical ? this : null;
            UIView centeringVerticalView = IsVertical ? null : this;

            curr.SetConstraints(leftConstraint, topConstraint, rightConstraint, bottomConstraint, centeringHorizontalView, centeringVerticalView, widthConstraint, heightConstraint, centerViaLayoutGuideIfNeeded: weight > 0 && IsVertical);
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