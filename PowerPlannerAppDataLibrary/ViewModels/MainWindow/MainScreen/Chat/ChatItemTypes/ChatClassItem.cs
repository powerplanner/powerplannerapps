using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes
{
    internal class ChatClassItem : BaseChatItem
    {
        [Description("The name of the class.")]
        [NotNull]
        [DisallowNull]
        public string Name { get; set; }

        [Description("The color of the class, in hex string format, ex \"#FF0000\" for red.")]
        [NotNull]
        [DisallowNull]
        public string Color { get; set; }

        [Description("Optional details (free-form user-visible text) for the class.")]
        public string Details { get; set; }

        [Description("The credits the class is worth, or null if no credits assigned.")]
        public double? Credits { get; set; }

        [Description("Optional start date of the class, if it starts later than the semester itself.")]
        public DateOnly? StartDate { get; set; }

        [Description("Optional end date of the class, if it ends earlier than the semester itself.")]
        public DateOnly? EndDate { get; set; }

        [Description("The schedule of the class, which is a list of days and times the class meets.")]
        [NotNull]
        [DisallowNull]
        public List<ChatScheduleItem> Schedule { get; set; }

        public ChatClassItem(ViewItemClass c) : base(c)
        {
            Name = c.Name;
            Color = ColorHelper.ToString(c.Color);
            Details = c.Details;
            Credits = c.Credits == PowerPlannerSending.Grade.NO_CREDITS ? null : c.Credits;
            StartDate = c.StartDate == DateValues.UNASSIGNED ? null : DateOnly.FromDateTime(c.StartDate);
            EndDate = c.EndDate == DateValues.UNASSIGNED ? null : DateOnly.FromDateTime(c.EndDate);
            Schedule = c.Schedules.Select(i => new ChatScheduleItem(i)).ToList();
        }
    }
}
