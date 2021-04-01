using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace InterfacesDroid.Views
{
    public abstract class InflatedView : FrameLayout
    {
        public InflatedView(Context context, int resource, IAttributeSet attrs) : base(context, attrs)
        {
            base.AddView(LayoutInflater.FromContext(context).Inflate(resource, null));
        }

        public InflatedView(Context context, int resource) : base(context)
        {
            base.AddView(LayoutInflater.FromContext(context).Inflate(resource, null));
        }
    }
}