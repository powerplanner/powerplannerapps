using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;
using static PowerPlannerAppDataLibrary.NavigationManager;

namespace PowerPlannerUWP.Converters
{
    public class MainMenuItemToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((MainMenuSelections)value)
            {
                case MainMenuSelections.Agenda:
                    return LocalizedResources.GetString("MainMenuItem_Agenda");

                case MainMenuSelections.Calendar:
                    return LocalizedResources.GetString("MainMenuItem_Calendar");

                case MainMenuSelections.Classes:
                    return LocalizedResources.GetString("MainMenuItem_Classes");

                case MainMenuSelections.Day:
                    return LocalizedResources.GetString("MainMenuItem_Day");

                case MainMenuSelections.Schedule:
                    return LocalizedResources.GetString("MainMenuItem_Schedule");

                case MainMenuSelections.Settings:
                    return LocalizedResources.GetString("MainMenuItem_Settings");

                case MainMenuSelections.Years:
                    return LocalizedResources.GetString("MainMenuItem_Years");

                default:
                    throw new NotImplementedException("Unknown MainMenuSelections enum value");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
