using Microsoft.Toolkit.Uwp.Notifications;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerUWPLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Globalization.DateTimeFormatting;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using static PowerPlannerUWPLibrary.TileHelpers.TileHelper;

namespace PowerPlannerUWPLibrary.TileHelpers
{
    public static class ScheduleTileHelper
    {

        public static bool IsPinned(Guid localAccountId)
        {
            try
            {
                return SecondaryTile.Exists(GenerateScheduleTileId(localAccountId));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return false;
            }
        }

        private static async Task<SecondaryTile> GetTile(Guid localAccountId)
        {
            SecondaryTile tile = (await SecondaryTile.FindAllAsync()).FirstOrDefault(i => i.TileId.Equals(GenerateScheduleTileId(localAccountId)));

            return tile;
        }

        public static async Task UnpinTile(Guid localAccountId)
        {
            try
            {
                SecondaryTile tile = await GetTile(localAccountId);

                if (tile != null)
                    await tile.RequestDeleteAsync();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        /// <summary>
        /// Pins and updates the secondary tile
        /// </summary>
        /// <param name="account"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task PinTile(AccountDataItem account, AccountDataStore data)
        {
            string args = new ViewScheduleArguments()
            {
                LocalAccountId = account.LocalAccountId
            }.SerializeToString();

            SecondaryTile tile = new SecondaryTile(GenerateScheduleTileId(account.LocalAccountId), "Schedule", args, new Uri("ms-appx:///Assets/Square150x150Logo.png"), TileSize.Default);
            tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/Square71x71Logo.png");
            tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.png");
            tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/Square310x310Logo.png");
            tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.png");
            tile.LockScreenBadgeLogo = new Uri("ms-appx:///Assets/LockScreenLogo.png");
            tile.LockScreenDisplayBadgeAndTileText = true;
            tile.VisualElements.ShowNameOnSquare150x150Logo = true;
            tile.VisualElements.ShowNameOnSquare310x310Logo = true;
            tile.VisualElements.ShowNameOnWide310x150Logo = true;

            if (!await tile.RequestCreateAsync())
                return;

            await UpdateScheduleTile(tile.TileId, account, data);
        }

        private static string GenerateScheduleTileId(Guid localAccountId)
        {
            return new TileIdInfo(localAccountId)
            {
                Type = TileType.Schedule

            }.ToString();
        }

        public static async Task UpdateScheduleTile(AccountDataItem account, AccountDataStore data)
        {
            try
            {
                await UpdateScheduleTile(GenerateScheduleTileId(account.LocalAccountId), account, data);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static async Task UpdateScheduleTile(string tileId, AccountDataItem account, AccountDataStore data)
        {
            try
            {
                if (!SecondaryTile.Exists(tileId))
                    return;

                Debug.WriteLine("Updating Secondary Schedule Tile");

                TileUpdater updater;

                try
                {
                    updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileId);
                }

                catch
                {
                    return;
                }

                TileHelper.ClearScheduledNotifications(updater);

                bool sentNotification = false;

                var notifications = await GenerateTileNotificationContentsAsync(account);

                foreach (var n in notifications)
                {
                    Schedule(updater, n.Content, n.DeliveryTime ?? DateTime.Now, n.ExpirationTime);
                    sentNotification = true;
                }

                if (!sentNotification)
                    updater.Clear();
            }

            catch (Exception ex)
            {
                if (!UWPExceptionHelper.TrackIfNotificationsIssue(ex, "Tiles") && !UWPExceptionHelper.TrackIfElementNotFound(ex, "Tiles"))
                {
                    throw ex;
                }
            }
        }

        private static async Task<IEnumerable<TileNotificationContentAndData>> GenerateTileNotificationContentsAsync(AccountDataItem account)
        {
            DateTime today = DateTime.Today;
            var scheduleTileData = await ScheduleTileDataHelper.LoadAsync(account, today, DAYS_INCLUDING_TODAY);

            return GenerateTileNotificationContents(scheduleTileData);
        }

        private static IEnumerable<TileNotificationContentAndData> GenerateTileNotificationContents(ScheduleTileDataHelper scheduleTileData)
        {
            // If no semester
            if (!scheduleTileData.HasSemester)
            {
                yield return new TileNotificationContentAndData()
                {
                    Content = CreateBasicTileNotificationContent(PowerPlannerResources.GetString("String_NoSemester"))
                };

                yield break;
            }

            bool createdContent = false;
            DateTime now = DateTime.Now.AddSeconds(30); // Use now as 30 secs in advance to prevent scheduling notifications that already passed
            List<DateTime> previousTimes = new List<DateTime>();

            foreach (var dayData in scheduleTileData.GetDataForAllDays())
            {
                // If nothing on this day
                if (!dayData.Schedules.Any() && !dayData.Holidays.Any())
                {
                    // Add that we skipped this day, and continue to next
                    previousTimes.Add(dayData.Date);
                    continue;
                }

                // Handle previously skipped days
                if (previousTimes.Count > 0)
                {
                    foreach (var dateSkipped in previousTimes)
                    {
                        XmlDocument xmlForSkipped;

                        if (dayData.Holidays.Any())
                        {
                            xmlForSkipped = GenerateHolidayTileNotification(dayData.Holidays, TileHelper.ToFriendlyDate(dayData.Date, dateSkipped), dayData.Date);
                        }
                        else
                        {
                            xmlForSkipped = GenerateTileNotification(dayData.Schedules, TileHelper.ToFriendlyDate(dayData.Date, dateSkipped), dayData.Date, dateSkipped);
                        }

                        yield return new TileNotificationContentAndData()
                        {
                            Content = xmlForSkipped,
                            DeliveryTime = dateSkipped,
                            ExpirationTime = dateSkipped.AddDays(1)
                        };
                        createdContent = true;
                    }

                    previousTimes.Clear();
                }

                // First one starts displaying exactly at the start of the day
                DateTime deliveryTime = dayData.Date;

                if (dayData.Holidays.Any())
                {
                    XmlDocument xml = GenerateHolidayTileNotification(dayData.Holidays, "Today", dayData.Date);

                    var expirationTime = dayData.Date.AddHours(15); // Switches at 3 PM
                    if (expirationTime > now)
                    {
                        yield return new TileNotificationContentAndData()
                        {
                            Content = xml,
                            DeliveryTime = deliveryTime,
                            ExpirationTime = expirationTime
                        };
                        createdContent = true;
                    }

                    // We always add this, so that the next day's content will start appearing after the last item
                    previousTimes.Add(expirationTime);
                }
                else
                {
                    var schedulesOnDate = dayData.Schedules.ToList();
                    while (schedulesOnDate.Count > 0)
                    {
                        DateTime expirationTime = dayData.Date.Add(schedulesOnDate.First().EndTime.TimeOfDay);

                        // If it hasn't expired yet
                        if (expirationTime > now)
                        {
                            XmlDocument xml = GenerateTileNotification(schedulesOnDate, "Today", dayData.Date, dayData.Date);

                            // If the class is 10 mins or longer, we'll switch it 5 mins before the class ends, so that user can see their next class
                            if ((schedulesOnDate.First().EndTime.TimeOfDay - schedulesOnDate.First().StartTime.TimeOfDay).TotalMinutes >= 10)
                                expirationTime = expirationTime.AddMinutes(-5);

                            yield return new TileNotificationContentAndData()
                            {
                                Content = xml,
                                DeliveryTime = deliveryTime,
                                ExpirationTime = expirationTime
                            };
                            createdContent = true;
                        }

                        deliveryTime = expirationTime;

                        schedulesOnDate.RemoveAt(0);
                    }

                    // We always add this, so that the next day's content will start appearing after the last item
                    previousTimes.Add(deliveryTime);
                }
            }

            if (!createdContent)
            {
                yield return new TileNotificationContentAndData()
                {
                    Content = CreateBasicTileNotificationContent(PowerPlannerResources.GetString("String_NoClassesThisWeek"))
                };
            }
        }

        private class TileNotificationContentAndData
        {
            public XmlDocument Content { get; set; }

            public DateTime? DeliveryTime { get; set; }

            public DateTime? ExpirationTime { get; set; }
        }

        private static XmlDocument CreateBasicTileNotificationContent(string content)
        {
            TileBinding binding = new TileBinding()
            {
                Content = new TileBindingContentAdaptive()
                {
                    Children =
                    {
                        new AdaptiveText()
                        {
                            Text = content,
                            HintWrap = true
                        }
                    }
                }
            };

            return new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = TileBranding.NameAndLogo,
                    TileMedium = binding,
                    TileWide = binding,
                    TileLarge = binding
                }
            }.GetXml();
        }

        private static void Schedule(TileUpdater updater, XmlDocument content, DateTime deliveryTime, DateTime? expirationTime)
        {
            if (deliveryTime == null || deliveryTime < DateTime.Now.AddSeconds(30))
            {
                var tileNotif = new TileNotification(content);
                if (expirationTime != null)
                {
                    tileNotif.ExpirationTime = expirationTime;
                }
                updater.Update(tileNotif);
            }

            else
            {
                var scheduledNotif = new ScheduledTileNotification(content, deliveryTime);
                if (expirationTime != null)
                {
                    scheduledNotif.ExpirationTime = expirationTime;
                }
                updater.AddToSchedule(scheduledNotif);
            }
        }

        private static XmlDocument GenerateHolidayTileNotification(IEnumerable<ViewItemHoliday> holidays, string displayName, DateTime dateOfHoliday)
        {
            var genericContent = GenerateGenericHolidayContent(holidays, dateOfHoliday);

            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    DisplayName = displayName,

                    TileSmall = new TileBinding()
                    {
                        Content = GenerateSmallHolidayContent(holidays, dateOfHoliday)
                    },

                    TileMedium = new TileBinding()
                    {
                        Content = genericContent
                    },

                    TileWide = new TileBinding()
                    {
                        Content = genericContent
                    },

                    TileLarge = new TileBinding()
                    {
                        Content = genericContent
                    }
                }
            };

