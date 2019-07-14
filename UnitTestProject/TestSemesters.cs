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
    public class TestSemesters : BaseTestAccountWithYears
    {
        [TestMethod]
        public async Task TestAddingSemester()
        {
            ViewModelYearsAndSemesters viewModel = await ViewModelYearsAndSemesters.LoadAsync(LocalAccountId);

            var years = viewModel.School.Years;

            var freshman = viewModel.School.Years[0];
            var sophomore = viewModel.School.Years[1];
            var junior = viewModel.School.Years[2];
            var senior = viewModel.School.Years[3];

            DataChanges changes = new DataChanges();

            changes.Add(new DataItemSemester()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = freshman.Identifier,
                Name = "Fall (Freshman)"
            });

            await DataStore.ProcessLocalChanges(changes);

            Assert.AreEqual(1, freshman.Semesters.Count);
            Assert.AreEqual(freshman.Identifier, freshman.Semesters.First().Year.Identifier);
            Assert.AreEqual("Fall (Freshman)", freshman.Semesters.First().Name);
        }
    }
}
