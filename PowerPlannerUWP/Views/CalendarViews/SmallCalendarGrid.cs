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

            if (!semester.IsMonthDuringThisSemester(DisplayMonth))
                return new DifferentSemesterOverlayControl();

            return null;
        }
    }
}
