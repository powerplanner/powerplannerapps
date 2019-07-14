using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.App.Job;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.SyncLayer;

namespace PowerPlannerAndroid.Services
{
    [Service(Exported = true, Permission = PERMISSION)]
    public class MySyncService : MyBaseJobService
    {
        protected override async Task PerformWorkAsync(JobParameters @params)
        {
            long accountId = @params.Extras.GetLong("AccountId");
            if (accountId == 0)
            {
                return;
            }

            // First try to grab cached
            var account = AccountsManager.GetCurrentlyLoadedAccounts().FirstOrDefault(i => i.AccountId == accountId);
            if (account == null)
            {
                account = (await AccountsManager.GetAllAccounts()).FirstOrDefault(i => i.AccountId == accountId);
            }
            if (account == null)
            {
                return;
            }

            var syncResult = await Sync.SyncAccountAsync(account);

            if (syncResult != null && syncResult.SaveChangesTask != null)
            {
                await syncResult.SaveChangesTask.WaitForAllTasksAsync();
            }
        }
    }
}