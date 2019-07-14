using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerUWPLibrary.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestProject.BaseTestClasses;

namespace UnitTestProject
{
    [TestClass]
    public class TestAdding : BaseTestAccount
    {
        [TestMethod]
        public async Task TestAdding_DateCreated()
        {
            DataChanges changes = new DataChanges();

            // Add multiple at once
            changes.Add(new DataItemYear()
            {
                Identifier = Guid.NewGuid(),
                Name = "Freshman"
            });

            changes.Add(new DataItemYear()
            {
                Identifier = Guid.NewGuid(),
                Name = "Sophomore"
            });

            await DataStore.ProcessLocalChanges(changes);


            ViewModelYearsAndSemesters viewModel = await ViewModelYearsAndSemesters.LoadAsync(LocalAccountId);

            var freshman = viewModel.School.Years[0];
            var sophomore = viewModel.School.Years[1];

            Assert.AreEqual("Freshman", freshman.Name);
            Assert.AreEqual("Sophomore", sophomore.Name);

            // Make sure the second item is 1 tick after the first
            Assert.AreEqual(freshman.DateCreated.AddTicks(1), sophomore.DateCreated);
        }
    }
}
