using System;
using System.Globalization;

namespace Vx.Extensions
{
    public static class CultureOverride
    {
        private static CultureInfo _originalCultureInfo;

        /// <summary>
        /// The original device culture info before any app-level overrides are applied.
        /// Used to revert to "automatic" language. Platforms where the process culture
        /// may already be overridden at startup (e.g. UWP with PrimaryLanguageOverride)
        /// should call <see cref="SetOriginalCultureInfo"/> before calling
        /// <see cref="ApplyCultureOverride"/>.
        /// </summary>
        public static CultureInfo OriginalCultureInfo
        {
            get => _originalCultureInfo ?? CultureInfo.CurrentUICulture;
        }

        /// <summary>
        /// Sets the original device culture info. This should be called early at startup
        /// on platforms where the process culture may already have been overridden
        /// (e.g. UWP sets PrimaryLanguageOverride which affects CurrentUICulture before
        /// .NET code runs). Must be called before <see cref="ApplyCultureOverride"/>.
        /// </summary>
        public static void SetOriginalCultureInfo(CultureInfo cultureInfo)
        {
            _originalCultureInfo = cultureInfo;
        }

        /// <summary>
        /// Applies the given language code to CultureInfo.CurrentCulture, CurrentUICulture,
        /// DefaultThreadCurrentCulture, and DefaultThreadCurrentUICulture so that
        /// DateTime.ToString() and other culture-sensitive APIs use the correct locale.
        /// Pass null or empty string to revert to the original culture.
        /// </summary>
        public static void ApplyCultureOverride(string code)
        {
            CultureInfo cultureInfo;

            if (string.IsNullOrEmpty(code))
            {
                cultureInfo = OriginalCultureInfo;
            }
            else
            {
                cultureInfo = new CultureInfo(code);
            }

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;

            DateTimeFormatterExtension.Current?.ClearCache();
        }
    }
}
