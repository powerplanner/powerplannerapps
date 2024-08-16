using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerUWP.TileHelpers;

namespace PowerPlannerUWP.Extensions
{
    public class UWPTilesExtension : TilesExtension
    {
        public override Task UnpinAllTilesForAccount(Guid localAccountId)
        {
            return TileHelper.UnpinAllTilesForAccount(localAccountId);
        }

        public override Task UpdatePrimaryTileNotificationsAsync()
        {
            if (TileHelper.AreLiveTilesSupported)
            {
                return TileHelper.UpdatePrimaryTileNotificationsAsync();
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public override Task UpdateTileNotificationsForAccountAsync(AccountDataItem account, AccountDataStore data)
        {
            if (TileHelper.AreLiveTilesSupported)
            {
                return TileHelper.UpdateTileNotificationsForAccountAsync(account, data);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }
}
