using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Converters
{
    public static class WeightValueToTextBoxTextConverter
    {
        public static string Convert(double value)
        {
            return value.ToString();
        }

        public static double ConvertBack(string value)
        {
            double parsed;
            if (value is string && double.TryParse(value as string, out parsed))
            {
                return parsed;
            }

            return 0;
        }
    }
}
