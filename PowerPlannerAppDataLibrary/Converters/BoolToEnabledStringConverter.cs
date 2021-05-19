using System;
namespace PowerPlannerAppDataLibrary.Converters
{
    public class BoolToEnabledStringConverter
    {
        public static string Convert(bool value)
        {
            if (value)
            {
                return PowerPlannerResources.GetString("String_Enabled");
            }

            return PowerPlannerResources.GetString("String_Disabled");
        }
    }
}
