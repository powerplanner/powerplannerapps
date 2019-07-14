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
    public class TestViewModelAgenda : BaseTestAccountWithClasses
    {
        [TestMethod]
        public async System.Threading.Tasks.Task TestViewModelAgenda_Normal()
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
                UpperIdentifier = base.CurrentClassId,
                PercentComplete = 0.3
            });

            changes.Add(new DataItemHomework()
            {
                Identifier = Guid.NewGuid(),
                Name = "Afterparty 2015",
                Date = new DateTime(2015, 9, 25, 0, 0, 0, DateTimeKind.Utc),
                Details = "Celebrate!",
                UpperIdentifier = base.CurrentClassId
            });

            changes.Add(new DataItemHomework()
            {
                Identifier = Guid.NewGuid(),
                Name = "Recovery",
                Date = new DateTime(2015, 9, 26, 0, 0, 0, DateTimeKind.Utc),
                Details = "",
                UpperIdentifier = base.CurrentClassId
            });

            // SHOULDN'T BE INCLUDED
            changes.Add(new DataItemHomework()
            {
                Identifier = Guid.NewGuid(),
                Name = "Completed in the future",
                Date = new DateTime(2015, 9, 28, 0, 0, 0, DateTimeKind.Utc),
                Details = "",
                UpperIdentifier = base.CurrentClassId,
                PercentComplete = 1
            });

            changes.Add(new DataItemHomework()
            {
                Identifier = Guid.NewGuid(),
                Name = "In the past",
                Date = new DateTime(2015, 9, 23, 0, 0, 0, DateTimeKind.Utc),
                Details = "",
                UpperIdentifier = base.CurrentClassId
            });

            // SHOULDN'T BE INCLUDED
            changes.Add(new DataItemHomework()
            {
                Identifier = Guid.NewGuid(),
                Name = "Completed in the past",
                Date = new DateTime(2015, 9, 24, 0, 0, 0, DateTimeKind.Utc),
                Details = "",
                UpperIdentifier = base.CurrentClassId,
                PercentComplete = 1
            });

            // SHOULDN'T BE INCLUDED
            changes.Add(new DataItemExam()
            {
                Identifier = Guid.NewGuid(),
                Name = "Exam in the past",
                Date = new DateTime(2015, 9, 24, 0, 0, 0, DateTimeKind.Utc),
                Details = "",
                UpperIdentifier = base.CurrentClassId
            });

            changes.Add(new DataItemExam()
            {
                Identifier = Guid.NewGuid(),
                Name = "Exam in the future",
                Date = new DateTime(2015, 9, 28, 0, 0, 0, DateTimeKind.Utc),
                Details = "",
                UpperIdentifier = base.CurrentClassId
            });

            changes.Add(new DataItemExam()
            {
                Identifier = Guid.NewGuid(),
                Name = "Exam today",
                Date = new DateTime(2015, 9, 25, 0, 0, 0, DateTimeKind.Utc),
                Details = "",
                UpperIdentifier = base.CurrentClassId
            });

            await base.DataStore.ProcessLocalChanges(changes);


            //var viewModel = await ViewModelAgenda.LoadAsync(base.LocalAccountId, base.CurrentSemesterId, new DateTime(2015, 9, 25));


            //Assert.AreEqual(7, viewModel.Items.Count);
            //Assert.IsTrue(viewModel.Items.Any(i => i.Name.Equals("New Year 2016")));
            //Assert.IsTrue(viewModel.Items.Any(i => i.Name.Equals("Birthday 2015")));
            //Assert.IsTrue(viewModel.Items.Any(i => i.Name.Equals("Afterparty 2015")));
            //Assert.IsTrue(viewModel.Items.Any(i => i.Name.Equals("Recovery")));
            //Assert.IsTrue(viewModel.Items.Any(i => i.Name.Equals("In the past")));
            //Assert.IsTrue(viewModel.Items.Any(i => i.Name.Equals("Exam in the future")));
            //Assert.IsTrue(viewModel.Items.Any(i => i.Name.Equals("Exam today")));
        }
    }
}
