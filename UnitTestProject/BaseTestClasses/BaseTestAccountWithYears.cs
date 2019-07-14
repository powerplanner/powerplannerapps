using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject.BaseTestClasses
{
    public class BaseTestAccountWithYears : BaseTestAccount
    {
        protected override async Task InitializeAfterAccount()
        {
            DataChanges changes = new DataChanges();

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

            changes.Add(new DataItemYear()
            {
                Identifier = Guid.NewGuid(),
                Name = "Junior"
            });

            changes.Add(new DataItemYear()
            {
                Identifier = Guid.NewGuid(),
                Name = "Senior"
            });

            await DataStore.ProcessLocalChanges(changes);

            await InitializeAfterYears();
        }

        protected virtual async Task InitializeAfterYears()
        {
            // nothing
        }
    }
}
