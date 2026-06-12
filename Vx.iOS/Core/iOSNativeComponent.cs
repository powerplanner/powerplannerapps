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
    public class iOSNativeComponent : UIContentView, INativeComponent
    {
        public Action<UIView> AfterViewChanged { get; set; }

        public VxComponent Component { get; private set; }

        private IUITraitChangeRegistration _traitChangeRegistration;

        private UIHoverGestureRecognizer _hoverGestureRecognizer;

        public iOSNativeComponent(VxComponent component)
        {
            Component = component;
            _rendered = !component.DelayFirstRenderTillSizePresent;

            _traitChangeRegistration = this.RegisterForTraitChanges(
                [typeof(UITraitUserInterfaceStyle)],
                (self, previousTraitCollection) =>
                {
                    ThemeChanged?.Invoke(this, null);
                });

            if (component.SubscribeToIsMouseOver)
            {
                _hoverGestureRecognizer = new UIHoverGestureRecognizer(HandleHover);
                AddGestureRecognizer(_hoverGestureRecognizer);
            }
        }

        private void HandleHover(UIHoverGestureRecognizer recognizer)
        {
            switch (recognizer.State)
            {
                case UIGestureRecognizerState.Began:
                case UIGestureRecognizerState.Changed:
                    if (!Component.IsMouseOver.Value)
                    {
                        Component.IsMouseOver.Value = true;
                        MouseOverChanged?.Invoke(this, true);
                    }
                    break;

                case UIGestureRecognizerState.Ended:
                case UIGestureRecognizerState.Cancelled:
                case UIGestureRecognizerState.Failed:
                    if (Component.IsMouseOver.Value)
                    {
                        Component.IsMouseOver.Value = false;
                        MouseOverChanged?.Invoke(this, false);
                    }
                    break;
            }
        }

        public SizeF ComponentSize => new SizeF((float)this.Bounds.Width, (float)this.Bounds.Height);

        public event EventHandler<SizeF> ComponentSizeChanged;
        public event EventHandler ThemeChanged;
        public event EventHandler<bool> MouseOverChanged;

        private bool _rendered;

        private SizeF _currSize;

        public override CGRect Bounds
        {
            get => base.Bounds;
            set
            {
                base.Bounds = value;

                if (!_rendered)
                {
                    _rendered = true;
                    Component.InitializeForDisplay(this);
                }
                else
                {
                    var newSize = ComponentSize;
                    if (_currSize != newSize)
                    {
                        _currSize = newSize;
                        ComponentSizeChanged?.Invoke(this, newSize);
                    }
                }
            }
        }

        public void ChangeView(View view)
        {
            Content = view?.CreateUIView(null);

            if (Content?.View != null)
            {
                AfterViewChanged?.Invoke(Content.View);
            }
        }
    }
}