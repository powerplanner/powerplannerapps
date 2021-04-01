using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ToolsUniversal
{
    public static class ColorTranslator
    {
        public static Color FromHtml(string hexColorString)
        {
            if (hexColorString == null)
                throw new NullReferenceException("Hex string can't be null.");

            //ignore hash if at start
            int pos = hexColorString[0] == '#' ? 1 : 0;

            int length = hexColorString.Length - pos;

            // a value is optional
            byte a = 255, r = 0, b = 0, g = 0;

            if (length == 8)
            {
                a = Convert.ToByte(hexColorString.Substring(pos, 2), 16);
                pos += 2;
            }

            r = Convert.ToByte(hexColorString.Substring(pos, 2), 16);
            pos += 2;

            g = Convert.ToByte(hexColorString.Substring(pos, 2), 16);
            pos += 2;

            b = Convert.ToByte(hexColorString.Substring(pos, 2), 16);

            return Color.FromArgb(a, r, g, b);
        }
    }
}
