using System;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlanneriOS.Helpers;

namespace PowerPlanneriOS.Extensions
{
    public class iOSTilesExtension : TilesExtension
    {
        public override Task UnpinAllTilesForAccount(Guid localAccountId)
        {
            // Not implemented on iOS
            return Task.FromResult(true);
        }

        public override async Task UpdatePrimaryTileNotificationsAsync()
        {
            // This gets called upon account log off
            await WidgetsHelper.UpdateAllWidgetsAsync();
        }

        public override async Task UpdateTileNotificationsForAccountAsync(AccountDataItem account, AccountDataStore data)
        {
            await WidgetsHelper.UpdateAllWidgetsAsync();
        }
    }
}