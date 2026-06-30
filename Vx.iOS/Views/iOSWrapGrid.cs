using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    internal class iOSWrapGrid : iOSView<Vx.Views.WrapGrid, UIWrapGrid>
    {
        protected override void ApplyProperties(WrapGrid oldView, WrapGrid newView)
        {
            View.HoldOffApplyingChanges();

            base.ApplyProperties(oldView, newView);

            View.ItemWidth = newView.ItemWidth;
            View.ItemHeight = newView.ItemHeight;

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

    internal class UIWrapGrid : UIPanel
    {
        private float _itemWidth = 40;
        public float ItemWidth
        {
            get => _itemWidth;
            set
            {
                if (_itemWidth != value)
                {
                    _itemWidth = value;
                    InvalidateLayout();
                }
            }
        }

        private float _itemHeight = 40;
        public float ItemHeight
        {
            get => _itemHeight;
            set
            {
                if (_itemHeight != value)
                {
                    _itemHeight = value;
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

        private int GetColumns(nfloat width)
        {
            if (width <= 0 || width >= UIViewWrapper.UnboundedSize)
            {
                return ArrangedSubviews.Count > 0 ? ArrangedSubviews.Count : 1;
            }

            int columns = (int)(width / ItemWidth);
            return columns < 1 ? 1 : columns;
        }

        public override CGSize MeasureContent(CGSize available)
        {
            int count = ArrangedSubviews.Count;
            if (count == 0)
            {
                return CGSize.Empty;
            }

            int columns = GetColumns(available.Width);
            if (columns > count)
            {
                columns = count;
            }
            int rows = (count + columns - 1) / columns;

            return new CGSize(columns * ItemWidth, rows * ItemHeight);
        }

        public override void ArrangeContent(CGSize size)
        {
            var children = ArrangedSubviews;
            if (children.Count == 0)
            {
                return;
            }

            int columns = GetColumns(size.Width);

            for (int i = 0; i < children.Count; i++)
            {
                int row = i / columns;
                int col = i % columns;

                children[i].Arrange(new CGRect(col * ItemWidth, row * ItemHeight, ItemWidth, ItemHeight));
            }
        }
    }
}
