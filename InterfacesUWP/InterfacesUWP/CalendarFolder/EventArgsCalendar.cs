using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfacesUWP.CalendarFolder
{
    public class EventArgsCalendar : EventArgs
    {
        private DateTime? selectedDate;
        public EventArgsCalendar(DateTime? selectedDate)
        {
            this.selectedDate = selectedDate;
        }

        public DateTime? SelectedDate
        {
            get { return selectedDate; }
        }
    }
}
