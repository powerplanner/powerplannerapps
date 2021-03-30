using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Droid.NativeViews
{
    public class VxDroidTextBlock : VxDroidNativeView<VxTextBlock, AppCompatTextView>, IVxTextBlock
    {
        public string Text { set => NativeView.Text = value; }
    }
}