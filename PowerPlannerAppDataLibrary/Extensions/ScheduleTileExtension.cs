using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class ScheduleTileExtension
    {
        public static ScheduleTileExtension Current { get; set; }

        public abstract bool IsPinned(Guid localAccountId);
        public abstract Task UnpinTile(Guid localAccountId);

        /// <summary>
        /// Pins and updates the secondary tile
        /// </summary>
        /// <param name="account"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract Task PinTile(AccountDataItem account, AccountDataStore data);

        public abstract Task UpdateScheduleTile(AccountDataItem account, AccountDataStore data);
    }
}
