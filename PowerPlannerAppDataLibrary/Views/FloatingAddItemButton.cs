using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Views
{
    /// <summary>
    /// Only supported on Android
    /// </summary>
    public class FloatingAddItemButton : View
    {
        public Action AddTask { get; set; }
        public Action AddEvent { get; set; }
        public Action AddHoliday { get; set; }
    }
}
