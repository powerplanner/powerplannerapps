using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsUniversal;
using Windows.UI;

namespace UnitTestProject.BaseTestClasses
{
    public abstract class BaseTestAccountWithClasses : BaseTestAccountWithSemesters
    {
        public Guid CurrentClassId { get; private set; }

        protected override async Task InitializeAfterSemesters()
        {
            DataChanges changes = new DataChanges();

            changes.Add(new DataItemClass()
            {
                Credits = 3,
                Details = "Some details about Math class",
                Identifier = Guid.NewGuid(),
                Name = "Math",
                RawColor = ColorTools.GetArray(Colors.Blue, 3),
                UpperIdentifier = CurrentSemesterId
            });

            CurrentClassId = Guid.NewGuid();

            changes.Add(new DataItemClass()
            {
                Name = "English",
                Details = "The class where you write essays in.",
                Identifier = CurrentClassId,
                Credits = 4,
                RawColor = ColorTools.GetArray(Colors.Red, 3),
                UpperIdentifier = CurrentSemesterId
            });

            changes.Add(new DataItemClass()
            {
                Name = "Spanish",
                Details = "Learn a foreign language",
                Identifier = Guid.NewGuid(),
                Credits = 4,
                RawColor = ColorTools.GetArray(Colors.Green, 3),
                UpperIdentifier = base.CurrentSemesterId
            });

            changes.Add(new DataItemClass()
            {
                Name = "Science",
                Details = "Perform experiments",
                Identifier = Guid.NewGuid(),
                Credits = 4,
                RawColor = ColorTools.GetArray(Colors.Purple, 3),
                UpperIdentifier = base.CurrentSemesterId
            });

            await DataStore.ProcessLocalChanges(changes);
        }
    }
}
