using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PowerPlannerSending;
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
    public class TestClassSchedules : BaseTestAccountWithClasses
    {
        [TestMethod]
        public async System.Threading.Tasks.Task TestAddingClassSchedule()
        {
            DataChanges changes = new DataChanges();

            changes.Add(new DataItemSchedule()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = base.CurrentClassId,
                StartTime = new DateTime(2015, 1, 1, 8, 00, 00, DateTimeKind.Utc),
                EndTime = new DateTime(2015, 1, 1, 8, 50, 00, DateTimeKind.Utc),
                Room = "Modern Languages 201",
                ScheduleWeek = Schedule.Week.BothWeeks,
                ScheduleType = Schedule.Type.Normal,
                DayOfWeek = DayOfWeek.Monday
            });

            await DataStore.ProcessLocalChanges(changes);

            var viewModel = await ViewModelClass.LoadAsync(base.LocalAccountId, base.CurrentClassId, new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            Assert.AreEqual(1, viewModel.Class.Schedules.Count);

            var schedule = viewModel.Class.Schedules[0];

            Assert.AreEqual(new DateTime(2015, 1, 1, 8, 00, 00, DateTimeKind.Utc), schedule.StartTime);
            Assert.AreEqual(new DateTime(2015, 1, 1, 8, 50, 00, DateTimeKind.Utc), schedule.EndTime);
            Assert.AreEqual("Modern Languages 201", schedule.Room);
            Assert.AreEqual(Schedule.Week.BothWeeks, schedule.ScheduleWeek);
            Assert.AreEqual(DayOfWeek.Monday, schedule.DayOfWeek);
        }

        [TestMethod]
        public async System.Threading.Tasks.Task TestDeletingClassSchedule()
        {
            DataChanges changes = new DataChanges();

            changes.Add(new DataItemSchedule()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = base.CurrentClassId,
                StartTime = new DateTime(2015, 1, 1, 8, 00, 00, DateTimeKind.Utc),
                EndTime = new DateTime(2015, 1, 1, 8, 50, 00, DateTimeKind.Utc),
                Room = "Modern Languages 201",
                ScheduleWeek = Schedule.Week.BothWeeks,
                ScheduleType = Schedule.Type.Normal,
                DayOfWeek = DayOfWeek.Monday
            });

            await DataStore.ProcessLocalChanges(changes);

            var viewModel = await ViewModelClass.LoadAsync(base.LocalAccountId, base.CurrentClassId, new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            Assert.AreEqual(1, viewModel.Class.Schedules.Count);

            var schedule = viewModel.Class.Schedules[0];


            changes = new DataChanges();

            changes.DeleteItem(schedule.Identifier);

            await DataStore.ProcessLocalChanges(changes);

            viewModel = await ViewModelClass.LoadAsync(base.LocalAccountId, base.CurrentClassId, new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            Assert.AreEqual(0, viewModel.Class.Schedules.Count);
        }
    }
}
