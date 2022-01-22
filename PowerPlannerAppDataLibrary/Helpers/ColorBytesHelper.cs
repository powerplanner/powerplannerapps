using System;
using System.Drawing;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public static class ColorBytesHelper
    {
        public static Color ToColor(this byte[] colorArray)
        {
            if (colorArray == null)
                return default(Color);

            if (colorArray.Length == 3)
                return Color.FromArgb(255, colorArray[0], colorArray[1], colorArray[2]);
            else if (colorArray.Length == 4)
                return Color.FromArgb(colorArray[0], colorArray[1], colorArray[2], colorArray[3]);

            return default(Color);
        }

        public static Color Invert(this Color color)
        {
            return Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);
        }

        /// <summary>
        /// Returns a color with the overlay color on top
        /// </summary>
        /// <param name="color"></param>
        /// <param name="overlayColor"></param>
        /// <param name="overlayOpacity"></param>
        /// <returns></returns>
        public static Color Overlay(this Color color, Color overlayColor, double overlayOpacity)
        {
            double amountFrom = 1.0 - overlayOpacity;

            return Color.FromArgb(
            (int)(color.A * amountFrom + overlayColor.A * overlayOpacity),
            (int)(color.R * amountFrom + overlayColor.R * overlayOpacity),
            (int)(color.G * amountFrom + overlayColor.G * overlayOpacity),
            (int)(color.B * amountFrom + overlayColor.B * overlayOpacity));
        }

        public static Color Opacity(this Color color, double opacity)
        {
            return Color.FromArgb((int)(opacity * 255), color.R, color.G, color.B);
        }
    }
}
