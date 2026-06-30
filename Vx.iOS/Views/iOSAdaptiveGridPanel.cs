using System;
using System.Collections.Generic;
using CoreGraphics;
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
                    InvalidateLayout();
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
                    InvalidateLayout();
                }
            }
        }

        private void InvalidateLayout()
        {
            MarkMeasureDirty();
            SetNeedsLayout();
            PropagateLayoutDirty(Superview);
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

        /// <summary>
        /// Computes the per-row height (max of the children in that row, including their margins)
        /// for the given total width, plus the column geometry.
        /// </summary>
        private nfloat[] ComputeRowHeights(nfloat totalWidth, out int numColumns, out nfloat columnWidth, out nfloat totalHeight)
        {
            var children = ArrangedSubviews;
            int childCount = children.Count;

            numColumns = GetNumberOfColumns(totalWidth, childCount);
            columnWidth = GetColumnWidth(totalWidth, numColumns);
            int numRows = (childCount + numColumns - 1) / numColumns;

            var rowHeights = new nfloat[numRows];
            totalHeight = 0;

            for (int i = 0; i < childCount; i++)
            {
                int row = i / numColumns;
                var child = children[i];

                nfloat marginW = child.Margin.Left + child.Margin.Right;
                nfloat marginH = child.Margin.Top + child.Margin.Bottom;
                nfloat cellContentWidth = MaxF(0, columnWidth - marginW);

                var measured = child.Measure(new CGSize(cellContentWidth, UIViewWrapper.UnboundedSize));
                rowHeights[row] = MaxF(rowHeights[row], measured.Height + marginH);
            }

            for (int r = 0; r < numRows; r++)
            {
                totalHeight += rowHeights[r];
            }

            return rowHeights;
        }

        public override CGSize MeasureContent(CGSize available)
        {
            int childCount = ArrangedSubviews.Count;
            if (childCount == 0)
            {
                return CGSize.Empty;
            }

            nfloat width = (available.Width > 0 && available.Width < UIViewWrapper.UnboundedSize)
                ? available.Width
                : _minColumnWidth;

            ComputeRowHeights(width, out _, out _, out nfloat totalHeight);
            return new CGSize(width, totalHeight);
        }

        public override void ArrangeContent(CGSize size)
        {
            var children = ArrangedSubviews;
            int childCount = children.Count;
            if (childCount == 0 || size.Width <= 0)
            {
                return;
            }

            var rowHeights = ComputeRowHeights(size.Width, out int numColumns, out nfloat columnWidth, out _);

            nfloat y = 0;
            int currentRow = 0;
            for (int i = 0; i < childCount; i++)
            {
                int row = i / numColumns;
                int col = i % numColumns;

                if (row != currentRow)
                {
                    y += rowHeights[currentRow];
                    currentRow = row;
                }

                nfloat x = col * (columnWidth + _columnSpacing);
                children[i].Arrange(new CGRect(x, y, columnWidth, rowHeights[row]));
            }
        }
    }
}
