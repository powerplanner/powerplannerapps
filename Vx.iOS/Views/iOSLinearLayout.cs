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
            View.AllowOverflowAndClip = newView.AllowOverflowAndClip;

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
        public UILinearLayout()
        {
            ClipsToBounds = true;
        }

        private Orientation _orientation;
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (value != _orientation)
                {
                    _orientation = value;
                    InvalidateLayout();
                }
            }
        }

        private bool _allowOverflowAndClip;
        public bool AllowOverflowAndClip
        {
            get => _allowOverflowAndClip;
            set
            {
                if (value != _allowOverflowAndClip)
                {
                    _allowOverflowAndClip = value;
                    ClipsToBounds = !value;
                    InvalidateLayout();
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

        private bool IsVertical => Orientation == Orientation.Vertical;

        private void InvalidateLayout()
        {
            MarkMeasureDirty();
            SetNeedsLayout();
            PropagateLayoutDirty(Superview);
        }

        private static bool IsBounded(nfloat value)
        {
            return value > 0 && value < UIViewWrapper.UnboundedSize;
        }

        /// <summary>
        /// Runs the weighted measure pass and returns each child's main-axis content extent
        /// (excluding its margin), plus the panel's desired main and cross extents.
        /// </summary>
        private nfloat[] ComputeLayout(CGSize available, out nfloat desiredMain, out nfloat desiredCross)
        {
            var children = ArrangedSubviews;
            int n = children.Count;
            var mainExtents = new nfloat[n];

            bool vertical = IsVertical;
            nfloat availCross = vertical ? available.Width : available.Height;
            nfloat availMain = vertical ? available.Height : available.Width;
            bool crossBounded = IsBounded(availCross);
            bool mainBounded = IsBounded(availMain);

            nfloat sumNonWeighted = 0;
            float totalWeight = 0;
            nfloat weightedMarginMain = 0;
            nfloat maxCross = 0;

            // Pass 1: measure every non-weighted child with the main axis unbounded.
            for (int i = 0; i < n; i++)
            {
                var child = children[i];
                float weight = GetWeight(child);
                nfloat marginCross = vertical ? (child.Margin.Left + child.Margin.Right) : (child.Margin.Top + child.Margin.Bottom);
                nfloat marginMain = vertical ? (child.Margin.Top + child.Margin.Bottom) : (child.Margin.Left + child.Margin.Right);

                if (weight > 0)
                {
                    totalWeight += weight;
                    // Weighted children still occupy their margin, so reserve it before
                    // dividing the remaining space into weighted shares.
                    weightedMarginMain += marginMain;
                    continue;
                }

                nfloat crossAvail = crossBounded ? MaxF(0, availCross - marginCross) : UIViewWrapper.UnboundedSize;
                CGSize m = child.Measure(vertical
                    ? new CGSize(crossAvail, UIViewWrapper.UnboundedSize)
                    : new CGSize(UIViewWrapper.UnboundedSize, crossAvail));

                nfloat mainExtent = vertical ? m.Height : m.Width;
                mainExtents[i] = mainExtent;
                sumNonWeighted += mainExtent + marginMain;
                maxCross = MaxF(maxCross, (vertical ? m.Width : m.Height) + marginCross);
            }

            // Pass 2: distribute the remaining main-axis space across weighted children.
            nfloat remaining = mainBounded ? MaxF(0, availMain - sumNonWeighted - weightedMarginMain) : UIViewWrapper.UnboundedSize;
            bool distributeWeights = mainBounded && totalWeight > 0;
            nfloat sumWeightedMain = 0;

            for (int i = 0; i < n; i++)
            {
                var child = children[i];
                float weight = GetWeight(child);
                if (weight <= 0)
                {
                    continue;
                }

                nfloat marginCross = vertical ? (child.Margin.Left + child.Margin.Right) : (child.Margin.Top + child.Margin.Bottom);
                nfloat marginMain = vertical ? (child.Margin.Top + child.Margin.Bottom) : (child.Margin.Left + child.Margin.Right);

                nfloat crossAvail = crossBounded ? MaxF(0, availCross - marginCross) : UIViewWrapper.UnboundedSize;
                nfloat share = distributeWeights ? remaining * (weight / totalWeight) : UIViewWrapper.UnboundedSize;

                CGSize m = child.Measure(vertical
                    ? new CGSize(crossAvail, share)
                    : new CGSize(share, crossAvail));

                // When bounded, the weighted child occupies exactly its share; otherwise (e.g.
                // inside a scroll view) it collapses to its measured content size, like Android.
                nfloat mainExtent = distributeWeights ? share : (vertical ? m.Height : m.Width);
                mainExtents[i] = mainExtent;
                sumWeightedMain += mainExtent + marginMain;
                maxCross = MaxF(maxCross, (vertical ? m.Width : m.Height) + marginCross);
            }

            desiredMain = sumNonWeighted + sumWeightedMain;
            desiredCross = maxCross;
            return mainExtents;
        }

        public override CGSize MeasureContent(CGSize available)
        {
            if (ArrangedSubviews.Count == 0)
            {
                return CGSize.Empty;
            }

            ComputeLayout(available, out nfloat desiredMain, out nfloat desiredCross);

            return IsVertical
                ? new CGSize(desiredCross, desiredMain)
                : new CGSize(desiredMain, desiredCross);
        }

        public override void ArrangeContent(CGSize size)
        {
            var children = ArrangedSubviews;
            if (children.Count == 0)
            {
                return;
            }

            bool vertical = IsVertical;
            var mainExtents = ComputeLayout(size, out _, out _);

            nfloat pos = 0;
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                nfloat marginMain = vertical ? (child.Margin.Top + child.Margin.Bottom) : (child.Margin.Left + child.Margin.Right);
                nfloat slotMain = mainExtents[i] + marginMain;

                CGRect slot = vertical
                    ? new CGRect(0, pos, size.Width, slotMain)
                    : new CGRect(pos, 0, slotMain, size.Height);

                child.Arrange(slot);
                pos += slotMain;
            }
        }
    }
}