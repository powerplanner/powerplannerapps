using Android.Content;
using Android.Views;
using InterfacesDroid.Themes;
using System;
using Vx.Views;

namespace Vx.Droid.Views
{
    internal class DroidAdaptiveGridPanel : DroidView<AdaptiveGridPanel, DroidAdaptiveGridPanel.AdaptiveGridPanelLayout>
    {
        public DroidAdaptiveGridPanel() : base(new AdaptiveGridPanelLayout(VxDroidExtensions.ApplicationContext))
        {
        }

        protected override void ApplyProperties(AdaptiveGridPanel oldView, AdaptiveGridPanel newView)
        {
            base.ApplyProperties(oldView, newView);

            View.MinColumnWidth = ThemeHelper.AsPx(View.Context, newView.MinColumnWidth);
            View.ColumnSpacing = ThemeHelper.AsPx(View.Context, newView.ColumnSpacing);

            ReconcileChildren(oldView?.Children, newView.Children, View);
        }

        public class AdaptiveGridPanelLayout : ViewGroup
        {
            private int _minColumnWidth = 250;
            private int _columnSpacing = 24;

            public AdaptiveGridPanelLayout(Context context) : base(context)
            {
            }

            public int MinColumnWidth
            {
                get => _minColumnWidth;
                set
                {
                    if (_minColumnWidth != value)
                    {
                        _minColumnWidth = value;
                        RequestLayout();
                    }
                }
            }

            public int ColumnSpacing
            {
                get => _columnSpacing;
                set
                {
                    if (_columnSpacing != value)
                    {
                        _columnSpacing = value;
                        RequestLayout();
                    }
                }
            }

            private int GetNumberOfColumns(int width, int childCount)
            {
                if (childCount <= 1)
                    return 1;

                int cols = (int)((width - _columnSpacing) / (double)(_minColumnWidth + _columnSpacing));
                if (cols <= 0)
                    cols = 1;
                if (cols > childCount)
                    cols = childCount;
                return cols;
            }

            private int GetColumnWidth(int totalWidth, int numColumns)
            {
                if (numColumns <= 1)
                    return totalWidth;
                return (totalWidth - _columnSpacing * (numColumns - 1)) / numColumns;
            }

            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                int widthSize = MeasureSpec.GetSize(widthMeasureSpec);
                int childCount = ChildCount;

                if (widthSize == 0 || childCount == 0)
                {
                    SetMeasuredDimension(widthSize, 0);
                    return;
                }

                // Stretch if only one child
                if (childCount == 1)
                {
                    var child = GetChildAt(0);
                    child.Measure(
                        MeasureSpec.MakeMeasureSpec(widthSize, MeasureSpecMode.Exactly),
                        MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                    SetMeasuredDimension(widthSize, child.MeasuredHeight);
                    return;
                }

                int numColumns = GetNumberOfColumns(widthSize, childCount);
                int columnWidth = GetColumnWidth(widthSize, numColumns);

                int totalHeight = 0;
                int i = 0;

                while (i < childCount)
                {
                    int rowHeight = 0;

                    for (int col = 0; col < numColumns && i < childCount; col++, i++)
                    {
                        var child = GetChildAt(i);
                        child.Measure(
                            MeasureSpec.MakeMeasureSpec(columnWidth, MeasureSpecMode.Exactly),
                            MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                        rowHeight = Math.Max(rowHeight, child.MeasuredHeight);
                    }

                    totalHeight += rowHeight;
                }

                SetMeasuredDimension(widthSize, totalHeight);
            }

            protected override void OnLayout(bool changed, int l, int t, int r, int b)
            {
                int width = r - l;
                int childCount = ChildCount;

                if (width == 0 || childCount == 0)
                    return;

                if (childCount == 1)
                {
                    var child = GetChildAt(0);
                    child.Layout(0, 0, width, child.MeasuredHeight);
                    return;
                }

                int numColumns = GetNumberOfColumns(width, childCount);
                int columnWidth = GetColumnWidth(width, numColumns);

                int y = 0;
                int i = 0;

                while (i < childCount)
                {
                    int rowHeight = 0;

                    // First pass: determine row height
                    for (int col = 0; col < numColumns && i + col < childCount; col++)
                    {
                        rowHeight = Math.Max(rowHeight, GetChildAt(i + col).MeasuredHeight);
                    }

                    // Second pass: arrange children
                    for (int col = 0; col < numColumns && i < childCount; col++, i++)
                    {
                        var child = GetChildAt(i);
                        int x = col * (columnWidth + _columnSpacing);
                        child.Layout(x, y, x + columnWidth, y + child.MeasuredHeight);
                    }

                    y += rowHeight;
                }
            }
        }
    }
}
