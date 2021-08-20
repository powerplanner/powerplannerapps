using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Vx.Views
{
    public class AdaptiveGridPanel : VxComponent
    {
        public List<View> Children { get; private set; } = new List<View>();

        public float MinColumnWidth { get; set; } = 200;

        private int _renderedCols;

        public float ColumnSpacing { get; set; } = 24;

        protected override View Render()
        {
            var numOfCols = CalcNumberOfColumns();
            _renderedCols = numOfCols;

            if (numOfCols == 1 || Children.Count <= 1 || Size.Width == 0)
            {
                var layout = new LinearLayout();
                layout.Children.AddRange(Children);
                return layout;
            }

            // When we have too few children, stretch them
            numOfCols = Children.Count < numOfCols ? Children.Count : numOfCols;

            int col = 0;
            var parentLayout = new LinearLayout();
            LinearLayout currHorizLayout = null;
            foreach (var child in Children)
            {
                if (currHorizLayout == null || col == numOfCols)
                {
                    col = 0;

                    currHorizLayout = new LinearLayout()
                    {
                        Orientation = Orientation.Horizontal
                    };

                    parentLayout.Children.Add(currHorizLayout);
                }

                if (currHorizLayout.Children.Count > 0)
                {
                    currHorizLayout.Children.Add(new Border
                    {
                        Width = ColumnSpacing
                    });
                }

                currHorizLayout.Children.Add(child.LinearLayoutWeight(1));

                col++;
            }

            // If we didn't finish the last column
            if (currHorizLayout != null)
            {
                for (int c = col; c < numOfCols; c++)
                {
                    if (currHorizLayout.Children.Count > 0)
                    {
                        currHorizLayout.Children.Add(new Border
                        {
                            Width = ColumnSpacing
                        });
                    }

                    currHorizLayout.Children.Add(new Border().LinearLayoutWeight(1));
                }
            }

            return parentLayout;
        }

        protected override void OnSizeChanged(SizeF size)
        {
            var calc = CalcNumberOfColumns();
            if (_renderedCols != calc)
            {
                MarkDirty();
            }
        }

        private int CalcNumberOfColumns()
        {
            // ColWidth * Cols + ColSpacing * (Cols - 1) = TotalWidth
            // ColWidth * Cols + ColSpacing * Cols - ColSpacing = TotalWidth
            // Cols * (ColWidth + ColSpacing) = TotalWidth - ColSpacing
            // Cols = (TotalWidth - ColSpacing) / (ColWidth + ColSpacing)
            int cols = (int)((Size.Width - ColumnSpacing) / (MinColumnWidth + ColumnSpacing));
            return cols >= 1 ? cols : 1;
        }
    }
}
