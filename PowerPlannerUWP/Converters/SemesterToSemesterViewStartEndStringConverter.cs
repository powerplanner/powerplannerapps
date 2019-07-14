using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Converters
{
    public class SemesterToSemesterViewStartEndStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ViewItemSemester semester = value as ViewItemSemester;
            if (semester == null)
                return value;

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

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
