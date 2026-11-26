using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerSending;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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

        [Description("The credits the class is worth, or -1 if no credits assigned.")]
        [NotNull]
        [DisallowNull]
        public double Credits { get; set; }

        [Description("Optional start date of the class, if it starts later than the semester itself.")]
        public DateOnly? StartDate { get; set; }

        [Description("Optional end date of the class, if it ends earlier than the semester itself.")]
        public DateOnly? EndDate { get; set; }

        public ChatClassItem(ViewItemClass c)
        {
            Name = c.Name;
            Color = ColorHelper.ToString(c.Color);
            Details = c.Details;
            Credits = c.Credits;
            StartDate = c.StartDate == DateValues.UNASSIGNED ? null : DateOnly.FromDateTime(c.StartDate);
            EndDate = c.EndDate == DateValues.UNASSIGNED ? null : DateOnly.FromDateTime(c.EndDate);
        }
    }
}
