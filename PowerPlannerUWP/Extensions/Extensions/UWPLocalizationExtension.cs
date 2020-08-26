using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerUWP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerUWPLibrary.Extensions
{
    public class UWPLocalizationExtension : LocalizationExtension
    {
        public override string GetString(string id)
        {
            return LocalizedResources.GetString(id);
        }
    }
}
