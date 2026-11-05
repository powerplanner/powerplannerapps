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

        public static double GetContrastRatio(Color c1, Color c2)
        {
            double l1 = GetRelativeLuminance(c1);
            double l2 = GetRelativeLuminance(c2);

            // L1 should be the lighter color, L2 the darker
            double lighter = Math.Max(l1, l2);
            double darker = Math.Min(l1, l2);

            return (lighter + 0.05) / (darker + 0.05);
        }

        public static bool HasSufficientContrast(this Color foreground, Color background, bool isLargeText = false)
        {
            double ratio = GetContrastRatio(foreground, background);
            double minimumRequired = isLargeText ? 3.0 : 4.5;

            return ratio >= minimumRequired;
        }

        private static double GetRelativeLuminance(Color color)
        {
            double r = LinearizeColorComponent(color.R / 255.0);
            double g = LinearizeColorComponent(color.G / 255.0);
            double b = LinearizeColorComponent(color.B / 255.0);

            // Standard sRGB luminance coefficients
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        private static double LinearizeColorComponent(double channel)
        {
            // WCAG formula for sRGB gamma correction
            return channel <= 0.04045
                ? channel / 12.92
                : Math.Pow((channel + 0.055) / 1.055, 2.4);
        }
    }
}
