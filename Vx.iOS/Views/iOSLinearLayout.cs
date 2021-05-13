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

            bool changed = false;

            if (View.Orientation != newView.Orientation)
            {
                View.Orientation = newView.Orientation;
                changed = true;
            }

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
                    View.InsertSubview(childView, i);

                    changed = true;
                },
                clear: () =>
                {
                    View.ClearArrangedSubviews();

                    changed = true;
                }
                );


            for (int i = 0; i < newView.Children.Count; i++)
            {
                changed = View.SetWeight(i, LinearLayout.GetWeight(newView.Children[i])) || changed;
                changed = View.SetMargins(i, newView.Children[i].Margin.AsModified()) || changed;
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

        public bool SetWeight(int index, float weight)
        {
            if (_arrangedSubviews[index].Weight != weight)
            {
                _arrangedSubviews[index].Weight = weight;
                return true;
            }

            return false;
        }

        public bool SetMargins(int index, Thickness margins)
        {
            if (_arrangedSubviews[index].Margin != margins)
            {
                _arrangedSubviews[index].Margin = margins;
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
            _arrangedSubviews[index].RemoveAllConstraints();
            _arrangedSubviews.RemoveAt(index);
            Subviews[index].RemoveFromSuperview();
        }

        public void InsertArrangedSubview(UIView subview, int index)
        {
            InsertArrangedSubview(subview, index, 0);
        }

        private class ArrangedSubview
        {
            public UILinearLayout Parent { get; set; }
            public UIView Subview { get; set; }
            public Thickness Margin { get; set; }
            public float Weight { get; set; }

            private NSLayoutConstraint _topConstraint;
            public NSLayoutConstraint TopConstraint
            {
                get => _topConstraint;
                set => SetConstraint(ref _topConstraint, value);
            }

            private NSLayoutConstraint _rightConstraint;
            public NSLayoutConstraint RightConstraint
            {
                get => _rightConstraint;
                set => SetConstraint(ref _rightConstraint, value);
            }

            private NSLayoutConstraint _leftConstraint;
            public NSLayoutConstraint LeftConstraint
            {
                get => _leftConstraint;
                set => SetConstraint(ref _leftConstraint, value);
            }

            private NSLayoutConstraint _bottomConstraint;
            public NSLayoutConstraint BottomConstraint
            {
                get => _bottomConstraint;
                set => SetConstraint(ref _bottomConstraint, value);
            }

            private NSLayoutConstraint _weightConstraint;
            public NSLayoutConstraint WeightConstraint
            {
                get => _weightConstraint;
                set => SetConstraint(ref _weightConstraint, value);
            }

            private void SetConstraint(ref NSLayoutConstraint storage, NSLayoutConstraint value)
            {
                // If removing
                if (storage != null && value == null)
                {
                    // Simply remove
                    Parent.RemoveConstraint(storage);
                    storage = null;
                    return;
                }

                if (storage != null)
                {
                    // If identical
                    if (object.ReferenceEquals(storage.FirstItem, value.FirstItem)
                        && storage.FirstAttribute == value.FirstAttribute
                        && storage.Relation == value.Relation
                        && object.ReferenceEquals(storage.SecondItem, value.SecondItem)
                        && storage.SecondAttribute == value.SecondAttribute
                        && storage.Multiplier == value.Multiplier
                        && storage.Constant == value.Constant)
                    {
                        // No need to update
                        return;
                    }

                    // Otherwise we need to remove existing constraint to update it
                    Parent.RemoveConstraint(storage);
                }

                if (value != null)
                {
                    Parent.AddConstraint(value);
                }
            }

            public void RemoveAllConstraints()
            {
                TopConstraint = null;
                LeftConstraint = null;
                RightConstraint = null;
                BottomConstraint = null;
                WeightConstraint = null;
            }

            public ArrangedSubview(UILinearLayout parent, UIView subview, Thickness margin, float weight)
            {
                Parent = parent;
                Subview = subview;
                Margin = margin;
                Weight = weight;
            }

            private bool IsVertical => Parent.Orientation == Orientation.Vertical;

            public void UpdateConstraints(ArrangedSubview prev, ArrangedSubview next, bool usingWeights, ArrangedSubview firstWeighted)
            {
                if (IsVertical)
                {
                    if (prev == null)
                    {
                        TopConstraint = NSLayoutConstraint.Create(
                            Subview,
                            NSLayoutAttribute.Top,
                            NSLayoutRelation.Equal,
                            Parent,
                            NSLayoutAttribute.Top,
                            multiplier: 1,
                            constant: Margin.Top);
                    }
                    else
                    {
                        TopConstraint = NSLayoutConstraint.Create(
                            Subview,
                            NSLayoutAttribute.Top,
                            NSLayoutRelation.Equal,
                            prev.Subview,
                            NSLayoutAttribute.Bottom,
                            multiplier: 1,
                            constant: prev.Margin.Bottom + Margin.Top);
                    }

                    if (next == null)
                    {
                        AssignFinalEndingConstraint(usingWeights);
                    }
                    else
                    {
                        BottomConstraint = null;
                    }

                    LeftConstraint = NSLayoutConstraint.Create(
                        Subview,
                        NSLayoutAttribute.Left,
                        NSLayoutRelation.Equal,
                        Parent,
                        NSLayoutAttribute.Left,
                        multiplier: 1,
                        constant: Margin.Left);

                    RightConstraint = NSLayoutConstraint.Create(
                        Subview,
                        NSLayoutAttribute.Right,
                        NSLayoutRelation.Equal,
                        Parent,
                        NSLayoutAttribute.Right,
                        multiplier: 1,
                        constant: Margin.Right);
                }
                else
                {
                    if (prev == null)
                    {
                        LeftConstraint = NSLayoutConstraint.Create(
                            Subview,
                            NSLayoutAttribute.Left,
                            NSLayoutRelation.Equal,
                            Parent,
                            NSLayoutAttribute.Left,
                            multiplier: 1,
                            constant: Margin.Left);
                    }
                    else
                    {
                        LeftConstraint = NSLayoutConstraint.Create(
                            Subview,
                            NSLayoutAttribute.Left,
                            NSLayoutRelation.Equal,
                            prev.Subview,
                            NSLayoutAttribute.Right,
                            multiplier: 1,
                            constant: prev.Margin.Right + Margin.Left);
                    }

                    if (next == null)
                    {
                        AssignFinalEndingConstraint(usingWeights);
                    }
                    else
                    {
                        RightConstraint = null;
                    }

                    TopConstraint = NSLayoutConstraint.Create(
                        Subview,
                        NSLayoutAttribute.Top,
                        NSLayoutRelation.Equal,
                        Parent,
                        NSLayoutAttribute.Top,
                        multiplier: 1,
                        constant: Margin.Top);

                    BottomConstraint = NSLayoutConstraint.Create(
                        Subview,
                        NSLayoutAttribute.Bottom,
                        NSLayoutRelation.Equal,
                        Parent,
                        NSLayoutAttribute.Bottom,
                        multiplier: 1,
                        constant: Margin.Bottom);
                }

                if (usingWeights && firstWeighted != null && Weight > 0)
                {
                    WeightConstraint = NSLayoutConstraint.Create(
                        Subview,
                        IsVertical ? NSLayoutAttribute.Height : NSLayoutAttribute.Width,
                        NSLayoutRelation.Equal,
                        firstWeighted.Subview,
                        IsVertical ? NSLayoutAttribute.Height : NSLayoutAttribute.Width,
                        multiplier: Weight / firstWeighted.Weight,
                        constant: 0);
                }
                else
                {
                    WeightConstraint = null;
                }
            }

            public void AssignFinalEndingConstraint(bool usingWeights)
            {
                if (IsVertical)
                {
                    BottomConstraint = NSLayoutConstraint.Create(
                        Parent,
                        NSLayoutAttribute.Bottom,
                        usingWeights ? NSLayoutRelation.Equal : NSLayoutRelation.GreaterThanOrEqual,
                        Subview,
                        NSLayoutAttribute.Bottom,
                        multiplier: 1,
                        constant: Margin.Bottom);
                }
                else
                {
                    RightConstraint = NSLayoutConstraint.Create(
                        Parent,
                        NSLayoutAttribute.Right,
                        usingWeights ? NSLayoutRelation.Equal : NSLayoutRelation.GreaterThanOrEqual,
                        Subview,
                        NSLayoutAttribute.Right,
                        multiplier: 1,
                        constant: Margin.Right);
                }
            }
        }

        private List<ArrangedSubview> _arrangedSubviews = new List<ArrangedSubview>();

        public void InsertArrangedSubview(UIView subview, int index, float weight)
        {
            subview.TranslatesAutoresizingMaskIntoConstraints = false;
            InsertSubview(subview, index);

            ArrangedSubview curr = new ArrangedSubview(this, subview, new Thickness(), weight);

            _arrangedSubviews.Insert(index, curr);
        }

        /// <summary>
        /// You must call this after modifying anything
        /// </summary>
        public void UpdateAllConstraints()
        {
            ArrangedSubview prev = null;
            ArrangedSubview curr = null;
            ArrangedSubview firstWeighted = null;

            bool usingWeights = _arrangedSubviews.Any(i => i.Weight > 0);

            for (int i = 0; i < _arrangedSubviews.Count; i++)
            {
                curr = _arrangedSubviews[i];
                ArrangedSubview next = _arrangedSubviews.ElementAtOrDefault(i + 1);

                curr.UpdateConstraints(prev, next, usingWeights, firstWeighted);

                if (firstWeighted == null && curr.Weight > 0)
                {
                    firstWeighted = curr;
                }

                prev = curr;
            }
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