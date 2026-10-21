using System;
using System.Collections.Generic;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Views
{
    /// <summary>
    /// Supported on Android, UWP, and iOS
    /// </summary>
    public class FloatingAddItemButton : View
    {
        public Action AddTask { get; set; }
        public Action AddEvent { get; set; }
        public Action AddHoliday { get; set; }
    }
}
