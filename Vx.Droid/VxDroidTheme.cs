using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
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
    }
}