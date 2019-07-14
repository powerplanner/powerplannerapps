using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject.BaseTestClasses
{
    public abstract class BaseTestAccount
    {
        public Guid LocalAccountId { get; private set; }

        public AccountDataStore DataStore
        {
            get; private set;
        }

        [TestInitialize]
        public async Task Initialize()
        {
            AccountDataItem account = await AccountsManager.CreateAccount("andrewbares", "andrew", 0, 0, true, true, true);

            LocalAccountId = account.LocalAccountId;

            await InitializeAfterAccount();
        }

        protected virtual async Task InitializeAfterAccount()
        {
            DataStore = await AccountDataStore.Get(LocalAccountId);
        }
    }
}
