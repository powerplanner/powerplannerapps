using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Vx.Views;

namespace Vx.Droid.NativeViews
{
    public abstract class VxDroidNativeView<V, N> : VxNativeView<V, N>, IVxView
        where V : VxView
        where N : View
    {
        public int GridRow { set => MarkLayoutParamsNeedsUpdating(); }
        public int GridColumn { set => MarkLayoutParamsNeedsUpdating(); }
        public VxHorizontalAlignment HorizontalAlignment { set => MarkLayoutParamsNeedsUpdating(); }
        public VxVerticalAlignment VerticalAlignment { set => MarkLayoutParamsNeedsUpdating(); }
        public VxThickness Margin { set => MarkLayoutParamsNeedsUpdating(); }

        protected override object CreateNativeView()
        {
            _layoutParamsNeedsUpdating = true;
            return Activator.CreateInstance(typeof(N), new object[] { VxDroidNative.Context });
        }

        private bool _layoutParamsNeedsUpdating;

        private void MarkLayoutParamsNeedsUpdating()
        {
            _layoutParamsNeedsUpdating = true;
        }

        protected override void OnFinishedApplyingProperties()
        {
            base.OnFinishedApplyingProperties();

            if (_layoutParamsNeedsUpdating)
            {
                _layoutParamsNeedsUpdating = false;

                if (ParentView is VxGrid)
                {
                    NativeView.LayoutParameters = new GridLayout.LayoutParams()
                    {
                        Width = GetGridWidth(),
                        Height = GetGridHeight(),
                        RowSpec = GetRowSpec(),
                        ColumnSpec = GetColumnSpec()
                    };
                }

                else if (ParentView is VxStackPanel)
                {
                    NativeView.LayoutParameters = new LinearLayout.LayoutParams(GetLayoutParamsWidth(), GetLayoutParamsHeight())
                    {
                        Gravity = GetLayoutGravity()
                    };
                }

                else
                {
                    NativeView.LayoutParameters = new FrameLayout.LayoutParams(GetLayoutParamsWidth(), GetLayoutParamsHeight())
                    {
                        Gravity = GetLayoutGravity()
                    };
                }

                SetLayoutMargins();
            }
        }

        private int GetGridHeight()
        {
            var row = GetRowDefinition();
            if (row.Height.IsAuto)
            {
                return GridLayout.LayoutParams.WrapContent;
            }

            if (row.Height.IsAbsolute)
            {
                return GridLayout.LayoutParams.MatchParent;
            }

            // For stretch, height needs to be set to 0dp so that the weight gets applied
            return 0;
        }

        private int AsPixels(double dp)
        {
            // TODO: Fix this
            return (int)dp;
        }

        private int GetGridWidth()
        {
            var col = GetColumnDefinition();
            if (col.Width.IsAuto)
            {
                return GridLayout.LayoutParams.WrapContent;
            }

            if (col.Width.IsAbsolute)
            {
                return GridLayout.LayoutParams.MatchParent;
            }

            // For stretch, width needs to be set to 0dp so that the weight gets applied
            return 0;
        }

        private VxRowDefinition GetRowDefinition()
        {
            var rowDefinitions = (ParentView as VxGrid).RowDefinitions;
            return rowDefinitions?.ElementAtOrDefault(View.GridRow) ?? rowDefinitions?.Last() ?? new VxRowDefinition(VxGridLength.Star(1));
        }

        private VxColumnDefinition GetColumnDefinition()
        {
            var columnDefinitions = (ParentView as VxGrid).ColumnDefinitions;
            return columnDefinitions?.ElementAtOrDefault(View.GridColumn) ?? columnDefinitions?.Last() ?? new VxColumnDefinition(VxGridLength.Star(1));
        }

        private GridLayout.Spec GetRowSpec()
        {
            var row = GetRowDefinition();
            if (row.Height.IsAuto)
            {
                return GridLayout.InvokeSpec(View.GridRow);
            }

            if (row.Height.IsAbsolute)
            {
                return GridLayout.InvokeSpec(View.GridRow, AsPixels(row.Height.Value));
            }

            return GridLayout.InvokeSpec(View.GridRow, (float)row.Height.Value);
        }

        private GridLayout.Spec GetColumnSpec()
        {
            var col = GetColumnDefinition();
            if (col.Width.IsAuto)
            {
                return GridLayout.InvokeSpec(View.GridColumn);
            }

            if (col.Width.IsAbsolute)
            {
                return GridLayout.InvokeSpec(View.GridColumn, AsPixels(col.Width.Value));
            }

            return GridLayout.InvokeSpec(View.GridColumn, (float)col.Width.Value);
        }

        private void SetLayoutMargins()
        {
            if (NativeView.LayoutParameters is ViewGroup.MarginLayoutParams marginParams)
            {
                // Need to transform to DP
                //marginParams.BottomMargin = View.Margin.Bottom;
                //marginParams.
            }
        }

        private int GetLayoutParamsWidth()
        {
            if (View.HorizontalAlignment == VxHorizontalAlignment.Stretch)
            {
                return ViewGroup.LayoutParams.MatchParent;
            }
            else
            {
                return ViewGroup.LayoutParams.WrapContent;
            }
        }

        private int GetLayoutParamsHeight()
        {
            if (View.VerticalAlignment == VxVerticalAlignment.Stretch)
            {
                return ViewGroup.LayoutParams.MatchParent;
            }
            else
            {
                return ViewGroup.LayoutParams.WrapContent;
            }
        }

        private GravityFlags GetLayoutGravity()
        {
            GravityFlags flags = GravityFlags.NoGravity;

            switch (View.HorizontalAlignment)
            {
                case VxHorizontalAlignment.Left:
                    flags = flags | GravityFlags.Start;
                    break;

                case VxHorizontalAlignment.Center:
                    flags = flags | GravityFlags.CenterHorizontal;
                    break;

                case VxHorizontalAlignment.Right:
                    flags = flags | GravityFlags.End;
                    break;
            }

            return flags;
        }

        protected void SetListOfViewsOnViewGroup(VxView[] views, [CallerMemberName] string callerName = null)
        {
            ViewGroup viewGroup = NativeView as ViewGroup;

            SetListOfViews(views, (changeType, index, nativeView) =>
            {
                switch (changeType)
                {
                    case VxNativeViewListItemChange.Insert:
                        viewGroup.AddView(nativeView.NativeView as View, index);
                        break;

                    case VxNativeViewListItemChange.Replace:
                        viewGroup.RemoveViewAt(index);
                        viewGroup.AddView(nativeView.NativeView as View, index);
                        break;

                    case VxNativeViewListItemChange.Remove:
                        viewGroup.RemoveViewAt(index);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }, callerName);
        }
    }
}