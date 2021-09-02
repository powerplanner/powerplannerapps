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
        public Action<UIView> AfterViewChanged { get; set; }

        public VxComponent Component { get; private set; }

        public iOSNativeComponent(VxComponent component)
        {
            Component = component;
        }

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

            var modifiedMargin = view.Margin.AsModified();
            uiView.StretchWidthAndHeight(this, modifiedMargin.Left, modifiedMargin.Top, modifiedMargin.Right, modifiedMargin.Bottom);

            AfterViewChanged?.Invoke(uiView);
        }
    }
}