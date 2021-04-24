using InterfacesUWP.CalendarFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfacesUWP
{
    public class DefaultSmallCalendarView : TCalendarView
    {
        protected override TCalendarGrid GenerateCalendarGrid(DateTime displayMonth)
        {
            return new DefaultSmallCalendarGrid(this, displayMonth, IsMouseOver);
        }

        protected override void OnMouseOverChanged(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (IsMouseOver && e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                for (int i = 0; i < _calendars.Count; i++)
                    (_calendars[i] as DefaultSmallCalendarGrid).ShowArrows();
            }

            else
            {
                for (int i = 0; i < _calendars.Count; i++)
                    (_calendars[i] as DefaultSmallCalendarGrid).HideArrows();
            }
        }

        protected override void GenerateEventsOnGrid(TCalendarGrid grid, DateTime displayMonth)
        {
            //nothing
        }
    }
}
