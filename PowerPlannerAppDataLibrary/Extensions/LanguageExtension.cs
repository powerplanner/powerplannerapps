using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Vx.Extensions;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class LanguageExtension
    {
        public static LanguageExtension Current { get; set; }

        /// <summary>
        /// The original culture info captured before any overrides are applied.
        /// Used to revert to "automatic" language.
        /// </summary>
        public static CultureInfo OriginalCultureInfo => CultureOverride.OriginalCultureInfo;

        public abstract string GetLanguageOverrideCode();

        public abstract void SetLanguageOverrideCode(string code);

        /// <summary>
        /// Applies the given language code to CultureInfo so that
        /// DateTime.ToString() and other culture-sensitive APIs use the correct locale.
        /// Pass null or empty string to revert to the original culture.
        /// </summary>
        public static void ApplyCultureOverride(string code) => CultureOverride.ApplyCultureOverride(code);

        public static Action OpenSystemAppLanguageSelector { get; set; }
    }
}
