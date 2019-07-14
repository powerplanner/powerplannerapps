using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Converters
{
    public class ItemTypeToDatePrefixConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string initialString = "- ";
            if (parameter is string && (parameter as string).Equals("IncludeSpace"))
            {
                initialString = " - ";
            }

            if (value is ViewItemHomework)
            {
                if ((value as ViewItemHomework).Class.IsNoClassClass)
                {
                    return CapitalizeFirstLetter(LocalizedResources.GetString("String_HomeworkDatePrefix"));
                }
                else
                {
                    return initialString + LocalizedResources.GetString("String_HomeworkDatePrefix");
                }
            }

            else if (value is ViewItemExam)
            {
                if ((value as ViewItemExam).Class.IsNoClassClass)
                {
                    return CapitalizeFirstLetter(LocalizedResources.GetString("String_ExamDatePrefix"));
                }
                else
                {
                    return initialString + LocalizedResources.GetString("String_ExamDatePrefix");
                }
            }

            return value;
        }

        private static string CapitalizeFirstLetter(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
