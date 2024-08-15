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
            // This gets called upon account log off or account change, and should run on current thread.
            await WidgetsHelper.UpdateAllWidgetsAsync();
        }

        public override async Task UpdateTileNotificationsForAccountAsync(AccountDataItem account, AccountDataStore data)
        {
            // This is called frequently (when any items are edited) and should run on a background thread.
            await Task.Run(WidgetsHelper.UpdateAllWidgetsAsync);
        }
    }
}