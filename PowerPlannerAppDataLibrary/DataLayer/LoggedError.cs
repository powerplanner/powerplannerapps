using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public class LoggedError
    {
        public string Name { get; private set; }
        public string Message { get; private set; }
        public DateTime Date { get; private set; }

        public LoggedError(string name, string message)
        {
            Name = name;
            Message = message;
            Date = DateTime.Now;
        }

        public override string ToString()
        {
            return Name + " - " + Date + "\n" + Message;
        }
    }
}
