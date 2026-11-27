using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes
{
    [Description("The time option for the event, which can be during class, all day, or a custom time. The default is during class.")]
    internal enum EventTimeOption
    {
        DuringClass,
        AllDay,
        CustomTime,
    }

    internal class EventDateAndTime
    {
        [Description("The date of the event.")]
        [NotNull]
        [DisallowNull]
        public required DateOnly Date { get; set; }

        [Description("The time option for the event.")]
        [NotNull]
        [DisallowNull]
        public required EventTimeOption TimeOption { get; set; }

        [Description("If the TimeOption is set to CustomTime, this specifies the custom start time.")]
        public TimeOnly? CustomStartTime { get; set; }

        [Description("If the TimeOption is set to CustomTime, this specifies the custom end time.")]
        public TimeOnly? CustomEndTime { get; set; }

        public static EventDateAndTime FromViewItem(ViewItemTaskOrEvent task)
        {
            EventTimeOption timeOption;
            switch (task.TimeOption)
            {
                case DataLayer.DataItems.DataItemMegaItem.TimeOptions.AllDay:
                    timeOption = EventTimeOption.AllDay;
                    break;
                case DataLayer.DataItems.DataItemMegaItem.TimeOptions.Custom:
                    timeOption = EventTimeOption.CustomTime;
                    break;
                default: // During class
                    timeOption = EventTimeOption.DuringClass;
                    break;
            }

            return new EventDateAndTime
            {
                Date = DateOnly.FromDateTime(task.Date),
                TimeOption = timeOption,
                CustomStartTime = timeOption == EventTimeOption.CustomTime ? new TimeOnly(task.Date.Hour, task.Date.Minute) : null,
                CustomEndTime = timeOption == EventTimeOption.CustomTime ? new TimeOnly(task.Date.Hour, task.Date.Minute) : null
            };
        }
    }

    internal class ChatEventItem : BaseChatItem
    {
        [Description("The name of the event.")]
        [NotNull]
        [DisallowNull]
        public string Name { get; set; }

        [Description("Optional details for the event.")]
        public string Details { get; set; }

        [Description("The date and time of the event.")]
        [NotNull]
        [DisallowNull]
        public EventDateAndTime DateAndTime { get; set; }

        [Description("The upper identifier of the event, typically a class identifier, but it also can be a semester identifier if it's a \"no class\" task.")]
        [NotNull]
        [DisallowNull]
        public Guid UpperIdentifier { get; set; }

        public ChatEventItem(ViewItemTaskOrEvent e) : base(e)
        {
            Name = e.Name;
            Details = e.Details;
            DateAndTime = EventDateAndTime.FromViewItem(e);
            UpperIdentifier = e.Class.Identifier;
        }
    }
}
