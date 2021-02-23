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

namespace PowerPlannerAndroid.Vx
{
    public class VxTextView : TextView
    {
        public VxTextView(Context context, VxTextStyle textStyle) : base(context, null, 0, VxTextStyleToTextStyle(textStyle))
        {

        }

        private static int VxTextStyleToTextStyle(VxTextStyle textStyle)
        {
            switch (textStyle)
            {
                case VxTextStyle.Small:
                    return Resource.Style.TextAppearance_AppCompat_Small;

                case VxTextStyle.Medium:
                    return Resource.Style.TextAppearance_AppCompat_Medium;

                case VxTextStyle.Large:
                    return Resource.Style.TextAppearance_AppCompat_Large;

                default:
                    throw new NotImplementedException();
            }
        }
    }

    public enum VxTextStyle
    {
        Small,
        Medium,
        Large
    }
}