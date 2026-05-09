using System;
using System.Collections.Generic;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    internal class iOSAdaptiveGridPanel : iOSView<AdaptiveGridPanel, UIAdaptiveGridPanel>
    {
        protected override void ApplyProperties(AdaptiveGridPanel oldView, AdaptiveGridPanel newView)
        {
            View.HoldOffApplyingChanges();

            base.ApplyProperties(oldView, newView);

            View.MinColumnWidth = newView.MinColumnWidth;
            View.ColumnSpacing = newView.ColumnSpacing;

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

    internal class UIAdaptiveGridPanel : UIPanel
    {
        private float _minColumnWidth = 250;
        public float MinColumnWidth
        {
            get => _minColumnWidth;
            set
            {
                if (_minColumnWidth != value)
                {
                    _minColumnWidth = value;
                    SetNeedsUpdateConstraints();
                }
            }
        }

        private float _columnSpacing = 24;
        public float ColumnSpacing
        {
            get => _columnSpacing;
            set
            {
                if (_columnSpacing != value)
                {
                    _columnSpacing = value;
                    SetNeedsUpdateConstraints();
                }
            }
        }

        private int GetNumberOfColumns(nfloat width, int childCount)
        {
            if (childCount <= 1)
                return 1;

            int cols = (int)((width - _columnSpacing) / (_minColumnWidth + _columnSpacing));
            if (cols <= 0)
                cols = 1;
            if (cols > childCount)
                cols = childCount;
            return cols;
        }

        private nfloat GetColumnWidth(nfloat totalWidth, int numColumns)
        {
            if (numColumns <= 1)
                return totalWidth;
            return (totalWidth - _columnSpacing * (numColumns - 1)) / numColumns;
        }

        private List<NSLayoutConstraint> _extraConstraints = new List<NSLayoutConstraint>();
        private int _lastNumColumns = -1;
        private nfloat _lastWidth = -1;
        private int _lastChildCount = -1;

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var width = Frame.Size.Width;
            if (width == 0)
                return;

            int childCount = ArrangedSubviews.Count;
            int newNumColumns = GetNumberOfColumns(width, childCount);
            if (newNumColumns != _lastNumColumns || width != _lastWidth || childCount != _lastChildCount)
            {
                SetNeedsUpdateConstraints();
            }
        }

        public override void ArrangeSubviews()
        {
            // Remove old extra constraints
            foreach (var c in _extraConstraints)
                RemoveConstraint(c);
            _extraConstraints.Clear();

            var children = ArrangedSubviews;
            int childCount = children.Count;

            if (childCount == 0)
                return;

            var width = Frame.Size.Width;
            if (width == 0)
                return;

            // Stretch if only one child
            if (childCount == 1)
            {
                var child = children[0];
                child.SetConstraints(
                    leftConstraint: new WrapperConstraint(this, NSLayoutAttribute.Left),
                    topConstraint: new WrapperConstraint(this, NSLayoutAttribute.Top),
                    rightConstraint: new WrapperConstraint(this, NSLayoutAttribute.Right),
                    bottomConstraint: new WrapperConstraint(this, NSLayoutAttribute.Bottom) { GreaterThanOrEqual = true },
                    centeringHorizontalView: null,
                    centeringVerticalView: null);
                _lastNumColumns = 1;
                _lastWidth = width;
                _lastChildCount = childCount;
                return;
            }

            int numColumns = GetNumberOfColumns(width, childCount);
            nfloat columnWidth = GetColumnWidth(width, numColumns);
            int numRows = (childCount + numColumns - 1) / numColumns;
            _lastNumColumns = numColumns;
            _lastWidth = width;
            _lastChildCount = childCount;

            for (int i = 0; i < childCount; i++)
            {
                int row = i / numColumns;
                int col = i % numColumns;
                var child = children[i];

                // LEFT constraint: first column pins to panel, others chain to previous item
                WrapperConstraint leftConstraint;
                if (col == 0)
                {
                    leftConstraint = new WrapperConstraint(this, NSLayoutAttribute.Left);
                }
                else
                {
                    var prev = children[i - 1];
                    leftConstraint = new WrapperConstraint(
                        prev.View,
                        NSLayoutAttribute.Right,
                        1,
                        prev.Margin.Right + _columnSpacing);
                }

                // TOP constraint: first row pins to panel top, subsequent rows use manual constraints
                WrapperConstraint? topConstraint = null;
                if (row == 0)
                {
                    topConstraint = new WrapperConstraint(this, NSLayoutAttribute.Top);
                }

                // BOTTOM constraint: last row items connect to panel bottom
                WrapperConstraint? bottomConstraint = null;
                if (row == numRows - 1)
                {
                    bottomConstraint = new WrapperConstraint(this, NSLayoutAttribute.Bottom) { GreaterThanOrEqual = true };
                }

                // WIDTH constraint
                var widthConstraint = new WrapperConstraint(null, NSLayoutAttribute.Width, 1, columnWidth - child.Margin.Width);

                child.SetConstraints(
                    leftConstraint: leftConstraint,
                    topConstraint: topConstraint,
                    rightConstraint: null,
                    bottomConstraint: bottomConstraint,
                    centeringHorizontalView: null,
                    centeringVerticalView: null,
                    widthConstraint: widthConstraint);

                child.View.SetContentHuggingPriority(1000, UILayoutConstraintAxis.Vertical);

                // For rows after the first: add top >= bottom constraints for ALL items
                // in the previous row, ensuring this item starts below the entire row
                if (row > 0)
                {
                    int prevRowStart = (row - 1) * numColumns;
                    int prevRowEnd = Math.Min(prevRowStart + numColumns, childCount);
                    for (int j = prevRowStart; j < prevRowEnd; j++)
                    {
                        var above = children[j];
                        var tc = NSLayoutConstraint.Create(
                            child.View,
                            NSLayoutAttribute.Top,
                            NSLayoutRelation.GreaterThanOrEqual,
                            above.View,
                            NSLayoutAttribute.Bottom,
                            1,
                            above.Margin.Bottom + child.Margin.Top);
                        AddConstraint(tc);
                        _extraConstraints.Add(tc);
                    }

                    // Low-priority pull-up constraint to resolve ambiguity:
                    // items should sit as high as possible while respecting the
                    // GreaterThanOrEqual constraints above
                    var pullUp = NSLayoutConstraint.Create(
                        child.View,
                        NSLayoutAttribute.Top,
                        NSLayoutRelation.Equal,
                        this,
                        NSLayoutAttribute.Top,
                        1,
                        child.Margin.Top);
                    pullUp.Priority = 1;
                    AddConstraint(pullUp);
                    _extraConstraints.Add(pullUp);
                }
            }
        }
    }
}
