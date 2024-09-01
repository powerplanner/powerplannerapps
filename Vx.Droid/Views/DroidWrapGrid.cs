using Android.Content;
using Android.Util;
using Android.Views;
using InterfacesDroid.Themes;
using InterfacesDroid.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;
using static Android.Icu.Text.ListFormatter;
using static Android.Views.View;

namespace Vx.Droid.Views
{
    internal class DroidWrapGrid : DroidView<Vx.Views.WrapGrid, DroidWrapGrid.WrapGridLayout>
    {
        public DroidWrapGrid() : base(new WrapGridLayout(VxDroidExtensions.ApplicationContext))
        {
        }

        protected override void ApplyProperties(WrapGrid oldView, WrapGrid newView)
        {
            base.ApplyProperties(oldView, newView);

            View.SetItemSize(ThemeHelper.AsPx(View.Context, newView.ItemWidth), ThemeHelper.AsPx(View.Context, newView.ItemHeight));
            View.SetBackgroundColor(newView.BackgroundColor.ToDroid());

            ReconcileChildren(oldView?.Children, newView.Children, View);
        }

        public class WrapGridLayout : ViewGroup
        {
            private int _itemWidth = 100;
            private int _itemHeight = 100;

            public WrapGridLayout(Context context) : base(context)
            {
            }

            // Set the item width and height
            public void SetItemSize(int width, int height)
            {
                bool changed = false;

                if (_itemWidth != width)
                {
                    _itemWidth = width;
                    changed = true;
                }

                if (_itemHeight != height)
                {
                    _itemHeight = height;
                    changed = true;
                }

                if (changed)
                {
                    RequestLayout();
                }
            }

            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                int widthSize = MeasureSpec.GetSize(widthMeasureSpec);
                int heightSize = MeasureSpec.GetSize(heightMeasureSpec);

                int columnCount = widthSize / _itemWidth;
                if (columnCount == 0)
                {
                    columnCount = 1;
                }

                int totalHeight = 0;
                int childCount = ChildCount;
                int rowCount = (int)Math.Ceiling((double)childCount / columnCount);

                // Measure each child
                for (int i = 0; i < childCount; i++)
                {
                    var child = GetChildAt(i);
                    MeasureChild(child, MeasureSpec.MakeMeasureSpec(_itemWidth, MeasureSpecMode.Exactly),
                        MeasureSpec.MakeMeasureSpec(_itemHeight, MeasureSpecMode.Exactly));
                }

                totalHeight = rowCount * _itemHeight;

                // Set the measured dimensions of the layout
                SetMeasuredDimension(widthSize, totalHeight);
            }

            protected override void OnLayout(bool changed, int l, int t, int r, int b)
            {
                int columnCount = Width / _itemWidth;
                if (columnCount == 0)
                {
                    columnCount = 1;
                }

                int x = 0;
                int y = 0;
                int childCount = ChildCount;

                for (int i = 0; i < childCount; i++)
                {
                    var child = GetChildAt(i);

                    if (i % columnCount == 0 && i != 0)
                    {
                        x = 0;
                        y += _itemHeight;
                    }

                    child.Layout(x, y, x + _itemWidth, y + _itemHeight);
                    x += _itemWidth;
                }
            }
        }
    }
}
