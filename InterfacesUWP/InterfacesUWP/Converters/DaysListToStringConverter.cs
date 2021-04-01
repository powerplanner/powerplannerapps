using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Power_Planner_v_3.Converters
{
    public class DaysListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            IEnumerable<DayOfWeek> days = value as IEnumerable<DayOfWeek>;

            if (days == null)
                return "";

            if (days.Count() == 1)
                return days.First().ToString();

            string str = "";

            foreach (DayOfWeek d in days)
            {
                if (d.ToString().Length > 3)
                    str += d.ToString().Substring(0, 3);
                else
                    str += d.ToString();

                str += ", ";
            }

            return str.Substring(0, str.Length - 2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
