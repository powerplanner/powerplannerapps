using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace InterfacesiOS.Views
{
    public class BareUISafeView : UIView
    {
        public static BareUISafeView CreateAndAddTo(UIView parentView, float leftPadding = 0, float topPadding = 0, float rightPadding = 0, float bottomPadding = 0)
        {
            BareUISafeView answer = new BareUISafeView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            parentView.Add(answer);

            // https://stackoverflow.com/questions/46344381/ios-11-layout-guidance-about-safe-area-for-iphone-x
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] {
                    answer.LeftAnchor.ConstraintEqualTo(parentView.SafeAreaLayoutGuide.LeftAnchor, leftPadding),
                    answer.TopAnchor.ConstraintEqualTo(parentView.SafeAreaLayoutGuide.TopAnchor, topPadding),
                    answer.RightAnchor.ConstraintEqualTo(parentView.SafeAreaLayoutGuide.RightAnchor, rightPadding * -1),
                    answer.BottomAnchor.ConstraintEqualTo(parentView.SafeAreaLayoutGuide.BottomAnchor, bottomPadding * -1)
                });
            }
            else
            {
                answer.StretchWidthAndHeight(parentView, left: leftPadding, top: topPadding, right: rightPadding, bottom: bottomPadding);
            }

            return answer;
        }
    }
}