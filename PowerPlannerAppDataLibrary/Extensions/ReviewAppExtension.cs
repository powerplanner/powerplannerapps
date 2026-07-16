using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Extensions
{
    public abstract class ReviewAppExtension
    {
        public static ReviewAppExtension Current { get; set; }

        public abstract Task ReviewAppAsync();
    }
}
