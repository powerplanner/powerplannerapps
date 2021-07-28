using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;

namespace ToolsUniversal
{
    public class ColorTools
    {
        public static SolidColorBrush GetBrush(byte[] colorArray)
        {
            return new SolidColorBrush(GetColor(colorArray));
        }

        public static Windows.UI.Color GetColor(byte[] colorArray)
        {
            if (colorArray == null)
                return default(Windows.UI.Color);

            if (colorArray.Length == 3)
                return Windows.UI.Color.FromArgb(255, colorArray[0], colorArray[1], colorArray[2]);
            else if (colorArray.Length == 4)
                return Windows.UI.Color.FromArgb(colorArray[0], colorArray[1], colorArray[2], colorArray[3]);

            return default(Windows.UI.Color);
        }

        /// <summary>
        /// Amount should be either 3 or 4. If it's 3, it doesn't store the "a" value. 4 stores { A, R, G, B }, 3 stores { R, G, B }
        /// </summary>
        /// <param name="color"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static byte[] GetArray(Windows.UI.Color color, int amount)
        {
            if (amount == 3)
                return new byte[] { color.R, color.G, color.B };
            else if (amount == 4)
                return new byte[] { color.A, color.R, color.G, color.B };

            return null;
        }

        public static SolidColorBrush Lighten(Windows.UI.Color color, int percent)
        {
            double x = percent / 100.0;

            return new SolidColorBrush(new Windows.UI.Color()
                {
                    A = color.A,
                    R = (byte)(color.R * x),
                    G = (byte)(color.G * x),
                    B = (byte)(color.B * x)
                });
        }
    }
}
