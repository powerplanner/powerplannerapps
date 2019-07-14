using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace PowerPlannerUWP.WindowHosts
{
    public abstract class LoggedInWindowHost : WindowHost
    {
        public Guid LocalAccountId { get; private set; }

        public LoggedInWindowHost(Guid localAccountId)
        {
            LocalAccountId = localAccountId;

            // Watch account deletions so we can close windows if deleted
            AccountsManager.OnAccountDeleted += AccountsManager_OnAccountDeleted;
        }

        private void AccountsManager_OnAccountDeleted(object sender, Guid e)
        {
            // If this account was deleted, close the window
            if (LocalAccountId == e)
            {
                Close();
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            AccountsManager.OnAccountDeleted -= AccountsManager_OnAccountDeleted;
        }
    }
}