            var firstItem = holidays.First();

            // Name
            content.Visual.LockDetailedStatus1 = trim(firstItem.Name, 255);

            // Date
            content.Visual.LockDetailedStatus2 = dateOfHoliday.ToString("D");

            return content.GetXml();
        }

        private static TileBindingContentAdaptive GenerateSmallHolidayContent(IEnumerable<ViewItemHoliday> holidays, DateTime date)
        {
            ViewItemHoliday first = holidays.First();

            string timeString;

            timeString = date.ToString("M/d");

            return new TileBindingContentAdaptive()
            {
                TextStacking = TileTextStacking.Center,

                Children =
                {
                    new AdaptiveText()
                    {
                        Text = TrimString(first.Name, 35),
                        HintAlign = AdaptiveTextAlign.Center
                    },

                    new AdaptiveText()
                    {
                        Text = timeString,
                        HintAlign = AdaptiveTextAlign.Center,
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    }
                }
            };
        }

        private static TileBindingContentAdaptive GenerateGenericHolidayContent(IEnumerable<ViewItemHoliday> holidays, DateTime date)
        {
            var answer = new TileBindingContentAdaptive();

            foreach (var h in holidays.Take(3))
            {
                answer.Children.Add(new AdaptiveText()
                {
                    Text = TrimString(h.Name, 180),
                    HintWrap = true
                });
            }

            return answer;
        }

