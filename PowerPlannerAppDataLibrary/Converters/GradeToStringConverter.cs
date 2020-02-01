using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Converters
{
    public class GradeToStringConverter
    {
        public static string Convert(double grade)
        {
            if (grade == PowerPlannerSending.Grade.UNGRADED)
            {
                return "--%";
            }

            return grade.ToString("0.##%");
        }
    }
}
