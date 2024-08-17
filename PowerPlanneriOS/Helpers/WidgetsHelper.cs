using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Foundation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
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
                    await UpdateScheduleWidget(reload: false);

                    Reload();
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static void Reload()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(14,0))
            {
                ReloadWidgets();
            }
        }

        private static SimpleAsyncWorkerQueue _primaryWidgetWorkQueue = new SimpleAsyncWorkerQueue();
        private static Task UpdatePrimaryWidgetAsync()
        {
            return _primaryWidgetWorkQueue.QueueOrMergeAsync("primary", async delegate
            {
                try
                {
                    var account = await AccountsManager.GetLastLogin();

                    if (account != null)
                    {
                        AccountDataStore data = await AccountDataStore.Get(account.LocalAccountId);
                        var items = (await data.GetAllUpcomingItemsForWidgetAsync(DateTime.Today.Date, 10)).Items ?? new List<ViewItemTaskOrEvent>();

                        SavePrimaryWidgetData(new PrimaryWidgetData
                        {
                            Items = ConvertToPrimaryWidgetDataItems(items)
                        });
                    }
                    else
                    {
                        SavePrimaryWidgetData(new PrimaryWidgetData
                        {
                            ErrorMessage = R.S("String_NoAccount")
                        });
                    }
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                    SavePrimaryWidgetData(new PrimaryWidgetData
                    {
                        ErrorMessage = "Error generating data"
                    });
                }
            });
        }

        private static SimpleAsyncWorkerQueue _scheduleWidgetWorkQueue = new SimpleAsyncWorkerQueue();
        public static async Task UpdateScheduleWidget(bool reload = true)
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(14,0))
            {
                return;
            }

            await _scheduleWidgetWorkQueue.QueueOrMergeAsync("schedule", async delegate
            {
                try
                {
                    var account = await AccountsManager.GetLastLogin();

                    if (account != null)
                    {
                        var helper = await ScheduleTileDataHelper.LoadAsync(account, DateTime.Today, 14);
                        SaveScheduleWidgetData(helper);
                    }
                    else
                    {
                        SaveScheduleWidgetData(new ScheduleWidgetData
                        {
                            ErrorMessage = R.S("String_NoAccount")
                        });
                    }

                    if (reload)
                    {
                        Reload();
                    }
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                    SaveScheduleWidgetData(new ScheduleWidgetData
                    {
                        ErrorMessage = "Error generating data"
                    });
                }
            });
        }

        private static void SavePrimaryWidgetData(PrimaryWidgetData data)
        {
            SaveWidgetState("primaryWidget.json", data);
        }

        private static PrimaryWidgetDataItem[] ConvertToPrimaryWidgetDataItems(List<ViewItemTaskOrEvent> items)
        {
            PrimaryWidgetDataItem[] dataItems = items.Take(10).Select(i => new PrimaryWidgetDataItem
            {
                Name = i.Name,
                Color = i.Class.Color.Select(c => (int)c).ToArray(), // Byte arrays are serialized as Base64 strings, so serialize as an int array which will deserialize on iOS side
                Date = i.Date
            }).ToArray();

            return dataItems;
        }

        private static void SaveScheduleWidgetData(ScheduleTileDataHelper helper)
        {
            if (!helper.HasSemester)
            {
                SaveScheduleWidgetData(new ScheduleWidgetData
                {
                    ErrorMessage = R.S("String_NoSemester")
                });
                return;
            }

            List<ScheduleWidgetDayItem> dayItems = new List<ScheduleWidgetDayItem>();
            foreach (var day in helper.GetDataForAllDays())
            {
                dayItems.Add(new ScheduleWidgetDayItem
                {
                    Date = day.Date,
                    Holidays = day.Holidays.Any() ? day.Holidays.Select(i => i.Name).ToArray() : null,
                    Schedules = day.Schedules.Any() ? day.Schedules.Select(i => new ScheduleWidgetScheduleItem
                    {
                        ClassName = i.Class.Name,
                        ClassColor = i.Class.Color.Select(c => (int)c).ToArray(),
                        StartTime = i.StartTimeInLocalTime(day.Date).TimeOfDay,
                        EndTime = i.EndTimeInLocalTime(day.Date).TimeOfDay,
                        Room = i.Room
                    }).ToArray() : null
                });
            }

            SaveScheduleWidgetData(new ScheduleWidgetData
            {
                Days = dayItems.ToArray()
            });
        }

        private static void SaveScheduleWidgetData(ScheduleWidgetData data)
        {
            SaveWidgetState("scheduleWidget.json", data);
        }

        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static void SaveWidgetState(string fileName, object data)
        {
            try
            {
                NSUrl url = NSFileManager.DefaultManager.GetContainerUrl("group.com.barebonesdev.powerplanner");
                url = url.Append(fileName, false);
                System.IO.File.WriteAllText(url.Path, JsonConvert.SerializeObject(data, _jsonSettings));
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }

    public class PrimaryWidgetData
    {
        public string Title => R.S("MainMenuItem_Agenda");

        public PrimaryWidgetDataItem[] Items { get; set; }

        public string ErrorMessage { get; set; }

        public RelativeDateStrings DateStrings => new RelativeDateStrings();

        public string AllDoneString => R.S("Agenda_NoItemsHeader.Text");
    }

    public class RelativeDateStrings
    {
        public string InThePast => R.S("RelativeDate_InThePast");

        public string Today => R.S("RelativeDate_Today");

        public string Tomorrow => R.S("RelativeDate_Tomorrow");

        public string InTwoDays => PowerPlannerResources.GetRelativeDateInTwoDays();

        public string ThisX => R.S("RelativeDate_ThisDayOfWeek");

        public string NextX => R.S("RelativeDate_NextDayOfWeek");
    }

    public class PrimaryWidgetDataItem
    {
        public string Name { get; set; }

        public int[] Color { get; set; }

        public DateTime Date { get; set; }
    }

    public class ScheduleWidgetData
    {
        public string Title => R.S("MainMenuItem_Schedule");

        public string ErrorMessage { get; set; }

        public ScheduleWidgetDayItem[] Days { get; set; }

        public RelativeDateStrings DateStrings => new RelativeDateStrings();

        public string NoClassesString => R.S("String_NoUpcomingClasses");
    }

    public class ScheduleWidgetDayItem
    {
        public DateTime Date { get; set; }

        public string[] Holidays { get; set; }

        public ScheduleWidgetScheduleItem[] Schedules { get; set; }
    }

    public class ScheduleWidgetScheduleItem
    {
        public string ClassName { get; set; }
        
        public int[] ClassColor { get; set; }

        [JsonIgnore]
        public TimeSpan StartTime { get; set; }

        [JsonIgnore]
        public TimeSpan EndTime { get; set; }

        // Swift is able to parse seconds directly into TimeInterval
        [JsonProperty("startTime")]
        public int StartTimeInSeconds => (int)StartTime.TotalSeconds;

        [JsonProperty("endTime")]
        public int EndTimeInSeconds => (int)EndTime.TotalSeconds;

        public string Room { get; set; }
    }
}