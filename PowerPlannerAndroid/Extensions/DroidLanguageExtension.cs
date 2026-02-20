using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;

namespace PowerPlannerAndroid.Extensions
{
    public class DroidLanguageExtension : LanguageExtension
    {
        public override string GetLanguageOverrideCode()
        {
            return Settings.LanguageOverride ?? "";
        }

        public override void SetLanguageOverrideCode(string code)
        {
            Settings.LanguageOverride = string.IsNullOrEmpty(code) ? null : code;
            ApplyCultureOverride(code);
        }
    }
}
