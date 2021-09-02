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
    }
}
