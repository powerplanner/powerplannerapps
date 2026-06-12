using Plugin.Settings;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using ToolsPortable;
using Vx;

namespace PowerPlannerAppDataLibrary.App
{
    public class AppUpdatedHandler
    {
        private static bool _alreadyHandledVersionChange;
        public static void HandleAppVersionUpdated()
        {
            if (_alreadyHandledVersionChange)
            {
                // We'll make sure not to do this multiple times in single app lifecycle
                return;
            }

            _alreadyHandledVersionChange = true;

            const string VERSION = "Version";

            try
            {
                var versionString = CrossSettings.Current.GetValueOrDefault(VERSION, null);

                Version v;
                if (versionString != null && Version.TryParse(versionString, out v))
                {
                }
                else
                {
                    // If they have an account, that implies they were on version 5.3.16.0 which was the
                    // last version before I started storing the version number
                    if (AccountsManager.GetLastLoginLocalId() != Guid.Empty)
                    {
                        if (VxPlatform.Current == Platform.Android)
                        {
                            v = new Version(5, 3, 16, 0);
                        }
                        else if (VxPlatform.Current == Platform.iOS)
                        {
                            v = new Version(2606, 10, 118);
                        }
                        else
                        {
                            v = new Version(2606, 10, 113); // Uwp
                        }
                    }
                    else
                    {
                        v = new Version(0, 0, 0, 0);
                    }
                }

                // Assign new version number
                if (v < Variables.VERSION)
                {
                    CrossSettings.Current.AddOrUpdateValue(VERSION, Variables.VERSION.ToString());
                }

                // If was an existing user and things have changed
                if (v.Major > 0 && v < Variables.VERSION)
                {
                    DisplayWhatsNew(v);
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static void DisplayWhatsNew(Version v)
        {
            string changedText = "";

            if (v <= new Version(2606, 12, 0, 0))
            {
                changedText += "\n - Mac version of Power Planner is now available!";

                if (VxPlatform.Current == Platform.iOS)
                {
                    changedText += "\n - Lock screen widgets now supported!";
                    changedText += "\n - Class reminder notifications now supported!";
                }

                if (VxPlatform.Current == Platform.Uwp)
                {
                    changedText += "\n - Online sync now works when using VPNs";
                }
            }

            if (v <= new Version(2605, 10, 0, 0))
            {
                changedText += "\n - Add checklists to tasks / events! Try it when adding a task, below the details text box!";
            }

            if (v <= new Version(2605, 7, 0, 0))
            {
                changedText += "\n - Edit with AI beta feature! Try it on the calendar page, click the ⚡ icon.";
            }

            if (v <= new Version(2604, 7, 1, 99))
            {
                changedText += "\n - Custom theme colors added! Go to settings to pick your favorite color for the app!";
            }

            if (v <= new Version(2402, 19, 0, 0))
            {
                changedText += "\n - Ability to move semesters and classes to different semesters and years!";
            }

            if (v <= new Version(2302, 11, 2, 99))
            {
                changedText += "\n - Ability to copy semesters with their classes and schedules!";
            }

            if (v <= new Version(2109, 6, 1, 99))
            {
                changedText += "\n - Ability to override final grade/GPA on years/semesters";
            }

            if (v <= new Version(2108, 16, 2, 99))
            {
                changedText += "\n - Ability to override final grade/GPA on classes";
            }

            if (v < new Version(2105, 26, 1, 99))
            {
                changedText += "\n - Class reminders will respect semester end dates (so you won't get reminders after your semester is over)";
            }

            if (v <= new Version(2105, 17, 1, 0))
            {
                changedText += "\n - Default grade scales! Go to the settings page to configure default grade options for all classes!";
            }

            if (changedText.Length > 0)
            {
                _ = new PortableMessageDialog("Power Planner just installed an update!\n\nHere's what's new...\n" + changedText, "Just updated").ShowAsync();
            }
        }
    }
}
