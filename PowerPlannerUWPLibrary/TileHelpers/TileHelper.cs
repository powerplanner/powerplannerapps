using Microsoft.QueryStringDotNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using System.Threading;
using PowerPlannerUWP;
using ToolsPortable;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.DataLayer.TileSettings;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Helpers;
using ToolsUniversal;
using Microsoft.Toolkit.Uwp.Notifications;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerUWPLibrary.Helpers;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlannerUWPLibrary.TileHelpers
{
    public static class TileHelper
    {
        internal const string KEY_ACCOUNT = "account";

        private static MultipleChannelsWorkQueue _updateTileNotificationsForAccountWorkQueue = new MultipleChannelsWorkQueue();
        /// <summary>
        /// Updates primary tile and account's schedule tile. Runs on a separate thread.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.Task UpdateTileNotificationsForAccountAsync(AccountDataItem account, AccountDataStore data)
        {
            try
            {
                await _updateTileNotificationsForAccountWorkQueue.QueueOrMergeAsync(account.LocalAccountId, 0, async delegate
                {
                    await System.Threading.Tasks.Task.Run(async delegate
                    {
                        await UpdateTileNotificationsBlocking(account, data);

                        var accountTiles = await GetAllTilesForAccountAsync(account.LocalAccountId);

                        await ScheduleTileHelper.UpdateScheduleTile(account, data);

                        await ClassTileHelper.UpdateTilesAsync(account, data, accountTiles);

                    });
                });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                Debug.WriteLine("Failed UpdateTileNotificationsForAccountAsync");
            }
        }

        private static async Task UpdateTileNotificationsBlocking(AccountDataItem account, AccountDataStore data)
        {
            // If it's the main account, update primary tile
            if (account.LocalAccountId == AccountsManager.GetLastLoginLocalId())
                await UpdatePrimaryTileNotificationsBlocking(account, data);
        }

        public static async Task<XmlDocument> GetCurrentPrimaryTileNotificationContentAsync(AccountDataItem forAccount)
        {
            try
            {
                if (forAccount == null || forAccount.MainTileSettings.IsDisabled())
                {
                    return null;
                }

                AccountDataStore data = await AccountDataStore.Get(forAccount.LocalAccountId);

                return await Task.Run(async delegate
                {
                    DateTime todayInLocal = DateTime.Today;

                    var allUpcoming = await getAllUpcomingBlocking(forAccount, data, DateTime.SpecifyKind(todayInLocal, DateTimeKind.Utc), forAccount.MainTileSettings);

                    List<ItemsOnDay> groupedByDay = GroupByDay(allUpcoming);

                    return GenerateUpcomingTileNotificationContent(
                        groupedByDay: groupedByDay,
                        todayAsUtc: DateTime.SpecifyKind(todayInLocal, DateTimeKind.Utc),
                        useFriendlyDates: true,
                        type: UpcomingTileType.PrimaryTile);
                });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return null;
            }
        }

        private static SimpleAsyncWorkerQueue _updatePrimaryTileNotificationsWorkQueue = new SimpleAsyncWorkerQueue();
        public static async System.Threading.Tasks.Task UpdatePrimaryTileNotificationsAsync()
        {
            try
            {
                await _updatePrimaryTileNotificationsWorkQueue.QueueOrMergeAsync(0, async delegate
                {
                    await System.Threading.Tasks.Task.Run(async delegate
                    {
                        var account = await AccountsManager.GetLastLogin();

                        AccountDataStore data = null;

                        if (account != null)
                            data = await AccountDataStore.Get(account.LocalAccountId);

                        await UpdatePrimaryTileNotificationsBlocking(account, data);
                    });
                });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                Debug.WriteLine("Failed UpdatePrimaryTileNotificationsAsync");
            }
        }

        private static async Task UpdatePrimaryTileNotificationsBlocking(AccountDataItem account, AccountDataStore data)
        {
            try
            {
                Debug.WriteLine("Updating Primary Tile");

                TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();


                if (account == null || account.MainTileSettings.IsDisabled())
                {
                    ClearScheduledNotifications(tileUpdater);
                    tileUpdater.Clear();
                    return;
                }

                DateTime todayInLocal = DateTime.Today;

                var allUpcoming = await getAllUpcomingBlocking(account, data, DateTime.SpecifyKind(todayInLocal, DateTimeKind.Utc), account.MainTileSettings);

                UpdateUpcomingTile(tileUpdater, allUpcoming, todayInLocal, UpcomingTileType.PrimaryTile, account.MainTileSettings);
            }

            catch (Exception ex)
            {
                if (!UWPExceptionHelper.TrackIfNotificationsIssue(ex, "Tiles"))
                {
                    throw ex;
                }
            }
        }

        internal enum UpcomingTileType
        {
            PrimaryTile,
            ClassTile
        }

        internal static List<ItemsOnDay> GroupByDay(IEnumerable<BaseViewItemHomeworkExam> allUpcoming)
        {
            List<ItemsOnDay> answer = new List<ItemsOnDay>();

            ItemsOnDay curr = null;

            foreach (var item in allUpcoming)
            {
                var itemDateInUtc = DateTime.SpecifyKind(item.Date.Date, DateTimeKind.Utc);

                if (curr == null || curr.DateInUtc != itemDateInUtc)
                {
                    curr = new ItemsOnDay()
                    {
                        DateInUtc = itemDateInUtc
                    };

                    answer.Add(curr);
                }

                curr.Items.Add(item);
            }

            return answer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updater"></param>
        /// <param name="allUpcoming">This list will be manipulated. Should already be sorted.</param>
        /// <param name="todayInLocal"></param>
        /// <param name="type"></param>
        internal static void UpdateUpcomingTile(TileUpdater updater, List<BaseViewItemHomeworkExam> allUpcoming, DateTime todayInLocal, UpcomingTileType type, BaseUpcomingTileSettings tileSettings)
        {
            try
            {
                // Clear all scheduled notifications
                ClearScheduledNotifications(updater);

                List<ItemsOnDay> groupedByDay = GroupByDay(allUpcoming);

                bool veryFirst = true;
                DateTime today = todayInLocal;
                const int daysInAdvance = 5;
                for (int i = 0; i < daysInAdvance; i++, today = today.AddDays(1))
                {
                    DateTime todayAsUtc = DateTime.SpecifyKind(today, DateTimeKind.Utc);

                    // Remove any exams that are past "today"
                    for (int x = 0; x < groupedByDay.Count; x++)
                    {
                        groupedByDay[x].Items.RemoveAll(item => item is ViewItemExam && item.Date < today);
                        if (groupedByDay[x].Items.Count == 0)
                        {
                            groupedByDay.RemoveAt(x);
                            x--;
                        }
                    }

                    DateTime dateToStartFrom = tileSettings.GetDateToStartDisplayingOn(todayAsUtc);

                    // Remove any day groups that are older than the user's chosen max date range to display
                    while (groupedByDay.Count > 0 && groupedByDay[0].DateInUtc < dateToStartFrom)
                        groupedByDay.RemoveAt(0);

                    // If there's a today group
                    Dictionary<Guid, DateTime> identifiersAndEndTimes = new Dictionary<Guid, DateTime>();
                    var todayGroup = groupedByDay.FirstOrDefault(g => g.DateInUtc == todayAsUtc);
                    if (todayGroup != null)
                    {
                        // That means we'll need to check if there's exams/events, and if so, we need to update the tile
                        // after the event is over, rather than waiting till the day is over

                        // First we need to create a mapping of the item end times and their indexes, because we need to make sure that we're removing
                        // in order of the earliest end times (right now items are sorted by their start times, so not necessarily correct order based on end times)
                        // We also need to be aware that an event could potentially span multiple days... 
                        foreach (var item in todayGroup.Items)
                        {
                            // We ignore "all day" items which end at 11:59:59
                            DateTime endTime;
                            if (item is ViewItemExam && item.TryGetEndDateWithTime(out endTime) && endTime.TimeOfDay != new TimeSpan(23, 59, 59))
                            {
                                identifiersAndEndTimes[item.Identifier] = endTime;
                            }
                        }

                        // Remove any events that have already expired (so that the first update doesn't include them)
                        //if (veryFirst)
                        //{
                        //    while (true)
                        //    {
                        //        if (identifiersAndEndTimes.Count > 0)
                        //        {
                        //            DateTime minEndTime = identifiersAndEndTimes.Values.Min();

                        //            // If it's already expired, we remove it
                        //            if (minEndTime < DateTime.Now)
                        //            {
                        //                Guid[] identifiersToRemove = identifiersAndEndTimes.Where(p => p.Value == minEndTime).Select(p => p.Key).ToArray();
                        //                foreach (var id in identifiersToRemove)
                        //                {
                        //                    identifiersAndEndTimes.Remove(id);
                        //                }

                        //                // Remove those events
                        //                todayGroup.Items.RemoveAll(x => identifiersToRemove.Contains(x.Identifier));

                        //                // If we've removed all for that day
                        //                if (todayGroup.Items.Count == 0)
                        //                {
                        //                    // Remove the group
                        //                    groupedByDay.Remove(todayGroup);
                        //                }
                        //            }
                        //        }

                        //        break;
                        //    }
                        //}
                    }

                    DateTime deliveryTime = today;

                    do
                    {
                        if (deliveryTime.Date != today.Date)
                        {
                            // If an event ended up spanning multiple days causing delivery time to be past today, we just won't add it.
                            // In the future when we add support for multiple day events, we'll have to actually handle this... otherwise when the
                            // event expires, the tile won't update correctly on that future day.
                            break;
                        }

                        // On all items but the last, we use the friendly date
                        XmlDocument tile = GenerateUpcomingTileNotificationContent(
                            groupedByDay: groupedByDay,
                            todayAsUtc: todayAsUtc,
                            useFriendlyDates: i != daysInAdvance - 1,
                            type: type);

                        if (tile == null)
                        {
                            if (veryFirst)
                            {
                                updater.Clear();
                            }
                            return;
                        }

                        DateTime thisDeliveryTime = deliveryTime;

                        DateTime expirationTime = today.AddDays(1);

                        bool hasAdditionalOnThisDay = false;

                        // Pick off earliest ending time if there are any left
                        if (identifiersAndEndTimes.Count > 0)
                        {
                            DateTime minEndTime = identifiersAndEndTimes.Values.Min();
                            Guid[] identifiersToRemove = identifiersAndEndTimes.Where(p => p.Value == minEndTime).Select(p => p.Key).ToArray();
                            foreach (var id in identifiersToRemove)
                            {
                                identifiersAndEndTimes.Remove(id);
                            }

                            // Assign the expiration time and the next delivery time to be this end time
                            if (minEndTime > deliveryTime)
                            {
                                expirationTime = minEndTime;
                                deliveryTime = minEndTime; // Setting this for the NEXT notification
                            }

                            // Remove those events
                            todayGroup.Items.RemoveAll(x => identifiersToRemove.Contains(x.Identifier));

                            // If we've removed all for that day
                            if (todayGroup.Items.Count == 0)
                            {
                                // Remove the group
                                groupedByDay.Remove(todayGroup);
                            }

                            // Otherwise
                            else
                            {
                                // We have more on this day that we'll have to repeat the loop for
                                hasAdditionalOnThisDay = true;
                            }
                        }

                        // If we don't have additional on this day, we can remove today since we're done with it,
                        // which will ensure we correctly set expiration time if there's nothing else left
                        if (!hasAdditionalOnThisDay && todayGroup != null)
                        {
                            groupedByDay.Remove(todayGroup);
                        }

                        if (veryFirst || thisDeliveryTime < DateTime.Now.AddSeconds(5))
                        {
                            TileNotification currNotif = new TileNotification(tile);

                            // Only assign expiration time if we have no items left
                            if (groupedByDay.Count == 0)
                            {
                                currNotif.ExpirationTime = expirationTime;
                            }

                            updater.Update(currNotif);

                            veryFirst = false;
                        }
                        else
                        {
                            ScheduledTileNotification s = new ScheduledTileNotification(tile, thisDeliveryTime);

                            // Only assign expiration time if we have no items left
                            if (groupedByDay.Count == 0)
                            {
                                s.ExpirationTime = expirationTime;
                            }

                            updater.AddToSchedule(s);
                        }

                        if (!hasAdditionalOnThisDay)
                        {
                            break;
                        }

                    } while (true);
                }
            }

            catch (Exception ex)
            {
                if (!UWPExceptionHelper.TrackIfNotificationsIssue(ex, "Tiles"))
                {
                    throw ex;
                }
            }
        }

        private static IEnumerable<BaseDataItemHomeworkExam> getAllUpcomingFromExistingUpcomingList(IEnumerable<BaseDataItemHomeworkExam> existingUpcoming, DateTime todayAsUtc)
        {
            foreach (var item in existingUpcoming)
            {
                // Skip exams that are past today
                if (item is DataItemExam && DateTime.SpecifyKind(item.Date.Date, DateTimeKind.Utc) < todayAsUtc)
                    continue;

                // Add all homeworks, since a different today date doesn't change anything
                yield return item;
            }
        }

        internal class ItemsOnDay
        {
            public DateTime DateInUtc { get; set; }
            public List<BaseViewItemHomeworkExam> Items { get; private set; } = new List<BaseViewItemHomeworkExam>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupedByDay">This list will be modified, removing items that are past the date to start displaying on, which is beneficial to the next time this is called since those removed items would have never been applicable anyways.</param>
        /// <param name="todayAsUtc"></param>
        /// <param name="useFriendlyDates"></param>
        /// <param name="type"></param>
        /// <param name="tileSettings"></param>
        /// <returns></returns>
        internal static XmlDocument GenerateUpcomingTileNotificationContent(List<ItemsOnDay> groupedByDay, DateTime todayAsUtc, bool useFriendlyDates, UpcomingTileType type)
        {
            // If there aren't any
            if (groupedByDay.Count == 0)
                return null;

            var mediumAndWideContent = GenerateTileBindingContent(
                groupedItems: groupedByDay,
                todayAsUtc: todayAsUtc,
                useFriendlyDates: useFriendlyDates,
                useLargerHeader: false,
                maxLinesOfText: 10);

            var largeContent = GenerateTileBindingContent(
                groupedItems: groupedByDay,
                todayAsUtc: todayAsUtc,
                useFriendlyDates: useFriendlyDates,
                useLargerHeader: true,
                maxLinesOfText: 13);


            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = type == UpcomingTileType.PrimaryTile ? TileBranding.NameAndLogo : TileBranding.Auto,

                    TileMedium = new TileBinding()
                    {
                        DisplayName = type == UpcomingTileType.PrimaryTile ? "Planner" : null,
                        Content = mediumAndWideContent
                    },

                    TileWide = new TileBinding()
                    {
                        Content = mediumAndWideContent
                    },

                    TileLarge = new TileBinding()
                    {
                        Content = largeContent
                    },

                    LockDetailedStatus1 = GenerateUpcomingDateText(groupedByDay.First().DateInUtc, todayAsUtc, useFriendlyDates)
                }
            };

            var firstGroup = groupedByDay.First();

            if (firstGroup.Items.Count > 1)
                content.Visual.LockDetailedStatus1 += " - " + firstGroup.Items.Count + " Items";

            content.Visual.LockDetailedStatus2 = trim(firstGroup.Items[0].Name, 255);

            if (firstGroup.Items.Count >= 2)
                content.Visual.LockDetailedStatus3 = trim(firstGroup.Items[1].Name, 255);

            //XmlDocument doc = new XmlDocument();
            //string stringContent = content.GetContent();
            //doc.LoadXml(stringContent);
            //return doc;
            return content.GetXml();
        }

        private static TileBindingContentAdaptive GenerateTileBindingContent(List<ItemsOnDay> groupedItems, DateTime todayAsUtc, bool useFriendlyDates, bool useLargerHeader, int maxLinesOfText)
        {
            TileBindingContentAdaptive answer = new TileBindingContentAdaptive();

            int linesAdded = 0;
            bool firstGroup = true;

            // TODO: Need to filter upcoming items based on tile skip settings (and also remove exams that already happened)

            foreach (var group in groupedItems)
            {
                // We know we'll be adding the group header (like "Tomorrow") and at least one homework
                linesAdded += 2;

                // If it's not the first group, we also add a spacer line of text
                if (!firstGroup)
                    linesAdded++;

                // If there's no space for that, we stop
                if (linesAdded > maxLinesOfText)
                    break;

                // If not first group, we add the spacer line
                if (!firstGroup)
                    answer.Children.Add(new AdaptiveText());

                string displayDate = GenerateUpcomingDateText(group.DateInUtc, todayAsUtc, useFriendlyDates);

                // Add the group and first item
                answer.Children.Add(new AdaptiveGroup()
                {
                    Children =
                    {
                        new AdaptiveSubgroup()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = displayDate,
                                    HintStyle = useLargerHeader ? AdaptiveTextStyle.Body : AdaptiveTextStyle.Caption
                                },

                                GenerateTileTextForUpcomingItem(group.Items[0])
                            }
                        }
                    }
                });

                firstGroup = false;

                // And then add the subsequent items (skip first since already added)
                foreach (var item in group.Items.Skip(1))
                {
                    linesAdded++;

                    if (linesAdded > maxLinesOfText)
                        break;

                    answer.Children.Add(GenerateTileTextForUpcomingItem(item));
                }
            }

            return answer;
        }

        private static string GenerateUpcomingDateText(DateTime dateInUtc, DateTime todayAsUtc, bool useFriendlyDates)
        {
            if (useFriendlyDates)
                return ToFriendlyDate(dateInUtc, todayAsUtc);
            else
                return trim(dateInUtc.DayOfWeek.ToString(), 3).ToLower() + ", " + trim(dateInUtc.ToString("MMM"), 3).ToLower() + " " + dateInUtc.Day.ToString();
        }

        private static AdaptiveText GenerateTileTextForUpcomingItem(BaseViewItemHomeworkExam item)
        {
            return new AdaptiveText()
            {
                Text = TrimString(item.Name, 200),
                HintStyle = AdaptiveTextStyle.CaptionSubtle
            };
        }

        /// <summary>
        /// Guaranteed that data won't be null
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Should always return an initialized list.</returns>
        private static async Task<List<BaseViewItemHomeworkExam>> getAllUpcomingBlocking(AccountDataItem account, AccountDataStore data, DateTime todayAsUtc, BaseUpcomingTileSettings tileSettings)
        {
            var currSemesterId = account.CurrentSemesterId;
            if (currSemesterId == Guid.Empty)
                return new List<BaseViewItemHomeworkExam>();

            ScheduleViewItemsGroup scheduleViewGroup;
            try
            {
                scheduleViewGroup = await ScheduleViewItemsGroup.LoadAsync(account.LocalAccountId, account.CurrentSemesterId, trackChanges: true, includeWeightCategories: false);
            }
            catch
            {
                // If semester not found
                return new List<BaseViewItemHomeworkExam>();
            }

            DateTime dateToStartDisplayingFrom = DateTime.SpecifyKind(tileSettings.GetDateToStartDisplayingOn(todayAsUtc), DateTimeKind.Utc);

            var agendaViewGroup = await AgendaViewItemsGroup.LoadAsync(account.LocalAccountId, scheduleViewGroup.Semester, DateTime.SpecifyKind(todayAsUtc, DateTimeKind.Local), trackChanges: true);

            // We're not going to worry about locking changes while we enumerate, since if collection changes while we're enumerating, there'll be a
            // new incoming reset request anyways

            // Agenda view group doesn't sort, so we have to sort it
            return agendaViewGroup.Items.Where(
                i => i.Date.Date >= dateToStartDisplayingFrom
                && ((tileSettings.ShowHomework && i is ViewItemHomework) || (tileSettings.ShowExams && i is ViewItemExam))
                ).OrderBy(i => i).ToList();
        }



        internal static string TrimString(string str, int maxLength)
        {
            str = str.Trim();

            if (str.Length > maxLength)
                str = str.Substring(0, maxLength);

            return str;
        }

        /// <summary>
        /// Returns "x Days Ago", "Today", "Tomorrow", "Two Days", "This Xx", "Next Xx"
        /// </summary>
        /// <param name="dueDate"></param>
        /// <returns></returns>
        internal static string ToFriendlyDate(DateTime dueDate, DateTime relativeTo)
        {
            dueDate = dueDate.Date;
            relativeTo = relativeTo.Date;

            if (dueDate < relativeTo)
                return ((relativeTo - dueDate).Days == 1) ? LocalizedResources.Common.GetRelativeDateYesterday() : LocalizedResources.Common.GetRelativeDateXDaysAgo((relativeTo - dueDate).Days);

            else if (dueDate == relativeTo)
                return LocalizedResources.Common.GetRelativeDateToday();

            else if (dueDate == relativeTo.AddDays(1))
                return LocalizedResources.Common.GetRelativeDateTomorrow();

            else if (dueDate == relativeTo.AddDays(2))
                return LocalizedResources.Common.GetRelativeDateInTwoDays();

            else if (dueDate < relativeTo.AddDays(7))
                return LocalizedResources.Common.GetRelativeDateThisDayOfWeek(dueDate.DayOfWeek);

            else if (dueDate < relativeTo.AddDays(14))
                return LocalizedResources.Common.GetRelativeDateNextDayOfWeek(dueDate.DayOfWeek);

            // Aug 17
            else if (dueDate < DateTime.MaxValue)
                return dueDate.ToString("MMM d");

            return null;
        }

        internal static string trim(string original, int length)
        {
            if (original.Length > length)
                return original.Substring(0, length);

            return original;
        }

        /// <summary>
        /// Unpins all secondary tiles for an account, useful for when account is deleted
        /// </summary>
        /// <param name="localAccountId"></param>
        /// <returns></returns>
        public static async Task UnpinAllTilesForAccount(Guid localAccountId)
        {
            try
            {
                foreach (var tile in await GetAllTilesForAccountAsync(localAccountId))
                {
                    try
                    {
                        await tile.RequestDeleteAsync();
                    }

                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static async Task<List<SecondaryTile>> GetAllTilesForAccountAsync(Guid localAccountId)
        {
            List<SecondaryTile> answer = new List<SecondaryTile>();

            try
            {
                foreach (var tile in await SecondaryTile.FindAllAsync())
                {
                    try
                    {
                        TileIdInfo info;

                        if (TileIdInfo.TryParse(tile.TileId, out info))
                        {
                            if (info.MatchesLocalAccountId(localAccountId))
                                answer.Add(tile);
                        }
                    }

                    catch (Exception ex)
                    {
                        TelemetryExtension.Current?.TrackException(ex);
                    }
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            return answer;
        }

        internal class ItemTileIdInfo : TileIdInfo
        {
            public ItemTileIdInfo(Guid localAccountId, Guid itemId) : base(localAccountId)
            {
                HashedItemId = itemId.GetHashCode();
            }

            public ItemTileIdInfo() : base() { }

            public int HashedItemId { get; private set; }

            protected override bool TryParse(string tileId)
            {
                int splitterIndex = tileId.LastIndexOf('_');

                if (splitterIndex == -1)
                    return false;
                
                // Parse the base info
                if (!base.TryParse(tileId.Substring(0, splitterIndex)))
                    return false;

                // Parse the item ID
                int hashedItemId;

                if (!int.TryParse(tileId.Substring(splitterIndex + 1), out hashedItemId))
                    return false;

                HashedItemId = hashedItemId;

                return true;
            }

            /// <summary>
            /// Produces the serialized TileId string. Max 25 chars.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                // Max 13 chars
                return base.ToString()

                    // Accepted divider
                    + "_"

                    // Max 11 chars
                    + HashedItemId.ToString();
            }
        }

        internal class TileIdInfo
        {
            public TileIdInfo(Guid localAccountId)
            {
                HashedLocalAccountId = localAccountId.GetHashCode();
            }

            public TileIdInfo() { }

            public int HashedLocalAccountId { get; private set; }

            public TileType Type { get; set; }

            public bool MatchesLocalAccountId(Guid localAccountId)
            {
                return HashedLocalAccountId == localAccountId.GetHashCode();
            }

            protected virtual bool TryParse(string tileId)
            {
                string[] parameters = tileId.Split('_');

                if (parameters.Length < 2)
                    return false;

                int hashedLocalAccountId;

                if (!int.TryParse(parameters[0], out hashedLocalAccountId))
                    return false;

                TileType type;

                if (!Enum.TryParse(parameters[1], out type))
                    return false;

                HashedLocalAccountId = hashedLocalAccountId;
                Type = type;

                return true;
            }

            public static bool TryParse<T>(string tileId, out T info) where T : TileIdInfo, new()
            {
                var answer = new T();

                if (answer.TryParse(tileId))
                {
                    info = answer;
                    return true;
                }

                else
                {
                    info = null;
                    return false;
                }
            }

            /// <summary>
            /// Produces the serialized TileId format. Max 13 chars in length
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return

                    // Max 11 chars in length
                    HashedLocalAccountId.ToString()

                    // Accepted divider
                    + "_"

                    // 1 char in length (this format prints enum as decimal value)
                    + Type.ToString("d");
            }
        }

        internal enum TileType
        {
            // WARNING: Order matters (since I depend on the numerical value of the enums)
            Schedule,
            Class,
            QuickAdd,
        }

        private static bool IsForAccount(QueryString tileIdQueryString, Guid localAccountId)
        {
            string account;
            Guid parsedId;

            if (tileIdQueryString.TryGetValue(KEY_ACCOUNT, out account) && Guid.TryParse(account, out parsedId) && parsedId == localAccountId)
                return true;

            return false;
        }
        

        internal static void ClearScheduledNotifications(TileUpdater updater)
        {
            try
            {
                foreach (var scheduled in updater.GetScheduledTileNotifications())
                    updater.RemoveFromSchedule(scheduled);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
