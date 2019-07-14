using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.ViewItems
{
    public interface IGPACredits
    {
        double GPA { get; }

        double CreditsEarned { get; }

        double CreditsAffectingGpa { get; }
    }
}
