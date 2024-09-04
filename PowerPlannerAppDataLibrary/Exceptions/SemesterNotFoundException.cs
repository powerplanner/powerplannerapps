using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.Exceptions
{
    public class SemesterNotFoundException : Exception
    {
        public SemesterNotFoundException() : base("Semester couldn't be found") { }

        public SemesterNotFoundException(string message) : base(message) { }
    }
}
