using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;

namespace PowerPlannerAndroid.Extensions
{
    public class DroidAppShortcutsExtension : AppShortcutsExtension
    {
        public override Task UpdateAsync(bool showAddItemShortcuts)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.NMr1)
            {
                return Task.CompletedTask;
            }

            var shortcutManager = (ShortcutManager)Application.Context.GetSystemService(Context.ShortcutService);
            if (shortcutManager == null)
            {
                return Task.CompletedTask;
            }

            List<ShortcutInfo> shortcuts = new List<ShortcutInfo>();

            // Add task shortcut
            var addTaskIntent = new Intent(Application.Context, typeof(MainActivity));
            addTaskIntent.SetData(Android.Net.Uri.Parse("powerplanner:?" + new QuickAddTaskToCurrentAccountArguments().SerializeToString()));
            addTaskIntent.SetAction(Intent.ActionView);

            ShortcutInfo addTaskShortcut = new ShortcutInfo.Builder(Application.Context, "add-task")
                .SetShortLabel(PowerPlannerResources.GetString("String_Task")) // "Task"
                .SetLongLabel(PowerPlannerResources.GetString("String_AddTask")) // "Add task" (this one is almost always used)
                .SetIcon(Icon.CreateWithResource(Application.Context, Resource.Drawable.ic_add_24px))
                .SetIntent(addTaskIntent)
                .Build();

            shortcuts.Add(addTaskShortcut);

            // Add event shortcut
            var addEventIntent = new Intent(Application.Context, typeof(MainActivity));
            addEventIntent.SetData(Android.Net.Uri.Parse("powerplanner:?" + new QuickAddEventToCurrentAccountArguments().SerializeToString()));
            addEventIntent.SetAction(Intent.ActionView);

            ShortcutInfo addEventShortcut = new ShortcutInfo.Builder(Application.Context, "add-event")
                .SetShortLabel(PowerPlannerResources.GetString("String_Event")) // "Event"
                .SetLongLabel(PowerPlannerResources.GetString("String_AddEvent")) // "Add event"
                .SetIcon(Icon.CreateWithResource(Application.Context, Resource.Drawable.ic_add_24px))
                .SetIntent(addEventIntent)
                .Build();

            shortcuts.Add(addEventShortcut);

            var currShortcuts = shortcutManager.DynamicShortcuts;
            if (currShortcuts.Count == 2)
            {
                // Update existing
                shortcutManager.UpdateShortcuts(shortcuts);
            }
            else
            {
                // Add/reset
                if (currShortcuts.Count != 0)
                {
                    shortcutManager.RemoveAllDynamicShortcuts();
                }

                shortcutManager.AddDynamicShortcuts(shortcuts);
            }

            return Task.CompletedTask;
        }
    }
}