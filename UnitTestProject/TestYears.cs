using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerUWPLibrary.ViewModel;
using PowerPlannerUWPLibrary.ViewModel.ViewItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestProject.BaseTestClasses;

namespace UnitTestProject
{
    [TestClass]
    public class TestYears : BaseTestAccount
    {
        [TestMethod]
        public async Task TestAddingYear()
        {
            ViewModelYearsAndSemesters viewModel = await ViewModelYearsAndSemesters.LoadAsync(LocalAccountId);

            var years = viewModel.School.Years;

            Assert.AreEqual(0, years.Count);

            DataItemYear year = new DataItemYear()
            {
                Identifier = Guid.NewGuid(),
                Name = "Freshman"
            };

            DataChanges changes = new DataChanges();
            changes.Add(year);

            await DataStore.ProcessLocalChanges(changes);

            Assert.AreEqual(1, DataStore.TableYears.Count(), "Database count of years incorrect");

            Assert.AreEqual(1, years.Count, "View model didn't update");

            ViewItemYear viewYear = years.First();

            Assert.AreEqual("Freshman", viewYear.Name);
            Assert.AreEqual(year.Identifier, viewYear.Identifier);
            Assert.IsTrue((DateTime.UtcNow - viewYear.DateCreated).TotalSeconds < 2);


            changes = new DataChanges();
            changes.Add(new DataItemYear()
            {
                Identifier = Guid.NewGuid(),
                Name = "Sophomore"
            });

            await DataStore.ProcessLocalChanges(changes);

            Assert.AreEqual(2, DataStore.TableYears.Count(), "Database count of years incorrect");

            Assert.AreEqual(2, years.Count);

            Assert.AreEqual("Sophomore", years.Last().Name);


            // Sleep so that updated time should be different than created time
            await Task.Delay(20);


            DateTime originalDateCreated = years.First().DateCreated;


            changes = new DataChanges();
            changes.Add(new DataItemYear()
            {
                Identifier = years.First().Identifier,
                Name = "Freshman Edited"
            });

            await DataStore.ProcessLocalChanges(changes);

            Assert.AreEqual(2, DataStore.TableYears.Count());
            Assert.AreEqual(2, years.Count);

            viewYear = years.First();

            Assert.AreEqual("Freshman Edited", viewYear.Name);
            Assert.AreEqual(originalDateCreated, viewYear.DateCreated);
            Assert.AreNotEqual(viewYear.DateCreated, viewYear.Updated);
        }

        [TestMethod]
        public async Task TestCascadeDeleteYear()
        {
            Guid freshmanId = Guid.NewGuid();
            Guid tempId;

            DataChanges changes = new DataChanges();
            
            changes.Add(new DataItemYear()
            {
                Identifier = freshmanId,
                Name = "Freshman"
            });

            changes.Add(new DataItemSemester()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = changes.EditedItems.OfType<DataItemYear>().First().Identifier,
                Name = "Fall"
            });

            changes.Add(new DataItemSemester()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = changes.EditedItems.OfType<DataItemYear>().First().Identifier,
                Name = "Spring"
            });

            changes.Add(new DataItemYear()
            {
                Identifier = Guid.NewGuid(),
                Name = "Sophomore"
            });

            changes.Add(new DataItemSemester()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = changes.EditedItems.OfType<DataItemYear>().First(i => i.Name.Equals("Sophomore")).Identifier,
                Name = "Fall - Sophomore"
            });

            await DataStore.ProcessLocalChanges(changes);

            Assert.AreEqual(2, DataStore.TableYears.Count(), "Database count of years incorrect");
            Assert.AreEqual(3, DataStore.TableSemesters.Count(), "Database count of semesters incorrect");


            changes = new DataChanges();

            changes.DeleteItem(freshmanId);

            await DataStore.ProcessLocalChanges(changes);

            Assert.AreEqual(1, DataStore.TableYears.Count(), "Database count of years incorrect");
            Assert.AreEqual(1, DataStore.TableSemesters.Count(), "Database count of semesters incorrect");
        }
    }
}
