using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.iOS.Views;
using Vx.Views;

namespace Vx.iOS
{
    public class iOSNativeComponent : UIView, INativeComponent
    {
        public void ChangeView(View view)
        {
            foreach (var subview in base.Subviews)
            {
                subview.RemoveFromSuperview();
            }

            base.RemoveConstraints(base.Constraints);

            var uiView = view.CreateUIView(null);
            uiView.TranslatesAutoresizingMaskIntoConstraints = false;

            base.Add(uiView);

            uiView.StretchWidthAndHeight(this, view.Margin.Left, view.Margin.Top, view.Margin.Right, view.Margin.Bottom);
        }
    }
}