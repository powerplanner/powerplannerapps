using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.DataLayer.TileSettings;
using PowerPlannerAppDataLibrary.Exceptions;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerUWP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsUniversal;
using Windows.Data.Xml.Dom;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using static PowerPlannerUWP.TileHelpers.TileHelper;

namespace PowerPlannerUWP.TileHelpers
{
    public static class ClassTileHelper
    {
        public static bool IsPinned(Guid localAccountId, Guid classId)
        {
            try
            {
                return SecondaryTile.Exists(GenerateTileId(localAccountId, classId));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return false;
            }
        }

        private static async Task<SecondaryTile> GetTile(Guid localAccountId, Guid classId)
        {
            SecondaryTile tile = (await SecondaryTile.FindAllAsync()).FirstOrDefault(i => i.TileId.Equals(GenerateTileId(localAccountId, classId)));

            return tile;
        }

        public static async Task UnpinTile(Guid localAccountId, Guid classId)
        {
            try
            {
                SecondaryTile tile = await GetTile(localAccountId, classId);

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
        public static async Task PinTileAsync(AccountDataItem account, AccountDataStore data, Guid classId, string className, Color classColor)
        {
            string args = new ViewClassArguments()
            {
                LocalAccountId = account.LocalAccountId,
                ItemId = classId

            }.SerializeToString();

            // Display name is name of the class
            // Tile background is color of the class

            SecondaryTile tile = new SecondaryTile(GenerateTileId(account.LocalAccountId, classId), GetTrimmedClassName(className), args, new Uri("ms-appx:///Assets/Square150x150Logo.png"), TileSize.Default);
            tile.VisualElements.BackgroundColor = classColor;
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

            await UpdateTileAsync(tile, account, data, classId);
        }

        /// <summary>
        /// Updates the class tile, if it exists.
        /// </summary>
        /// <param name="account"></param>
        /// <param name="data"></param>
        /// <param name="classId"></param>
        /// <returns></returns>
        public static async Task UpdateTileAsync(AccountDataItem account, AccountDataStore data, Guid classId)
        {
            try
            {
                SecondaryTile tile = (await SecondaryTile.FindAllAsync()).FirstOrDefault(i => i.TileId.Equals(GenerateTileId(account.LocalAccountId, classId)));

                if (tile == null)
                    return;

                await UpdateTileAsync(tile, account, data, classId);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private static async Task UpdateTileAsync(SecondaryTile tile, AccountDataItem account, AccountDataStore data, Guid classId)
        {
            try
            {
                DateTime todayInLocal = DateTime.Today;

                // Get the class tile settings
                ClassTileSettings settings = await account.GetClassTileSettings(classId);

                ClassData classData = await LoadDataAsync(data, classId, DateTime.SpecifyKind(todayInLocal, DateTimeKind.Utc), settings);

                // If classData was null, that means the class wasn't found, so we should delete the tile
                if (classData == null)
                {
                    await tile.RequestDeleteAsync();
                    return;
                }

                bool changed = false;

                string desiredName = GetTrimmedClassName(classData.Class.Name);

                Color desiredColor;

                if (settings.CustomColor != null)
                    desiredColor = ColorTools.GetColor(settings.CustomColor);
                else
                    desiredColor = ColorTools.GetColor(classData.Class.Color);

                if (!tile.DisplayName.Equals(desiredName))
                {
                    changed = true;
                    tile.DisplayName = desiredName;
                }

                if (!tile.VisualElements.BackgroundColor.Equals(desiredColor))
                {
                    changed = true;
                    tile.VisualElements.BackgroundColor = desiredColor;
                }

                if (changed)
                    await tile.UpdateAsync();


                var updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tile.TileId);

                UpdateUpcomingTile(updater, classData.AllUpcoming, todayInLocal, UpcomingTileType.ClassTile, settings);
            }

            catch (Exception ex)
            {
                if (!UWPExceptionHelper.TrackIfNotificationsIssue(ex, "Tiles"))
                {
                    throw ex;
                }
            }
        }

        public static async Task<XmlDocument> GetCurrentTileNotificationContentAsync(AccountDataItem forAccount, Guid classId)
        {
            try
            {
                ClassTileSettings settings = await forAccount.GetClassTileSettings(classId);

                if (settings.IsDisabled())
                {
                    return null;
                }

                DateTime todayInLocal = DateTime.Today;

                ClassData data = await LoadDataAsync(await AccountDataStore.Get(forAccount.LocalAccountId), classId, DateTime.SpecifyKind(todayInLocal, DateTimeKind.Utc), settings);

                // That means the class was deleted, but we'll just return null here
                if (data == null)
                    return null;

                List<ItemsOnDay> groupedByDay = GroupByDay(data.AllUpcoming);

                return GenerateUpcomingTileNotificationContent(groupedByDay, DateTime.SpecifyKind(todayInLocal, DateTimeKind.Utc), true, UpcomingTileType.ClassTile);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return null;
            }
        }

        private static string GetTrimmedClassName(string className)
        {
            return TrimString(className, 256);
        }

        private class ClassData
        {
            public ViewItemClass Class { get; set; }

            public List<ViewItemTaskOrEvent> AllUpcoming { get; set; }
        }

        private static Task<ClassData> LoadDataAsync(AccountDataStore data, Guid classId, DateTime todayAsUtc, ClassTileSettings settings)
        {
            try
            {
                return Task.Run(delegate
                {
                    return LoadDataBlocking(data, classId, todayAsUtc, settings);
                });
            }
            catch (SemesterNotFoundException)
            {
                return Task.FromResult<ClassData>(null);
            }
        }

        private static async Task<ClassData> LoadDataBlocking(AccountDataStore data, Guid classId, DateTime todayAsUtc, ClassTileSettings settings)
        {
            DateTime dateToStartDisplayingFrom = DateTime.SpecifyKind(settings.GetDateToStartDisplayingOn(todayAsUtc), DateTimeKind.Local);

            Guid semesterId = Guid.Empty;

            // We lock the outside, since we are allowing trackChanges on the view items groups (so we have a chance of loading a cached one)... and since we're on a background thread, the lists inside the
            // view items groups could change while we're enumerating, hence throwing an exception. So we lock it to ensure this won't happen, and then we return a copy of the items that we need.
            using (await Locks.LockDataForReadAsync())
            {
                // First we need to obtain the semester id
                var c = data.TableClasses.FirstOrDefault(i => i.Identifier == classId);

                if (c == null)
                    return null;

                semesterId = c.UpperIdentifier;
            }

            // We need all classes loaded, to know what time the end of day is
            var scheduleViewItemsGroup = await ScheduleViewItemsGroup.LoadAsync(data.LocalAccountId, semesterId, trackChanges: true, includeWeightCategories: false);

            var classViewItemsGroup = await ClassViewItemsGroup.LoadAsync(
                localAccountId: data.LocalAccountId,
                classId: classId,
                today: DateTime.SpecifyKind(todayAsUtc, DateTimeKind.Local),
                viewItemSemester: scheduleViewItemsGroup.Semester,
                includeWeights: false);
            classViewItemsGroup.LoadTasksAndEvents();
            await classViewItemsGroup.LoadTasksAndEventsTask;

            List<ViewItemTaskOrEvent> copied;

            using (await classViewItemsGroup.DataChangeLock.LockForReadAsync())
            {
                // Class view group sorts the items, so no need to sort
                copied = classViewItemsGroup.Class.TasksAndEvents.Where(i => i.Date.Date >= dateToStartDisplayingFrom).ToList();
            }

            return new ClassData()
            {
                Class = classViewItemsGroup.Class,
                AllUpcoming = copied
            };
        }

        private static string GenerateTileId(Guid localAccountId, Guid classId)
        {
            return new ItemTileIdInfo(localAccountId, classId)
            {
                Type = TileType.Class

            }.ToString();
        }

        public static async Task UpdateTilesAsync(AccountDataItem account, AccountDataStore data, IEnumerable<SecondaryTile> tilesForAccount)
        {
            foreach (var tile in tilesForAccount)
            {
                var args = ArgumentsHelper.Parse(tile.Arguments) as ViewClassArguments;

                if (args != null)
                {
                    await UpdateTileAsync(tile, account, data, args.ItemId);
                }
            }
        }
    }
}
