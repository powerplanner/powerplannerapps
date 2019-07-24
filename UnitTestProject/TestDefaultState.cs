using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerUWPLibrary.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject
{
    [TestClass]
    public class TestDefaultState
    {
        [TestMethod]
        public async Task EnsureNoAccounts()
        {
            LoginViewModel accountsViewModel = await LoginViewModel.Load();

            Assert.AreEqual(0, accountsViewModel.Accounts.Count);
        }

        [TestMethod]
        public async Task TestCreatingAccount()
        {
            AccountDataItem account = await AccountsManager.CreateAccount("andrewbares", "andrew", null, 0, 0, true, true, true);

            LoginViewModel accountsViewModel = await LoginViewModel.Load();

            Assert.AreEqual(1, accountsViewModel.Accounts.Count);
            Assert.AreEqual("andrewbares", accountsViewModel.Accounts.First().Username);

            account = await AccountsManager.GetOrLoad(account.LocalAccountId);

            Assert.AreEqual("andrew", account.Password);
        }
    }
}
