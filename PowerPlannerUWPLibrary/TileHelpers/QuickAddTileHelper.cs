using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.StartScreen;
using static PowerPlannerUWPLibrary.TileHelpers.TileHelper;

namespace PowerPlannerUWPLibrary.TileHelpers
{
    public static class QuickAddTileHelper
    {
        public static bool IsPinned(Guid localAccountId)
        {
            try
            {
                return SecondaryTile.Exists(GenerateTileId(localAccountId));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                return false;
            }
        }

        private static async Task<SecondaryTile> GetTile(Guid localAccountId)
        {
            SecondaryTile tile = (await SecondaryTile.FindAllAsync()).FirstOrDefault(i => i.TileId.Equals(GenerateTileId(localAccountId)));

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
        /// Pins the secondary tile
        /// </summary>
        /// <param name="account"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task PinTileAsync(Guid localAccountId)
        {
            string args = new QuickAddArguments()
            {
                LocalAccountId = localAccountId

            }.SerializeToString();

            SecondaryTile tile = new SecondaryTile(GenerateTileId(localAccountId), "Quick Add", args, new Uri("ms-appx:///Assets/QuickAddTile/Square150x150Logo.png"), TileSize.Default);
            tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/QuickAddTile/Square71x71Logo.png");
            tile.VisualElements.ShowNameOnSquare150x150Logo = true;

            if (!await tile.RequestCreateAsync())
                return;
        }

        private static string GenerateTileId(Guid localAccountId)
        {
            return new QuickAddTileIdInfo(localAccountId).ToString();
        }



        internal class QuickAddTileIdInfo : TileIdInfo
        {
            public QuickAddTileIdInfo(Guid localAccountId) : base(localAccountId)
            {
                base.Type = TileType.QuickAdd;
            }

            public QuickAddTileIdInfo() : base() { }

            public int HashedItemId { get; private set; }
        }
    }
}
