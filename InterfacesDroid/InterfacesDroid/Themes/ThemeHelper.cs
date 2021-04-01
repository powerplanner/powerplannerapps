using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using static Android.Content.Res.Resources;
using Android.Util;
using Android.Graphics.Drawables;
using Android.Content.Res;

namespace InterfacesDroid.Themes
{
    public static class ThemeHelper
    {
        public static Color GetThemeColor(Context context, int attributeId)
        {
            Theme theme = context.Theme;

            TypedValue storedValueInTheme = new TypedValue();

            if (theme.ResolveAttribute(attributeId, storedValueInTheme, true))
            {
                return new Color(storedValueInTheme.Data);
            }
            
            throw new Exception("attributeId not found");
        }

        public static Drawable GetAttributeDrawable(Context context, int attributeId)
        {
            // http://stackoverflow.com/questions/20531516/how-do-i-add-selectableitembackground-to-an-imagebutton-programatically

            int[] attrs = new int[] { attributeId };

            TypedArray ta = context.ObtainStyledAttributes(attrs);

            try
            {
                Drawable drawable = ta.GetDrawable(0);
                return drawable;
            }

            finally
            { 
                ta.Recycle();
            }
        }

        /// <summary>
        /// Returns the absolute pixels from a given dp value
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dp"></param>
        /// <returns></returns>
        public static int AsPx(Context context, double dp)
        {
            return (int)AsPxPrecise(context, dp);
        }

        public static float AsPxPrecise(Context context, double dp)
        {
            return TypedValue.ApplyDimension(ComplexUnitType.Dip, (float)dp, context.Resources.DisplayMetrics);
        }
    }
}