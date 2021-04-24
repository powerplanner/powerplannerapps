using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Content.Res;
using InterfacesDroid.Themes;
using AndroidX.RecyclerView.Widget;

namespace InterfacesDroid.Decorations
{
    public class DividerItemDecoration : RecyclerView.ItemDecoration
    {
        private Drawable _divider;
        private int _dividerHeight;

        public DividerItemDecoration(Context context)
        {
            // Default divider
            TypedArray styledAttributes = context.ObtainStyledAttributes(new int[] { Android.Resource.Attribute.ListDivider });
            _divider = styledAttributes.GetDrawable(0);
            _dividerHeight = _divider.IntrinsicHeight;
            //_dividerHeight = ThemeHelper.AsPx(context, 1);
        }

        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            int position = parent.GetChildAdapterPosition(view);

            // For first item, no offset needed
            if (position == 0)
            {
                return;
            }

            outRect.Top = _dividerHeight;
        }

        public override void OnDrawOver(Canvas cValue, RecyclerView parent, RecyclerView.State state)
        {
            int left = parent.PaddingLeft;
            int right = parent.Width - parent.PaddingRight;
            int childCount = parent.ChildCount;

            for (int i = 1; i < childCount; i++)
            {
                View child = parent.GetChildAt(i);
                RecyclerView.LayoutParams lp = (RecyclerView.LayoutParams)child.LayoutParameters;
                int size = _dividerHeight;
                int top = child.Top - lp.TopMargin - _dividerHeight;
                int bottom = top + size;
                _divider.Bounds = new Rect(left, top, right, bottom);
                _divider.Draw(cValue);
            }
        }
    }
}