using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class AppointmentsExtension
    {
        public static AppointmentsExtension Current { get; set; }

        public abstract void ResetAll(AccountDataItem account, AccountDataStore dataStore);
        public abstract Task GetTaskForAllCompleted();
        public abstract void ResetAllIfNeeded(AccountDataItem account, AccountDataStore dataStore);
        public abstract UpdateResponse Update(AccountDataItem account, AccountDataStore dataStore, DataChangedEvent dataChangedEvent);
        public abstract Task DeleteAsync(Guid localAccountId);

        public class UpdateResponse
        {
            public bool NeedsAccountToBeSaved { get; set; }
        }
    }
}
