using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAndroid.Views.Controls
{
    public class FullWidthSpinner : Spinner
    {
        public FullWidthSpinner(Context context) : base(context)
        {
            SetPadding(0, 0, 0, 0);
            SetBackgroundResource(0);
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ThemeHelper.AsPx(context, 62));
        }
    }
}