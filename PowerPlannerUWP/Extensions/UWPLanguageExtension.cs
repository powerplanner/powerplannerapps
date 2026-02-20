using InterfacesUWP.App;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
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

            if (code == null || code == "")
            {
                CultureInfo.DefaultThreadCurrentCulture = NativeUwpApplication.OriginalCultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = NativeUwpApplication.OriginalCultureInfo;
                CultureInfo.CurrentCulture = NativeUwpApplication.OriginalCultureInfo;
                CultureInfo.CurrentUICulture = NativeUwpApplication.OriginalCultureInfo;
            }
            else
            {
                CultureInfo cultureInfo = new CultureInfo(code);
                CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
                CultureInfo.CurrentCulture = cultureInfo;
                CultureInfo.CurrentUICulture = cultureInfo;
            }
        }
    }
}
