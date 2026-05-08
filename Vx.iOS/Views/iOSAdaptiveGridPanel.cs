using CoreGraphics;
using System;
using System.Linq;
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

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            SetNeedsUpdateConstraints();
        }

        public override void ArrangeSubviews()
        {
            var width = Frame.Size.Width;
            if (width == 0)
                return;

            var children = ArrangedSubviews;
            int childCount = children.Count;

            if (childCount == 0)
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
                return;
            }

            int numColumns = GetNumberOfColumns(width, childCount);
            nfloat columnWidth = GetColumnWidth(width, numColumns);

            nfloat y = 0;
            int i = 0;

            while (i < childCount)
            {
                // Determine row height by measuring
                nfloat rowHeight = 0;
                for (int col = 0; col < numColumns && i + col < childCount; col++)
                {
                    var child = children[i + col];
                    var size = child.View.SystemLayoutSizeFittingSize(new CGSize(columnWidth, 0));
                    rowHeight = MaxF(rowHeight, size.Height);
                }

                // Arrange children in this row
                for (int col = 0; col < numColumns && i < childCount; col++, i++)
                {
                    var child = children[i];
                    nfloat x = col * (columnWidth + _columnSpacing);

                    child.SetConstraints(
                        leftConstraint: new WrapperConstraint(this, NSLayoutAttribute.Left, 1, x),
                        topConstraint: new WrapperConstraint(this, NSLayoutAttribute.Top, 1, y),
                        rightConstraint: new WrapperConstraint(this, NSLayoutAttribute.Right) { GreaterThanOrEqual = true },
                        bottomConstraint: new WrapperConstraint(this, NSLayoutAttribute.Bottom) { GreaterThanOrEqual = true },
                        centeringHorizontalView: null,
                        centeringVerticalView: null,
                        widthConstraint: new WrapperConstraint(null, NSLayoutAttribute.Width, 1, columnWidth - child.Margin.Width));
                }

                y += rowHeight;
            }
        }
    }
}
