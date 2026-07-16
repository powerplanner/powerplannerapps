using CoreGraphics;
using Foundation;
using System;
using UIKit;

namespace Vx.iOS.Views
{
    /// <summary>
    /// A <see cref="UIButton"/> that hosts arbitrary Vx content as a subview. Using a custom-type
    /// UIButton gives a completely unstyled button (no background, no title/image styling) that
    /// still supports proper button semantics: touch-down highlight states, target/action events,
    /// accessibility button traits, pointer/hover effects on Mac Catalyst and iPadOS, and the
    /// modern <see cref="UIButton.Menu"/> / <see cref="UIButton.ShowsMenuAsPrimaryAction"/> APIs.
    /// </summary>
    public class UIButtonContentView : UIButton
    {
        private readonly UIContentView _contentView;

        public UIButtonContentView()
        {
            // The default UIButton init produces a UIButtonType.Custom button (no system styling).
            // Explicitly ensure no configuration-based background is applied.
            if (OperatingSystem.IsIOSVersionAtLeast(15))
            {
                Configuration = null;
            }

            _contentView = new UIContentView
            {
                TranslatesAutoresizingMaskIntoConstraints = true,

                // Forward all touches to this button so it behaves as a single button
                UserInteractionEnabled = false
            };

            base.AddSubview(_contentView);
        }

        public UIViewWrapper Content
        {
            get => _contentView.Content;
            set => _contentView.Content = value;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            // Manually fill the button with the Vx content host (no Auto Layout constraints).
            _contentView.Frame = Bounds;
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            return _contentView.MeasureContent(size);
        }

        public override CGSize IntrinsicContentSize
        {
            get
            {
                nfloat width = Bounds.Width > 0 ? Bounds.Width : UIViewWrapper.UnboundedSize;
                return _contentView.MeasureContent(new CGSize(width, UIViewWrapper.UnboundedSize));
            }
        }

        public override bool Highlighted
        {
            get => base.Highlighted;
            set
            {
                base.Highlighted = value;

                // Provide press feedback by dimming the content
                _contentView.Alpha = value ? 0.6f : 1f;
            }
        }
    }
}
