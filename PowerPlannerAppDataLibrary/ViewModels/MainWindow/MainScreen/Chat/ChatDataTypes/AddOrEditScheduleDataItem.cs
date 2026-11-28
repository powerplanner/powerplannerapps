using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal class AddOrEditScheduleDataItem : BaseAddOrEditDataItem<DataItemSchedule>
    {
        public override string DataTypeName => "schedules";

        protected override BaseAddOrEditItemProperty[] GetProperties()
        {
            return
            [
                new Property<Guid>
                {
                    Name = "id",
                    Description = "The ID of the schedule.",
                    WhenAdding = PropertyPresence.NotPresent,
                    WhenEditing = PropertyPresence.Required,
                    RequireNonDefaultValueIfSet = true,
                    Apply = (dataItem, value) =>
                    {
                        if (value == Guid.Empty)
                        {
                            throw new ArgumentException("Schedule ID cannot be empty.");
                        }
                        dataItem.Identifier = value;
                    }
                },

                new Property<Guid>
                {
                    Name = "classId",
                    Description = "The ID of the class this schedule belongs to.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    RequireNonDefaultValueIfSet = true,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.UpperIdentifier = value;
                    }
                },

                new Property<DayOfWeek>
                {
                    Name = "dayOfWeek",
                    Description = "The day of the week the schedule occurs on.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.DayOfWeek = value;
                    }
                },

                new Property<TimeOnly>
                {
                    Name = "startTime",
                    Description = "The start time of the schedule, in HH:mm format.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.StartTime = DateTime.SpecifyKind(DateTime.Today.Add(value.ToTimeSpan()), DateTimeKind.Utc);
                    }
                },

                new Property<TimeOnly>
                {
                    Name = "endTime",
                    Description = "The end time of the schedule, in HH:mm format.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.EndTime = DateTime.SpecifyKind(DateTime.Today.Add(value.ToTimeSpan()), DateTimeKind.Utc);
                    }
                },

                new Property<PowerPlannerSending.Schedule.Week>
                {
                    Name = "weekType",
                    Description = "Whether the schedule is weekly, or bi-weekly on week one/A or week two/B.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.ScheduleWeek = value;
                    }
                },

                new Property<string>
                {
                    Name = "room",
                    Description = "The room the schedule occurs in, or a plain URL if it's an online class.",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    RequireSingleLineIfSet = true,
                    RequireNonEmptyStringIfSet = true,
                    MaxLength = 800,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.Room = value;
                    }
                }
            ];
        }
    }
}
