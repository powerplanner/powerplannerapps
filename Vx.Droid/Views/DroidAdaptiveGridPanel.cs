using Android.Content;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Themes;
using System;
using System.Collections.Generic;
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

            // Reset cached width so OnMeasure recalculates column count
            // with the updated child count
            View.ResetLastWidth();
            View.UpdateChildLayoutParams(View.ColumnCount);
        }

        public class AdaptiveGridPanelLayout : GridLayout
        {
            private int _minColumnWidth = 250;
            private int _columnSpacing = 24;
            private int _lastWidth = -1;

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
                        _lastWidth = -1;
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
                        _lastWidth = -1;
                        RequestLayout();
                    }
                }
            }

            internal void ResetLastWidth()
            {
                _lastWidth = -1;
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

            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                int widthSize = MeasureSpec.GetSize(widthMeasureSpec);

                if (widthSize > 0 && widthSize != _lastWidth)
                {
                    _lastWidth = widthSize;
                    int numColumns = GetNumberOfColumns(widthSize, ChildCount);
                    if (ColumnCount != numColumns)
                    {
                        ColumnCount = numColumns;
                        UpdateChildLayoutParams(numColumns);
                    }
                }

                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            }

            private Dictionary<Android.Views.View, (int Left, int Top, int Right, int Bottom)> _childOriginalMargins = new Dictionary<Android.Views.View, (int, int, int, int)>();

            public override void OnViewAdded(Android.Views.View child)
            {
                base.OnViewAdded(child);
                var lp = child.LayoutParameters as GridLayout.LayoutParams;
                if (lp != null)
                {
                    _childOriginalMargins[child] = (lp.LeftMargin, lp.TopMargin, lp.RightMargin, lp.BottomMargin);
                }
            }

            internal void UpdateChildOriginalMargins(Android.Views.View child, int left, int top, int right, int bottom)
            {
                _childOriginalMargins[child] = (left, top, right, bottom);
            }

            public override void OnViewRemoved(Android.Views.View child)
            {
                base.OnViewRemoved(child);
                _childOriginalMargins.Remove(child);
            }

            internal void UpdateChildLayoutParams(int numColumns)
            {
                int halfSpacing = _columnSpacing / 2;

                for (int i = 0; i < ChildCount; i++)
                {
                    var child = GetChildAt(i);
                    int row = i / numColumns;
                    int col = i % numColumns;

                    _childOriginalMargins.TryGetValue(child, out var originalMargins);

                    var lp = new GridLayout.LayoutParams(
                        GridLayout.InvokeSpec(row),
                        GridLayout.InvokeSpec(col, 1f));
                    lp.Width = 0;

                    int leftMargin = originalMargins.Left;
                    int rightMargin = originalMargins.Right;

                    if (numColumns > 1)
                    {
                        leftMargin += col == 0 ? 0 : halfSpacing;
                        rightMargin += col == numColumns - 1 ? 0 : halfSpacing;
                    }

                    lp.SetMargins(leftMargin, originalMargins.Top, rightMargin, originalMargins.Bottom);

                    child.LayoutParameters = lp;
                }
            }
        }
    }
}
