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
                    SetNeedsUpdateConstraints();
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
                    SetNeedsUpdateConstraints();
                }
            }
        }

        public override void ArrangeSubviews()
        {
            var width = Frame.Size.Width;
            var columns = (int)(width / ItemWidth);
            if (columns == 0)
            {
                columns = 1;
            }

            var rows = ArrangedSubviews.BatchAsArrays(columns).ToArray();

            for (int rIndex = 0; rIndex < rows.Length; rIndex++)
            {
                var r = rows[rIndex];

                for (int cIndex = 0; cIndex < columns; cIndex++)
                {
                    var view = r[cIndex];

                    var left = cIndex * ItemWidth;
                    var top = rIndex * ItemHeight;

                    view.Width = ItemWidth;
                    view.Height = ItemHeight;

                    view.SetConstraints(
                        leftConstraint: new WrapperConstraint(this, NSLayoutAttribute.Left, 1, left),
                        topConstraint: new WrapperConstraint(this, NSLayoutAttribute.Top, 1, top),
                        rightConstraint: null,
                        bottomConstraint: null);
                }
            }
        }
    }
}
