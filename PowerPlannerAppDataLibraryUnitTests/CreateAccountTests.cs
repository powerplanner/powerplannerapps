using BareMvvm.Core.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibraryUnitTests
{
    [TestClass]
    public class CreateAccountTests
    {
        [TestInitialize]
        public async Task InitializeAsync()
        {
            await DummyPowerPlannerApp.InitializeAndLaunchAsync();
        }

        [TestMethod]
        public async Task TestCreateOfflineAccountAsync()
        {
            var mainWindowViewModel = DummyPowerPlannerApp.Current.GetMainWindowViewModel();

            Assert.IsNotNull(mainWindowViewModel);

            Assert.IsInstanceOfType(mainWindowViewModel.GetFinalContent(), typeof(WelcomeViewModel));

            var welcomeViewModel = mainWindowViewModel.GetFinalContent() as WelcomeViewModel;

            // Press "Create account" button
            welcomeViewModel.CreateAccount();

            var createAccountViewModel = GetAndAssertCurrentPage<CreateAccountViewModel>();
        }

        private static T GetAndAssertCurrentPage<T>()
            where T : BaseViewModel
        {
            AssertPageVisible(typeof(T));

            return DummyPowerPlannerApp.Current.GetMainWindowViewModel().GetFinalContent() as T;
        }

        private static void AssertPageVisible(Type expectedPageType)
        {
            var mainWindowViewModel = DummyPowerPlannerApp.Current.GetMainWindowViewModel();

            Assert.IsInstanceOfType(mainWindowViewModel.GetFinalContent(), expectedPageType);
        }
    }
}
