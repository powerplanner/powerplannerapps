using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestProject.BaseTestClasses;

namespace UnitTestProject
{
    [TestClass]
    public class TestHomeworkExamSorting : BaseTestAccountWithClasses
    {
        [TestMethod]
        public void TestHomeworkExamSortingAll()
        {
            DataChanges changes = new DataChanges();

            changes.Add(new DataItemExam()
            {
                Name = "Second item",
                Date = new DateTime(2015, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                UpperIdentifier = base.CurrentClassId
            });

            changes.Add(new DataItemHomework()
            {
                Name = "First item",
                Date = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpperIdentifier = base.CurrentClassId
            });
        }
    }
}
