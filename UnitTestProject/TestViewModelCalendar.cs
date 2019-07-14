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
    public class TestViewModelCalendar : BaseTestAccountWithClasses
    {
        [TestMethod]
        public async Task TestViewModelCalendar_EdgeCase()
        {
            DataChanges changes = new DataChanges();

            changes.Add(new DataItemHomework()
            {
                Name = "New Year 2016",
                Date = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Identifier = Guid.NewGuid(),
                UpperIdentifier = base.CurrentClassId,
                Details = ""
            });

            await base.DataStore.ProcessLocalChanges(changes);

            // TODO
            //var viewModel = await ViewModelCalendar.LoadAsync(base.LocalAccountId, base.CurrentSemesterId);

            //Assert.AreEqual(0, viewModel.Items.Count, "Should start empty");

            //await viewModel.ApplyFilterAsync(new DateTime(2015, 10, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            //Assert.AreEqual(1, viewModel.Items.Count, "After first filter");

            //await viewModel.ApplyFilterAsync(new DateTime(2015, 11, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2016, 2, 1, 0, 0, 0, DateTimeKind.Utc));

            //Assert.AreEqual(1, viewModel.Items.Count, "After second filter");
        }

        [TestMethod]
        public async Task TestViewModelCalendar_Normal()
        {
            DataChanges changes = new DataChanges();

            changes.Add(new DataItemHomework()
            {
                Name = "New Year 2016",
                Date = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Identifier = Guid.NewGuid(),
                UpperIdentifier = base.CurrentClassId,
                Details = ""
            });

            changes.Add(new DataItemHomework()
            {
                Identifier = Guid.NewGuid(),
                Name = "Birthday 2015",
                Date = new DateTime(2015, 9, 25, 0, 0, 0, DateTimeKind.Utc),
                Details = "Congrats!",
                UpperIdentifier = base.CurrentClassId
            });

            changes.Add(new DataItemHomework()
            {
                Identifier = Guid.NewGuid(),
                Name = "Afterparty 2015",
                Date = new DateTime(2015, 9, 25, 0, 0, 0, DateTimeKind.Utc),
                Details = "Celebrate!",
                UpperIdentifier = base.CurrentClassId
            });

            await base.DataStore.ProcessLocalChanges(changes);

            // TODO: Needs updating
            Assert.Fail("Needs updating");
            //var viewModel = await ViewModelCalendar.LoadAsync(base.LocalAccountId, base.CurrentSemesterId);

            //Assert.AreEqual(0, viewModel.Items.Count, "Should start empty");

            //await viewModel.ApplyFilterAsync(new DateTime(2015, 7, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2015, 11, 1, 0, 0, 0, DateTimeKind.Utc));

            //Assert.AreEqual(2, viewModel.Items.Count, "After first filter");
            //Assert.IsTrue(viewModel.Items.Any(i => i.Name.Equals("Birthday 2015")));
            //Assert.IsTrue(viewModel.Items.Any(i => i.Name.Equals("Afterparty 2015")));
        }
    }
}
