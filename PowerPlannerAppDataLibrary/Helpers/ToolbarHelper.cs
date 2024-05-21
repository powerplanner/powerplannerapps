using System;
using System.Drawing;
using System.Linq;
using Vx;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public static class ToolbarHelper
    {
        public static Toolbar PowerPlannerThemed(this Toolbar toolbar)
        {
            if (VxPlatform.Current == Platform.iOS)
            {
                toolbar.BackgroundColor = Color.FromArgb(255, 93, 107, 162);
            }

            return toolbar;
        }

        public static Toolbar InnerToolbarThemed(this Toolbar toolbar)
        {
            toolbar.BackgroundColor = Color.FromArgb(255, 31, 38, 86);
            return toolbar;
        }

        public static ToolbarCommand AddCommand(Action addTask, Action addEvent, Action addHoliday = null)
        {
            return new ToolbarCommand
            {
                Text = PowerPlannerResources.GetString("Calendar_FullCalendarAddButton.ToolTipService.ToolTip"),
                Glyph = MaterialDesign.MaterialDesignIcons.Add,
                SubCommands = new ToolbarCommand[]
                {
                    new ToolbarCommand
                    {
                        Text = PowerPlannerResources.GetString("String_AddTask"),
                        Action = addTask
                    },

                    new ToolbarCommand
                    {
                        Text = PowerPlannerResources.GetString("String_AddEvent"),
                        Action = addEvent
                    },

                    addHoliday != null ? new ToolbarCommand
                    {
                        Text = PowerPlannerResources.GetString("String_AddHoliday"),
                        Action = addHoliday
                    } : null
                }.Where(i => i != null).ToArray()
            };
        }
    }
}

