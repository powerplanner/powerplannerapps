using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;

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

            if (value is ViewItemTaskOrEvent task && task.Type == TaskOrEventType.Task)
            {
                if (task.Class.IsNoClassClass)
                {
                    return CapitalizeFirstLetter(LocalizedResources.GetString("String_TaskDatePrefix"));
                }
                else
                {
                    return initialString + LocalizedResources.GetString("String_TaskDatePrefix");
                }
            }

            else if (value is ViewItemTaskOrEvent eventItem && eventItem.Type == TaskOrEventType.Event)
            {
                if (eventItem.Class.IsNoClassClass)
                {
                    return CapitalizeFirstLetter(LocalizedResources.GetString("String_EventDatePrefix"));
                }
                else
                {
                    return initialString + LocalizedResources.GetString("String_EventDatePrefix");
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
