using System;
using System.Threading.Tasks;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlanneriOS.Helpers;

namespace PowerPlanneriOS.Extensions
{
    public class iOSScheduleTileExtension : ScheduleTileExtension
    {
        public override bool IsPinned(Guid localAccountId)
        {
            // Isn't relevant for iOS
            return true;
        }

        public override Task PinTile(AccountDataItem account, AccountDataStore data)
        {
            // Isn't relevant for iOS
            return Task.FromResult(true);
        }

        public override Task UnpinTile(Guid localAccountId)
        {
            // Isn't relevant for iOS
            return Task.FromResult(true);
        }

        public override async Task UpdateScheduleTile(AccountDataItem account, AccountDataStore data)
        {
            await WidgetsHelper.UpdateScheduleWidget();
        }
    }
}