        private static XmlDocument GenerateTileNotification(IEnumerable<ViewItemSchedule> schedules, string displayName, DateTime dateOfClass, DateTime currentDate)
        {
            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    DisplayName = displayName,

                    TileSmall = new TileBinding()
                    {
                        Content = GenerateSmallContent(schedules, dateOfClass, currentDate)
                    },

                    TileMedium = new TileBinding()
                    {
                        Content = GenerateTileNotificationMediumContent(schedules)
                    },

                    TileWide = new TileBinding()
                    {
                        Content = GenerateTileNotificationWideContent(schedules)
                    },

                    TileLarge = new TileBinding()
                    {
                        Content = GenerateTileNotificationLargeContent(schedules)
                    }
                }
            };

            var firstItem = schedules.First();

            string dateAndTimeString;

            if (currentDate == dateOfClass)
                dateAndTimeString = GetTimeStringWithAmPm(firstItem.StartTime) + " - " + GetTimeStringWithAmPm(firstItem.EndTime);
            else
                dateAndTimeString = displayName + ": " + GetTimeStringWithAmPm(firstItem.StartTime) + " - " + GetTimeStringWithAmPm(firstItem.EndTime);

            // Name
            content.Visual.LockDetailedStatus1 = trim(firstItem.Class.Name, 255);

            if (!string.IsNullOrWhiteSpace(firstItem.Room))
            {
                // Room
                content.Visual.LockDetailedStatus2 = trim(firstItem.Room, 255);

                // Time
                content.Visual.LockDetailedStatus3 = dateAndTimeString;
            }

            else
            {
                // Time
                content.Visual.LockDetailedStatus2 = dateAndTimeString;
            }

            return content.GetXml();
        }

        private static TileBindingContentAdaptive GenerateSmallContent(IEnumerable<ViewItemSchedule> schedules, DateTime date, DateTime relativeTo)
        {
            ViewItemSchedule first = schedules.First();

            string timeString;

            if (date.Date == relativeTo.Date)
                timeString = GetTimeString(first.StartTime);
            else
                timeString = date.ToString("M/d");

            return new TileBindingContentAdaptive()
            {
                TextStacking = TileTextStacking.Center,

                Children =
                {
                    new AdaptiveText()
                    {
                        Text = TrimString(first.Class.Name, 35),
                        HintAlign = AdaptiveTextAlign.Center
                    },

                    new AdaptiveText()
                    {
                        Text = timeString,
                        HintAlign = AdaptiveTextAlign.Center,
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    }
                }
            };
        }

        private static TileBindingContentAdaptive GenerateTileNotificationWideContent(IEnumerable<ViewItemSchedule> schedules)
        {
            TileBindingContentAdaptive content = new TileBindingContentAdaptive();

            if (schedules.Any())
            {
                // Create and add the first large content
                content.Children.Add(GenerateTileNotificationWideAndLargeGroupContent(schedules.ElementAt(0)));

                if (schedules.Count() > 1)
                {
                    // Add the image spacer
                    content.Children.Add(new AdaptiveImage()
                    {
                        Source = "Assets/Tiles/Padding/4px.png",
                        HintAlign = AdaptiveImageAlign.Left,
                        HintRemoveMargin = true
                    });

                    const int MAX_ADDITIONAL = 4;

                    for (int i = 1; i < schedules.Count() && i < MAX_ADDITIONAL; i++)
                    {
                        ViewItemSchedule schedule = schedules.ElementAt(i);

                        var group = new AdaptiveGroup()
                        {
                            Children =
                            {
                                new AdaptiveSubgroup()
                                {
                                    HintWeight = 20,
                                    Children =
                                    {
                                        new AdaptiveText()
                                        {
                                            Text = GetTimeString(schedule.StartTime),
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        }
                                    }
                                },
                                new AdaptiveSubgroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveText()
                                        {
                                            Text = TrimString(schedule.Class.Name, 22),
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        }
                                    }
                                }
                            }
                        };

                        if (!string.IsNullOrWhiteSpace(schedule.Room))
                        {
                            group.Children.Last().HintWeight = 30;

                            group.Children.Add(new AdaptiveSubgroup()
                            {
                                // Don't need to assign weight since it gets the remaining from 100,
                                // which saves spaces in the XML!
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = TrimString(schedule.Room, 32),
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            });
                        }

                        content.Children.Add(group);
                    }
                }
            }

            return content;
        }

        private static TileBindingContentAdaptive GenerateTileNotificationLargeContent(IEnumerable<ViewItemSchedule> schedules)
        {
            const int max = 4;

            TileBindingContentAdaptive content = new TileBindingContentAdaptive();

            for (int i = 0; i < schedules.Count() && i < max; i++)
            {
                // Add spacer
                if (content.Children.Count > 0)
                    content.Children.Add(new AdaptiveText());

                content.Children.Add(GenerateTileNotificationWideAndLargeGroupContent(schedules.ElementAt(i)));
            }

            return content;
        }

        private static AdaptiveGroup GenerateTileNotificationWideAndLargeGroupContent(ViewItemSchedule schedule)
        {
            return new AdaptiveGroup()
            {
                Children =
                {
                    new AdaptiveSubgroup()
                    {
                        HintWeight = 44,

                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = GetTimeString(schedule.StartTime),
                                HintStyle = AdaptiveTextStyle.SubheaderNumeral
                            }
                        }
                    },

                    new AdaptiveSubgroup()
                    {
                        HintWeight = 57,

                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = TrimString(schedule.Class.Name, 50),
                                HintAlign = AdaptiveTextAlign.Right
                            },

                            new AdaptiveText()
                            {
                                Text = schedule.Room == null ? "" : TrimString(schedule.Room, 50),
                                HintAlign = AdaptiveTextAlign.Right,
                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                            }
                        }
                    }
                }
            };
        }

        private static string GetTimeString(DateTime time)
        {
            return new DateTimeFormatter("{hour.integer}‎:‎{minute.integer(2)}").Format(DateTime.SpecifyKind(time, DateTimeKind.Local));
        }

        private static string GetTimeStringWithAmPm(DateTime time)
        {
            return new DateTimeFormatter("shorttime").Format(DateTime.SpecifyKind(time, DateTimeKind.Local));
        }

        private static TileBindingContentAdaptive GenerateTileNotificationMediumContent(IEnumerable<ViewItemSchedule> schedules)
        {
            TileBindingContentAdaptive content = new TileBindingContentAdaptive();

            for (int i = 0; i < schedules.Count() && i < 2; i++)
            {
                if (i != 0)
                {
                    // Incorporate spacing between items
                    content.Children.Add(new AdaptiveText());
                }

                content.Children.Add(GenerateTileNotificationMediumGroupContent(schedules.ElementAt(i)));
            }

            return content;
        }

        private static AdaptiveGroup GenerateTileNotificationMediumGroupContent(ViewItemSchedule schedule)
        {
            AdaptiveSubgroup subgroup = new AdaptiveSubgroup()
            {
                Children =
                {
                    new AdaptiveText()
                    {
                        Text = TrimString(schedule.Class.Name, 40)
                    },

                    new AdaptiveText()
                    {
                        Text = GetTimeString(schedule.StartTime),
                        HintStyle = AdaptiveTextStyle.TitleNumeral
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(schedule.Room))
                subgroup.Children.Add(new AdaptiveText()
                {
                    Text = TrimString(schedule.Room, 40),
                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                    HintWrap = true
                });

            return new AdaptiveGroup()
            {
                Children =
                {
                    subgroup
                }
            };
        }

        private const int DAYS_INCLUDING_TODAY = 14;

        public static async Task<XmlDocument> GetCurrentTileNotificationContentAsync(AccountDataItem forAccount)
        {
            try
            {
                if (forAccount == null)
                {
                    return null;
                }

                return (await GenerateTileNotificationContentsAsync(forAccount)).FirstOrDefault()?.Content;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return null;
            }
        }
    }
}
