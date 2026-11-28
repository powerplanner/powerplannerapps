using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes
{
    [Description("The due time option for the task, which can be before class, start of class, during class, end of class, end of day, or a custom time. The default is start of class.")]
    internal enum TaskDueTimeOption
    {
        StartOfClass,
        BeforeClass,
        DuringClass,
        EndOfClass,
        EndOfDay,
        CustomTime,
    }

    internal class TaskDueDateAndTime
    {
        [Description("The due date of the task.")]
        [NotNull]
        [DisallowNull]
        public required DateOnly DueDate { get; set; }

        [Description("The due time option for the task.")]
        [NotNull]
        [DisallowNull]
        public required TaskDueTimeOption DueTimeOption { get; set; }

        [Description("If the dueTimeOption is set to CustomTime, this specifies the custom due time.")]
        public TimeOnly? CustomDueTime { get; set; }

        public static TaskDueDateAndTime FromViewItem(ViewItemTaskOrEvent task)
        {
            TaskDueTimeOption dueTimeOption;
            switch (task.TimeOption)
            {
                case DataLayer.DataItems.DataItemMegaItem.TimeOptions.AllDay:
                    dueTimeOption = TaskDueTimeOption.EndOfDay;
                    break;
                case DataLayer.DataItems.DataItemMegaItem.TimeOptions.BeforeClass:
                    dueTimeOption = TaskDueTimeOption.BeforeClass;
                    break;
                case DataLayer.DataItems.DataItemMegaItem.TimeOptions.DuringClass:
                    dueTimeOption = TaskDueTimeOption.DuringClass;
                    break;
                case DataLayer.DataItems.DataItemMegaItem.TimeOptions.EndOfClass:
                    dueTimeOption = TaskDueTimeOption.EndOfClass;
                    break;
                case DataLayer.DataItems.DataItemMegaItem.TimeOptions.StartOfClass:
                    dueTimeOption = TaskDueTimeOption.StartOfClass;
                    break;
                case DataLayer.DataItems.DataItemMegaItem.TimeOptions.Custom:
                    dueTimeOption = TaskDueTimeOption.CustomTime;
                    break;
                default:
                    dueTimeOption = TaskDueTimeOption.StartOfClass;
                    break;
            }

            return new TaskDueDateAndTime
            {
                DueDate = DateOnly.FromDateTime(task.Date),
                DueTimeOption = dueTimeOption,
                CustomDueTime = dueTimeOption == TaskDueTimeOption.CustomTime ? new TimeOnly(task.Date.Hour, task.Date.Minute) : null
            };
        }
    }

    internal class ChatTaskItem : BaseChatItem
    {
        [Description("The name of the task, trimmed to at most 100 chars.")]
        [NotNull]
        [DisallowNull]
        public string Name { get; set; }

        [Description("Optional details for the task, trimmed to at most 100 chars.")]
        public string Details { get; set; }

        [Description("The due date and time of the task.")]
        [NotNull]
        [DisallowNull]
        public TaskDueDateAndTime DueDateAndTime { get; set; }

        [Description("The percent complete of the task, from 0.0 to 1.0.")]
        public double PercentComplete { get; set; }

        [Description("The ID of the parent the task belongs to. Typically this is a class ID, but it can be a semester ID for \"no class\" tasks.")]
        [NotNull]
        [DisallowNull]
        public Guid ClassOrSemesterId { get; set; }

        public ChatTaskItem(ViewItemTaskOrEvent task) : base(task)
        {
            Name = StringTools.TrimLengthWithEllipses(task.Name, 100);
            Details = StringTools.TrimLengthWithEllipses(task.Details, 100);
            DueDateAndTime = TaskDueDateAndTime.FromViewItem(task);
            PercentComplete = task.PercentComplete;
            ClassOrSemesterId = task.Class.Identifier;
        }
    }

    internal class ChatFullTaskItem : ChatTaskItem
    {
        [Description("The name of the task.")]
        [NotNull]
        [DisallowNull]
        public new string Name { get; set; }

        [Description("Optional details for the task.")]

        public new string Details { get; set; }

        [Description("If set to a valid weight category from the task's class, this means the task is a graded item and is part of that weight. Alternatively, this can be an \"Unassigned\" grade if it's null. Unassigned grades show up in a separate list on the class grades page. Or, this can be \"Excluded from grades\" if set to \"11111111-1111-1111-1111-111111111111\", which prevents the item from showing up in the grades page completely. Default is null (unassigned).")]
        public Guid? GradeWeightCategoryId { get; set; }

        [Description("The number of points received for the grade. Can be null if not yet graded.")]
        public double? GradeReceived { get; set; }

        [Description("The total number of points possible for the grade.")]
        public double? GradeTotal { get; set; }

        [Description("Whether the grade is dropped (ignored from the overall grade calculation).")]
        public bool GradeIsDropped { get; set; }

        public ChatFullTaskItem(ViewItemTaskOrEvent e) : base(e)
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
