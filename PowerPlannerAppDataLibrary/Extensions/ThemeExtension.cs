using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class ThemeExtension
    {
        public static ThemeExtension Current { get; set; }

        public abstract void Relaunch();
    }

    public enum Themes
    {
        Automatic,
        Light,
        Dark
    }

    public static class ThemeExtensionMethods
    {
        public static string ToLocalizedString(this Themes theme)
        {
            switch (theme)
            {
                case Themes.Automatic:
                    return PowerPlannerResources.GetString("String_Automatic");

                case Themes.Light:
                    return PowerPlannerResources.GetString("String_Light");

                case Themes.Dark:
                    return PowerPlannerResources.GetString("String_Dark");

                default:
                    return theme.ToString();
            }
        }
    }
}
