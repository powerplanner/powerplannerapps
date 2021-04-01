using System;
using System.Collections.Generic;
using System.Text;

namespace InterfacesiOS.Converters
{
    public static class TextToDoubleConverter
    {
        public static string Convert(double value)
        {
            return value.ToString();
        }

        public static double ConvertBack(string text)
        {
            if (double.TryParse(text, out double answer))
            {
                return answer;
            }

            return 0;
        }
    }
}
