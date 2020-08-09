using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;

namespace PowerPlannerUWP.Extensions
{
    public class UWPLanguageExtension : LanguageExtension
    {
        public override string GetLanguageOverrideCode()
        {
            return ApplicationLanguages.PrimaryLanguageOverride;
        }

        public override void SetLanguageOverrideCode(string code)
        {
            ApplicationLanguages.PrimaryLanguageOverride = code;
        }
    }
}
