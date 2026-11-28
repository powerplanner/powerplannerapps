using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ToolsPortable;

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

        [Description("If the timeOption is set to CustomTime, this specifies the custom start time.")]
        public TimeOnly? CustomStartTime { get; set; }

        [Description("If the timeOption is set to CustomTime, this specifies the custom end time.")]
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
        [Description("The name of the event, trimmed to at most 100 chars.")]
        [NotNull]
        [DisallowNull]
        public string Name { get; set; }

        [Description("Optional details for the event, trimmed to at most 100 chars.")]
        public string Details { get; set; }

        [Description("The date and time of the event.")]
        [NotNull]
        [DisallowNull]
        public EventDateAndTime DateAndTime { get; set; }

        [Description("The ID of the parent the event belongs to. Typically this is a class ID, but it can be a semester ID for \"no class\" events.")]
        [NotNull]
        [DisallowNull]
        public Guid ClassOrSemesterId { get; set; }

        public ChatEventItem(ViewItemTaskOrEvent e) : base(e)
        {
            Name = StringTools.TrimLengthWithEllipses(e.Name, 100);
            Details = StringTools.TrimLengthWithEllipses(e.Details, 100);
            DateAndTime = EventDateAndTime.FromViewItem(e);
            ClassOrSemesterId = e.Class.Identifier;
        }
    }

    internal class ChatFullEventItem : ChatEventItem
    {
        [Description("The name of the event.")]
        [NotNull]
        [DisallowNull]
        public new string Name { get; set; }

        [Description("Optional details for the event.")]

        public new string Details { get; set; }

        [Description("If set to a valid weight category from the event's class, this means the event is a graded item and is part of that weight. Alternatively, this can be an \"Unassigned\" grade if it's null. Unassigned grades show up in a separate list on the class grades page. Or, this can be \"Excluded from grades\" if set to \"11111111-1111-1111-1111-111111111111\", which prevents the item from showing up in the grades page completely. Default is null (unassigned).")]
        public Guid? GradeWeightCategoryId { get; set; }

        [Description("The number of points received for the grade. Can be null if not yet graded.")]
        public double? GradeReceived { get; set; }

        [Description("The total number of points possible for the grade.")]
        public double? GradeTotal { get; set; }

        [Description("Whether the grade is dropped (ignored from the overall grade calculation).")]
        public bool GradeIsDropped { get; set; }

        public ChatFullEventItem(ViewItemTaskOrEvent e) : base(e)
        {
            Name = e.Name;
            Details = e.Details;
            GradeWeightCategoryId = e.WeightCategoryIdentifier == PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_UNASSIGNED ? null : e.WeightCategoryIdentifier;
            GradeReceived = e.GradeReceived == PowerPlannerSending.Grade.UNGRADED ? null : e.GradeReceived;
            GradeTotal = e.GradeTotal == PowerPlannerSending.Grade.UNGRADED ? null : e.GradeTotal;
            GradeIsDropped = e.IsDropped;
        }
    }
}
