using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Converters
{
    public static class GradeToTextBoxTextConverter
    {
        public static string Convert(double value)
        {
            if (value == PowerPlannerSending.Grade.UNGRADED)
                return "";

            return value.ToString();
        }

        public static double ConvertBack(string value)
        {
            double parsed;
            if (value is string && double.TryParse(value as string, out parsed))
            {
                return parsed;
            }

            return PowerPlannerSending.Grade.UNGRADED;
        }
    }
}
