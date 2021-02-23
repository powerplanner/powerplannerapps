using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using PowerPlannerAndroid.Vx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAndroid.Views.Controls
{
    public class FullWidthEditText : AppCompatEditText
    {
        public FullWidthEditText(Context context) : base(context)
        {
            this.VxPadding(16);
            this.VxBackgroundResource(0);
            this.SetMinHeight(Resources.GetDimensionPixelSize(Resource.Dimension.fullWidthItemHeight));
        }
    }
}