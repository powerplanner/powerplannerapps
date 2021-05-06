using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace Vx.iOS.Views
{
    public class iOSVxComponent : iOSView<Vx.Views.VxComponent, UIView>
    {
        public iOSVxComponent(Vx.Views.VxComponent component) : base(component.Render())
        {

        }
    }
}