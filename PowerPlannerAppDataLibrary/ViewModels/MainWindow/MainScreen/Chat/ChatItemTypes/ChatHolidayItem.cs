using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes
{
    internal class ChatHolidayItem : BaseChatItem
    {
        [Description("The name of the holiday.")]
        [NotNull]
        [DisallowNull]
        public string Name { get; set; }

        [Description("The start date of the holiday.")]
        [NotNull]
        [DisallowNull]
        public DateOnly StartDate { get; set; }

        [Description("The end date of the holiday.")]
        [NotNull]
        [DisallowNull]
        public DateOnly EndDate { get; set; }

        [Description("Optional details for the holiday.")]
        public string Details { get; set; }

        public ChatHolidayItem(ViewItemHoliday holiday) : base(holiday)
        {
            Name = holiday.Name;
            Details = holiday.Details;
            StartDate = DateOnly.FromDateTime(holiday.Date);
            EndDate = DateOnly.FromDateTime(holiday.EndTime);
        }
    }
}
