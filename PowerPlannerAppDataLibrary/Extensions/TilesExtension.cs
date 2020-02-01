using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class TilesExtension
    {
        public static TilesExtension Current { get; set; }

        public abstract Task UpdateTileNotificationsForAccountAsync(AccountDataItem account, AccountDataStore data);

        public abstract Task UpdatePrimaryTileNotificationsAsync();

        public abstract Task UnpinAllTilesForAccount(Guid localAccountId);
    }
}
