using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.Content.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Vx.Views;

namespace Vx.Droid
{
    public class VxDroidTheme : Theme
    {
        public override Color ForegroundColor => GetThemeColor(Android.Resource.Attribute.TextColorPrimary);

        public override Color SubtleForegroundColor => GetThemeColor(Android.Resource.Attribute.TextColorSecondary);

        public override Color PopupPageBackgroundColor => GetNormalColor(Android.Resource.Attribute.Background);

        public override Color PopupPageBackgroundAltColor => GetColor(Resource.Color.colorBackgroundSecondary);

        public override bool IsDarkTheme => VxDroidExtensions.ApplicationContext.Resources.Configuration.UiMode.HasFlag(UiMode.NightYes);

        private static Color GetThemeColor(int attributeId)
        {
            var context = VxDroidExtensions.ApplicationContext;
            Android.Content.Res.Resources.Theme theme = context.Theme;

            TypedValue storedValueInTheme = new TypedValue();

            if (theme.ResolveAttribute(attributeId, storedValueInTheme, true))
            {
                if (storedValueInTheme.ResourceId != 0)
                {
                    return Color.FromArgb(ContextCompat.GetColor(context, storedValueInTheme.ResourceId));
                }

                return Color.FromArgb(storedValueInTheme.Data);
            }

            throw new Exception("attributeId not found");
        }

        private static Color GetColor(int colorId)
        {
            //int colorInt = ResourcesCompat.GetColor(VxDroidExtensions.ApplicationContext.Resources, colorId, VxDroidExtensions.ApplicationContext.Theme);
            int colorInt = ContextCompat.GetColor(VxDroidExtensions.ApplicationContext, colorId);

            return Color.FromArgb(colorInt);
        }

        private static Color GetNormalColor(int attributeId)
        {
            int[] attrs = new int[] { attributeId };

            TypedArray ta = VxDroidExtensions.ApplicationContext.ObtainStyledAttributes(attrs);

            try
            {
                var color = ta.GetColor(0, 0);
                return Color.FromArgb(color.ToArgb());

                //var drawable = ta.GetDrawable(0);
                //return Color.FromArgb(((ColorDrawable)drawable).Color.ToArgb());
                //var color = ta.GetColor(0, 0);
                //return Color.FromArgb(color.ToArgb());
            }

            finally
            {
                ta.Recycle();
            }
        }
    }
}