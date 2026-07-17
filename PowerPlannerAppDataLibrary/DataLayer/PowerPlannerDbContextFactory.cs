using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public class PowerPlannerDbContextFactory : IDesignTimeDbContextFactory<PowerPlannerDbContext>
    {
        public PowerPlannerDbContext CreateDbContext(string[] args)
        {
            string databasePath = Path.Combine(Path.GetTempPath(), "powerplanner-design.db");
            return new PowerPlannerDbContext(databasePath, account: null);
        }
    }
}