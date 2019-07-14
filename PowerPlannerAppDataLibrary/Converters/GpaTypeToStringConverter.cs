using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Converters
{
    public static class GpaTypeToStringConverter
    {
        public static string Convert(GpaType gpaType)
        {
            switch (gpaType)
            {
                case GpaType.PassFail:
                    return PowerPlannerResources.GetString("Settings_GradeOptions_GpaType_PassFail.Text");

                default:
                    return PowerPlannerResources.GetString("Settings_GradeOptions_GpaType_Standard.Text");
            }
        }
    }
}
