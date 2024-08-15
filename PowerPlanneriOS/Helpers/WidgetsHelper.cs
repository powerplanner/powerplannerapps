using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Foundation;
using Newtonsoft.Json;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.ViewItems;
using ToolsPortable;
using UIKit;

namespace PowerPlanneriOS.Helpers
{
    public static class WidgetsHelper
    {

        [DllImport ("__Internal", EntryPoint = "ReloadWidgets")]
        public extern static int ReloadWidgets ();

        public static async Task UpdateAllWidgetsAsync()
        {
            try
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(14,0))
                {
                    await UpdatePrimaryWidgetAsync();
                    UpdateScheduleWidget(reload: false);

Reload();
                }
            }
            catch (Exception ex)
            {

                PortableDispatcher.GetCurrentDispatcher().Run(delegate
                {
                    new PortableMessageDialog("Error: " + ex.ToString()).Show();
                });
            }
        }

        private static void Reload()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(14,0))
            {
                ReloadWidgets();
            }
        }

        private static async Task UpdatePrimaryWidgetAsync()
        {
            var account = await AccountsManager.GetLastLogin();

            if (account != null)
            {
                AccountDataStore data = await AccountDataStore.Get(account.LocalAccountId);
                var items = await data.GetAllUpcomingItemsForWidgetAsync(DateTime.UtcNow.Date);
                SavePrimaryWidgetItems(items);
            }
            else
            {
                SavePrimaryWidgetItems(new List<ViewItemTaskOrEvent>());
            }
        }

        public static void UpdateScheduleWidget(bool reload = true)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(14,0))
            {
            }

            if (reload)
            {
                //Reload();
            }
        }

        private static void SavePrimaryWidgetItems(List<ViewItemTaskOrEvent> items)
        {
            PrimaryWidgetDataItem[] dataItems = items.Take(10).Select(i => new PrimaryWidgetDataItem
            {
                Name = i.Name,
                Color = i.Class.Color.Select(c => (int)c).ToArray(), // Byte arrays are serialized as Base64 strings, so serialize as an int array which will deserialize on iOS side
                Date = i.Date
            }).ToArray();

            SaveWidgetState("primaryWidget.json", dataItems);
        }

        private static void SaveWidgetState(string fileName, object data)
        {
            NSUrl url = NSFileManager.DefaultManager.GetContainerUrl("group.com.barebonesdev.powerplanner");
            url = url.Append(fileName, false);
            System.IO.File.WriteAllText(url.Path, JsonConvert.SerializeObject(data));
        }
    }

    public class PrimaryWidgetDataItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("color")]
        public int[] Color { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }
    }
}