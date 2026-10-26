using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using Google.Android.Material.TextField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Droid;

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
                // Min API is 23 (Marshmallow), so UIMode (API 8, Froyo) is always available
                var uiModeFlags = context.Resources.Configuration.UiMode & UiMode.NightMask;

                switch (uiModeFlags)
                {
                    case UiMode.NightYes:
                        return true;

                    default:
                        return false;
                }
            }
            catch { return false; }
        }

        public static Color GetColor(Context context, int resource)
        {
            return new Color(ContextCompat.GetColor(context, resource));
        }

        public static void ApplyTheme(TextInputLayout textInputLayout)
        {
            var colorStateList = new Android.Content.Res.ColorStateList(
                new int[][] {
                    new int[] { Android.Resource.Attribute.StateFocused },
                    new int[] { }
                },
                new int[] {
                    Vx.Views.Theme.Current.AccentColor.ToDroid(),
                    Vx.Views.Theme.Current.SubtleForegroundColor.ToDroid()
                });

            var subtleColorStateList = new Android.Content.Res.ColorStateList(
                new int[][] {
                    new int[] { },
                },
                new int[] {
                    Vx.Views.Theme.Current.SubtleForegroundColor.ToDroid()
                });

            textInputLayout.SetBoxStrokeColorStateList(colorStateList);
            textInputLayout.DefaultHintTextColor = subtleColorStateList;
            textInputLayout.HintTextColor = colorStateList;
            textInputLayout.CursorColor = colorStateList;
            textInputLayout.PlaceholderTextColor = colorStateList;

            // Set highlight color
            var pc = Vx.Views.Theme.Current.PrimaryColor;
            pc = System.Drawing.Color.FromArgb(100, pc.R, pc.G, pc.B);
            textInputLayout.EditText.SetHighlightColor(pc.ToDroid());

            // Set text color
            textInputLayout.EditText.SetTextColor(Vx.Views.Theme.Current.ForegroundColor.ToDroid());
        }
    }
}