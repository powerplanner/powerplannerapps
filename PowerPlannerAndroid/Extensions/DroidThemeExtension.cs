using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerPlannerAndroid.Extensions
{
    internal class DroidThemeExtension : ThemeExtension
    {
        public override void Relaunch()
        {
            ApplyTheme();
        }

        internal static void ApplyTheme(bool ignoreAutomatic = false)
        {
            switch (PowerPlannerAppDataLibrary.Helpers.Settings.ThemeOverride)
            {
                case Themes.Dark:
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
                    break;

                case Themes.Light:
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                    break;

                case Themes.Automatic:
                    if (!ignoreAutomatic)
                    {
                        // For the default setting (99% of users), don't even bother calling this
                        AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;
                    }
                    break;
            }
        }
    }
}