using InterfacesUWP.CalendarFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfacesUWP
{
    public class DefaultCalendarView : TCalendarView
    {
        protected override TCalendarGrid GenerateCalendarGrid(DateTime displayMonth)
        {
            return new DefaultCalendarGrid(this, displayMonth, IsMouseOver);
        }

        protected override void OnMouseOverChanged(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (IsMouseOver && e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                for (int i = 0; i < _calendars.Count; i++)
                    (_calendars[i] as DefaultCalendarGrid).ShowArrows();
            }

            else
            {
                for (int i = 0; i < _calendars.Count; i++)
                    (_calendars[i] as DefaultCalendarGrid).HideArrows();
            }
        }

        protected override void GenerateEventsOnGrid(TCalendarGrid grid, DateTime displayMonth)
        {
            //nothing
        }
    }
}
