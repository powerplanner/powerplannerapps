using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerUWPLibrary.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsUniversal;
using UnitTestProject.BaseTestClasses;
using Windows.UI;

namespace UnitTestProject
{
    [TestClass]
    public class TestClasses : BaseTestAccountWithSemesters
    {
        [TestMethod]
        public async Task TestAddingClass_Defaults()
        {
            DataItemClass c = new DataItemClass()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = base.CurrentSemesterId,
                Credits = 3,
                Name = "Math",
                Details = "",
                RawColor = ColorTools.GetArray(Colors.Red, 3)
            };

            DataChanges changes = new DataChanges();
            changes.Add(c);

            await base.DataStore.ProcessLocalChanges(changes);

            var viewModel = await ViewModelClass.LoadAsync(base.LocalAccountId, c.Identifier, DateTime.Today);

            var viewClass = viewModel.Class;

            Assert.AreEqual("Math", viewClass.Name);
            Assert.AreEqual(3, viewClass.Credits);
            Assert.AreEqual("", viewClass.Details);
            Assert.AreEqual(Colors.Red, viewClass.Color);
        }
    }
}
