using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAndroid.Views.Controls
{
    public class Divider : View
    {
        public Divider(Context context) : base(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ThemeHelper.AsPx(context, 1));

            // https://stackoverflow.com/questions/37987732/programmatically-set-selectableitembackground-on-android-view (want it to be ?android:attr/listDivider)
            TypedValue outValue = new TypedValue();
            context.Theme.ResolveAttribute(Android.Resource.Attribute.ListDivider, outValue, true);
            SetBackgroundResource(outValue.ResourceId);
        }
    }
}