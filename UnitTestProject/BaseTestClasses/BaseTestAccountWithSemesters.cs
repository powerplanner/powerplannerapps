using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerUWPLibrary.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject.BaseTestClasses
{
    public class BaseTestAccountWithSemesters : BaseTestAccountWithYears
    {
        public Guid CurrentSemesterId { get; private set; }

        protected override async Task InitializeAfterYears()
        {
            var years = (await ViewModelYearsAndSemesters.LoadAsync(LocalAccountId)).School.Years;

            DataChanges changes = new DataChanges();

            changes.Add(new DataItemSemester()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = years[0].Identifier,
                Name = "Fall (Freshman)"
            });

            changes.Add(new DataItemSemester()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = years[0].Identifier,
                Name = "Spring (Freshman)"
            });

            changes.Add(new DataItemSemester()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = years[1].Identifier,
                Name = "Fall (Sophomore)"
            });

            changes.Add(new DataItemSemester()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = years[1].Identifier,
                Name = "Spring (Sophomore)"
            });

            changes.Add(new DataItemSemester()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = years[2].Identifier,
                Name = "Fall (Junior)"
            });

            changes.Add(new DataItemSemester()
            {
                Identifier = Guid.NewGuid(),
                UpperIdentifier = years[2].Identifier,
                Name = "Spring (Junior)"
            });

            CurrentSemesterId = Guid.NewGuid();

            changes.Add(new DataItemSemester()
            {
                Identifier = CurrentSemesterId,
                UpperIdentifier = years[3].Identifier,
                Name = "Fall (Senior)"
            });

            await DataStore.ProcessLocalChanges(changes);

            await InitializeAfterSemesters();
        }

        protected virtual async Task InitializeAfterSemesters()
        {
            // Nothing
        }
    }
}
