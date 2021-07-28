using InterfacesUWP.Converters;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerUWP.Converters
{
    public class GradeToFormatConverter : NumberToFormatConverter
    {
        /// <summary>
        /// If the value is the UNGRADED constant, it'll return "--". Otherwise it formats the number with the provided parameter.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double && ((double)value == Grade.UNGRADED))
                return "--";

            return base.Convert(value, targetType, parameter, language);
        }
    }
}
