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
using Vx.Views;

namespace Vx.Droid.NativeViews
{
    public class VxDroidLinearLayout : VxDroidNativeView<VxLinearLayout, LinearLayout>, IVxLinearLayout
    {
        protected override void Initialize()
        {
            base.Initialize();

            NativeView.Orientation = Android.Widget.Orientation.Vertical;
        }

        public VxView[] Children { set => SetListOfViewsOnViewGroup(value); }
        public VxOrientation Orientation { set => NativeView.Orientation = value == VxOrientation.Vertical ? Android.Widget.Orientation.Vertical : Android.Widget.Orientation.Horizontal; }
    }
}