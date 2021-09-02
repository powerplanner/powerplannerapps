using System;
using UIKit;
using Vx.Views;

namespace Vx.iOS.BareUIViews
{
    /// <summary>
    /// Horizontally centers content
    /// </summary>
    public class BareUICenteredView : UIView
    {
        public UIView Content { get; private set; }

        public BareUICenteredView(UIView content)
        {
            Content = content;
            content.TranslatesAutoresizingMaskIntoConstraints = false;

            Add(content);

            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
            {
                content.TopAnchor.ConstraintEqualTo(TopAnchor),
                content.BottomAnchor.ConstraintEqualTo(BottomAnchor),
                content.CenterXAnchor.ConstraintEqualTo(CenterXAnchor),
                content.LeftAnchor.ConstraintGreaterThanOrEqualTo(LeftAnchor),
                content.RightAnchor.ConstraintLessThanOrEqualTo(RightAnchor)
            });
        }
    }
}
