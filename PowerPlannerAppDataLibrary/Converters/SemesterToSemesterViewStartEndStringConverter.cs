using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Converters
{
    public static class SemesterToSemesterViewStartEndStringConverter
    {
        public static string Convert(ViewItemSemester semester)
        {
            if (semester == null)
                return "";

            // If neither are assigned, do nothing
            if (PowerPlannerSending.DateValues.IsUnassigned(semester.Start) && PowerPlannerSending.DateValues.IsUnassigned(semester.End))
                return "";

            string start = "";
            if (!PowerPlannerSending.DateValues.IsUnassigned(semester.Start))
                start = semester.Start.ToString("d");

            string end = "";
            if (!PowerPlannerSending.DateValues.IsUnassigned(semester.End))
                end = semester.End.ToString("d");

            return start + " - \n" + end;
        }
    }
}
