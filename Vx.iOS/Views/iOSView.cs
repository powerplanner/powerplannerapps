using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Vx.Views;

namespace Vx.iOS.Views
{
    public abstract class iOSView<V, N> : NativeView<V, N> where V : View where N : UIView
    {
        public iOSView()
        {
            View = Activator.CreateInstance<N>();
            //View.TranslatesAutoresizingMaskIntoConstraints = false;
        }

        public iOSView(N view)
        {
            View = view;
            //view.TranslatesAutoresizingMaskIntoConstraints = false;
        }

        protected override void ApplyProperties(V oldView, V newView)
        {
            // Nothing yet
        }
    }
}