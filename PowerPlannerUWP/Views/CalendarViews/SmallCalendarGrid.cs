using InterfacesUWP.CalendarFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace PowerPlannerUWP.Views.CalendarViews
{
    public class SmallCalendarGrid : DefaultSmallCalendarGrid
    {
        public SmallCalendarGrid(TCalendarView calendarView, DateTime displayMonth, bool isMouseOver) : base(calendarView, displayMonth, isMouseOver)
        {
        }

        protected override FrameworkElement GenerateOverlay()
        {
            var semester = (CalendarView as SmallCalendarView)?.ViewModel?.Semester;

            if (semester == null)
                return null;

            if (!PowerPlannerSending.DateValues.IsUnassigned(semester.Start) && DisplayMonth.Date < DateTime.SpecifyKind(semester.Start.Date, DateTimeKind.Local))
                return new DifferentSemesterOverlayControl();

            if (!PowerPlannerSending.DateValues.IsUnassigned(semester.End) && DisplayMonth.Date > DateTime.SpecifyKind(semester.End.Date, DateTimeKind.Local))
                return new DifferentSemesterOverlayControl();

            return null;
        }
    }
}
