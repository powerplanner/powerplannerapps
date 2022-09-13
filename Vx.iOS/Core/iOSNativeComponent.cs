using CoreGraphics;
using Foundation;
using InterfacesiOS.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UIKit;
using Vx.iOS.Views;
using Vx.Views;

namespace Vx.iOS
{
    public class iOSNativeComponent : UIView, INativeComponent
    {
        public Action<UIView> AfterViewChanged { get; set; }

        public VxComponent Component { get; private set; }

        public iOSNativeComponent(VxComponent component)
        {
            Component = component;
        }

        public SizeF ComponentSize => new SizeF((float)this.Frame.Width, (float)this.Frame.Height);

        public event EventHandler<SizeF> ComponentSizeChanged;
        public event EventHandler ThemeChanged;
        public event EventHandler<bool> MouseOverChanged;

        private SizeF _currSize;

        private UIViewWrapper _viewWrapper;

        public override void LayoutSubviews()
        {
            if (_viewWrapper != null)
            {
                _viewWrapper.Measure(Frame.Size);
                _viewWrapper.Arrange(new CoreGraphics.CGPoint(0, 0), Frame.Size);
            }

            var newSize = ComponentSize;
            if (_currSize != newSize)
            {
                _currSize = newSize;
                ComponentSizeChanged?.Invoke(this, newSize);
            }
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            if (SdkSupportHelper.IsUserInterfaceStyleSupported && previousTraitCollection != null && previousTraitCollection.UserInterfaceStyle != TraitCollection.UserInterfaceStyle)
            {
                ThemeChanged?.Invoke(this, null);
            }
        }

        public void ChangeView(View view)
        {
            foreach (var subview in base.Subviews)
            {
                subview.RemoveFromSuperview();
            }

            base.RemoveConstraints(base.Constraints);

            _viewWrapper = null;

            if (view == null)
            {
                return;
            }

            _viewWrapper = view.CreateUIView(null);
            var uiView = _viewWrapper.View;

            base.Add(uiView);

            InvalidateIntrinsicContentSize();
            SetNeedsLayout();

            AfterViewChanged?.Invoke(uiView);
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            if (_viewWrapper != null)
            {
                _viewWrapper.Measure(size);
                return _viewWrapper.DesiredSize;
            }

            return new CGSize(0, 0);
        }

        public override CGSize IntrinsicContentSize => SizeThatFits(new CGSize(nfloat.MaxValue, nfloat.MaxValue));
    }
}