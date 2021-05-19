using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace InterfacesDroid.Views
{
    /// <summary>
    /// My re-creation of GridView functionality (automatic columns) but as a layout (no scrolling).
    /// </summary>
    public class GridViewLayout : GridLayout
    {
        public GridViewLayout(Context context) : base(context)
        {
        }

        public GridViewLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public int ColumnWidth { get; private set; } = 100;

        public void SetColumnWidth(int columnWidth)
        {
            if (columnWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columnWidth));
            }

            if (ColumnWidth != columnWidth)
            {
                ColumnWidth = columnWidth;
                Update();
            }
        }

        /// <summary>
        /// For now, only stretch column width is implemented.
        /// </summary>
        public StretchMode StretchMode { get; private set; } = StretchMode.StretchColumnWidth;

        /// <summary>
        /// For now, always -1 (auto).
        /// </summary>
        public int NumColumns { get; private set; } = -1;

        private void Update()
        {
            if (StretchMode == StretchMode.StretchColumnWidth && NumColumns == -1)
            {
                int actualNumColumns = Math.Max(1, Width / ColumnWidth);

                if (ColumnCount != actualNumColumns)
                {
                    ColumnCount = actualNumColumns;
                }
                // Setting RowCount seems to throw an exception for some reason
            }
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            Update();
        }

        public override void OnViewAdded(View child)
        {
            base.OnViewAdded(child);

            Update();
        }

        public override void OnViewRemoved(View child)
        {
            base.OnViewRemoved(child);

            Update();
        }
    }
}