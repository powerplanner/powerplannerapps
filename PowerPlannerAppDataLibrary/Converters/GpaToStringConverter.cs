using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Converters
{
    public static class GpaToStringConverter
    {
        public static string ConvertWithGpa(double gpa)
        {
            return Convert(gpa, includeGpa: true);
        }

        public static string Convert(double gpa, bool includeGpa = false)
        {
            string answer;

            if (gpa != -1)
                answer = gpa.ToString("0.0##");

            else
                answer = "--";

            if (includeGpa)
            {
                return string.Format(PowerPlannerResources.GetString("String_GPA"), answer);
            }

            return answer;
        }
    }
}
