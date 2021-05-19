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
using Android.Content.Res;
using AndroidX.Core.Content;

namespace InterfacesDroid.Helpers
{
    public static class ColorTools
    {
        public static Color GetColor(byte[] colorArray)
        {
            if (colorArray == null)
                return default(Color);

            if (colorArray.Length == 3)
                return new Color(colorArray[0], colorArray[1], colorArray[2]);
            else if (colorArray.Length == 4)
                return new Color(r: colorArray[1], g: colorArray[2], b: colorArray[3], a: colorArray[0]);

            return default(Color);
        }

        public static byte[] ToArray(Color color)
        {
            return new byte[] { color.R, color.G, color.B };
        }

        public static ColorStateList GetColorStateList(Color color)
        {
            return new ColorStateList(new int[][]
                { new int[0] },
                new int[]
                {
                    color
                });
        }

        public static bool IsInNightMode(Context context)
        {
            try
            {
                //Ensure the device is running Android Froyo or higher because UIMode was added in Android Froyo, API 8.0
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Froyo)
                {
                    var uiModeFlags = context.Resources.Configuration.UiMode & UiMode.NightMask;

                    switch (uiModeFlags)
                    {
                        case UiMode.NightYes:
                            return true;

                        default:
                            return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch { return false; }
        }

        public static Color GetColor(Context context, int resource)
        {
            return new Color(ContextCompat.GetColor(context, resource));
        }
    }
}