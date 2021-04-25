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
     * */
    public class iOSLinearLayout : iOSView<LinearLayout, UILinearLayout>
    {
        protected override void ApplyProperties(LinearLayout oldView, LinearLayout newView)
        {
            base.ApplyProperties(oldView, newView);

            // Temp background color for debugging purposes
            //View.BackgroundColor = UIColor.FromRGBA(0, 0, 255, 15);

            View.Orientation = newView.Orientation;

            bool hasChildren = View.Subviews.Length > 0;

            ReconcileList(
                oldView?.Children,
                newView.Children,
                insert: (i, v) =>
                {
                    var childView = v.CreateUIView(VxParentView);
                    View.InsertArrangedSubview(childView, i);
                },
                remove: (i) => View.RemoveArrangedSubview(i),
                replace: (i, v) =>
                {
                    View.RemoveArrangedSubview(i);

                    var childView = v.CreateUIView(VxParentView);
                    View.InsertSubview(childView, i);
                },
                clear: () =>
                {
                    View.ClearArrangedSubviews();
                }
                );


            for (int i = 0; i < newView.Children.Count; i++)
            {
                View.SetWeight(i, LinearLayout.GetWeight(newView.Children[i]));
                View.SetMargins(i, newView.Children[i].Margin);
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
                    SetNeedsLayout();
                    InvalidateIntrinsicContentSize();
                }
            }
        }

        private List<float> _weights = new List<float>();
        private List<Thickness> _margins = new List<Thickness>();

        public void SetWeight(int index, float weight)
        {
            if (_weights[index] != weight)
            {
                _weights[index] = weight;
                SetNeedsLayout();
                InvalidateIntrinsicContentSize();
            }
        }

        public void SetMargins(int index, Thickness margins)
        {
            if (_margins[index] != margins)
            {
                _margins[index] = margins;
                SetNeedsLayout();
                InvalidateIntrinsicContentSize();
            }
        }

        public void ClearArrangedSubviews()
        {
            _weights.Clear();
            _margins.Clear();
            foreach (var subview in Subviews)
            {
                subview.RemoveFromSuperview();
                InvalidateIntrinsicContentSize();
            }
        }

        public void RemoveArrangedSubview(int index)
        {
            _weights.RemoveAt(index);
            _margins.RemoveAt(index);
            Subviews[index].RemoveFromSuperview();
            InvalidateIntrinsicContentSize();
        }

        public void InsertArrangedSubview(UIView subview, int index)
        {
            InsertArrangedSubview(subview, index, 0);
        }

        public void InsertArrangedSubview(UIView subview, int index, float weight)
        {
            if (subview.TranslatesAutoresizingMaskIntoConstraints == false)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif

                throw new InvalidOperationException("TranslatesAutoresizingMaskIntoConstraints cannot be false when view is used inside UILinearLayout.");
            }

            InsertSubview(subview, index);
            _weights.Insert(index, weight);
            _margins.Insert(index, new Thickness());
            InvalidateIntrinsicContentSize();
        }

        public override CGSize IntrinsicContentSize
        {
            get
            {
                var sizes = CalculateSizes(new CGSize(double.MaxValue, double.MaxValue));

                if (Orientation == Orientation.Vertical)
                {
                    return new CGSize(sizes.Max(i => i.TotalSize.Width), sizes.Sum(i => i.TotalSize.Height));
                }
                else
                {
                    return new CGSize(sizes.Sum(i => i.TotalSize.Width), sizes.Max(i => i.TotalSize.Height));
                }
            }
        }

        public override CoreGraphics.CGSize SystemLayoutSizeFittingSize(CoreGraphics.CGSize size)
        {
            var sizes = CalculateSizes(size);

            if (Orientation == Orientation.Vertical)
            {
                return new CGSize(sizes.Max(i => i.TotalSize.Width), sizes.Sum(i => i.TotalSize.Height));
            }
            else
            {
                return new CGSize(sizes.Sum(i => i.TotalSize.Width), sizes.Max(i => i.TotalSize.Height));
            }
        }

        private class Calculated
        {
            public CGSize ViewSize { get; set; }

            public CGSize TotalSize { get; set; }

            public Thickness Margins { get; set; }

            public Calculated(CGSize viewSize, Thickness margins)
            {
                ViewSize = viewSize;
                Margins = margins;
                TotalSize = new CGSize(viewSize.Width + margins.Width, viewSize.Height + margins.Height);
            }
        }

        private Calculated[] CalculateSizes(CGSize frame)
        {
            var sizes = new Calculated[Subviews.Length];

            bool isVertical = Orientation == Orientation.Vertical;

            double remainingSpace = isVertical ? frame.Height : frame.Width;
            double totalWeight = _weights.Sum();

            // First do auto weights
            for (int i = 0; i < Subviews.Length; i++)
            {
                var subview = Subviews[i];
                var weight = _weights[i];
                var margins = _margins[i];

                if (weight == 0)
                {
                    var size = subview.SystemLayoutSizeFittingSize(isVertical ? new CGSize(frame.Width - margins.Width, remainingSpace - margins.Height) : new CGSize(remainingSpace - margins.Width, frame.Height - margins.Height));
                    var calculated = new Calculated(size, margins);
                    sizes[i] = calculated;

                    remainingSpace = Math.Max(0, remainingSpace - (isVertical ? calculated.TotalSize.Height : calculated.TotalSize.Width));
                }
            }

            // Then can arrange remaining
            for (int i = 0; i < Subviews.Length; i++)
            {
                var subview = Subviews[i];
                var weight = _weights[i];
                var margins = _margins[i];

                if (weight != 0)
                {
                    var space = remainingSpace * (weight / totalWeight);
                    var minSize = subview.SystemLayoutSizeFittingSize(isVertical ? new CGSize(frame.Width - margins.Width, space - margins.Height) : new CGSize(space - margins.Width, frame.Height - margins.Height));
                    sizes[i] = new Calculated(isVertical ? new CGSize(minSize.Width, space) : new CGSize(space, minSize.Height), margins);
                }
            }

            return sizes;
        }

        public override void LayoutSubviews()
        {
            bool isVertical = Orientation == Orientation.Vertical;
            var sizes = CalculateSizes(Frame.Size);

            double pos = 0;
            for (int i = 0; i < Subviews.Length; i++)
            {
                var subview = Subviews[i];
                var size = sizes[i];
                var margins = size.Margins;

                if (isVertical)
                {
                    subview.Frame = new CGRect(margins.Left, pos + margins.Top, Frame.Width - margins.Width, size.ViewSize.Height);
                    pos += size.TotalSize.Height;
                }
                else
                {
                    subview.Frame = new CGRect(pos + margins.Left, margins.Top, size.ViewSize.Width, Frame.Height - margins.Width);
                    pos += size.TotalSize.Width;
                }
            }
        }
    }
}