using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vx.Droid
{
    public class DroidNativeComponent : FrameLayout, INativeComponent
    {
        public DroidNativeComponent(Context context) : base(context) { }

        public void ChangeView(Vx.Views.View view)
        {
            base.RemoveAllViews();
            base.AddView(view.CreateDroidView(null));
        }
    }
}