using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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

        [Description("If the DueTimeOption is set to CustomTime, this specifies the custom due time.")]
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
        [Description("The name of the task.")]
        [NotNull]
        [DisallowNull]
        public string Name { get; set; }

        [Description("Optional details for the task.")]
        public string Details { get; set; }

        [Description("The due date and time of the task.")]
        [NotNull]
        [DisallowNull]
        public TaskDueDateAndTime DueDateAndTime { get; set; }

        [Description("The percent complete of the task, from 0.0 to 1.0.")]
        public double PercentComplete { get; set; }

        public ChatTaskItem(ViewItemTaskOrEvent task) : base(task)
        {
            Name = task.Name;
            Details = task.Details;
            DueDateAndTime = TaskDueDateAndTime.FromViewItem(task);
            PercentComplete = task.PercentComplete;
        }
    }
}
