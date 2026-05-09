using Android.Content;
using Android.Views;
using Android.Widget;
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
                        UpdateChildLayoutParams(numColumns);
                        ColumnCount = numColumns;
                    }
                }

                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            }

            internal void UpdateChildLayoutParams(int numColumns)
            {
                int halfSpacing = _columnSpacing / 2;

                for (int i = 0; i < ChildCount; i++)
                {
                    var child = GetChildAt(i);
                    int col = i % numColumns;

                    var lp = new GridLayout.LayoutParams(
                        GridLayout.InvokeSpec(GridLayout.Undefined, 1f),
                        GridLayout.InvokeSpec(GridLayout.Undefined, 1f));
                    lp.Width = 0;

                    if (numColumns > 1)
                    {
                        int leftMargin = col == 0 ? 0 : halfSpacing;
                        int rightMargin = col == numColumns - 1 ? 0 : halfSpacing;
                        lp.SetMargins(leftMargin, 0, rightMargin, 0);
                    }

                    child.LayoutParameters = lp;
                }
            }
        }
    }
}
