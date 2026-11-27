using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal class AddOrEditTaskDataItem : BaseAddOrEditDataItem<DataItemMegaItem>
    {
        public override string DataTypeName => "tasks";
        protected override BaseAddOrEditItemProperty[] GetProperties()
        {
            return
            [
                new Property<Guid>
                {
                    Name = "Identifier",
                    Description = "The identifier of the task.",
                    WhenAdding = PropertyPresence.NotPresent,
                    WhenEditing = PropertyPresence.Required,
                    RequireNonDefaultValueIfSet = true,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.Identifier = value;
                    }
                },

                new Property<Guid>
                {
                    Name = "UpperIdentifier",
                    Description = "The upper identifier of the task, typically a class identifier, but also can be a semester identifier if it's a \"no class\" task that isn't within a specific class.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    RequireNonDefaultValueIfSet = true,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.UpperIdentifier = value;
                    }
                },

                new Property<string>
                {
                    Name = "Name",
                    Description = "The name of the task.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    MaxLength = 200,
                    RequireNonEmptyStringIfSet = true,
                    RequireSingleLineIfSet = true,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.Name = value;
                    }
                },

                new Property<double>
                {
                    Name = "PercentComplete",
                    Description = "The percent complete of the task, from 0.0 to 1.0",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        if (value < 0 || value > 1)
                        {
                            throw new ArgumentException("Percent complete must be between 0.0 and 1.0");
                        }

                        dataItem.PercentComplete = value;
                    }
                },

                new NestedProperty
                {
                    Name = "DueDateAndTime",
                    Description = "The due date and time of the task.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    RequireNonDefaultValueIfSet = true,
                    NestedObject = new NestedObject
                    {
                        NestedProperties =
                        [
                            new Property<DateOnly>
                            {
                                Name = "DueDate",
                                Description = "The due date of the task.",
                                WhenAdding = PropertyPresence.Required,
                                WhenEditing = PropertyPresence.Required
                            },
                            new Property<TaskDueTimeOption>
                            {
                                Name = "DueTimeOption",
                                Description = "The due time option for the task. \"StartOfClass\" is the default option that should be used unless otherwise specified or already set.",
                                WhenAdding = PropertyPresence.Required,
                                WhenEditing = PropertyPresence.Required
                            },
                            new Property<TimeOnly>
                            {
                                Name = "CustomDueTime",
                                Description = "If the DueTimeOption is set to CustomTime, this specifies the custom due time in HH:MM format.",
                                WhenAdding = PropertyPresence.Optional,
                                WhenEditing = PropertyPresence.Optional
                            }
                        ]
                    },
                    Apply = (dataItem, value) =>
                    {
                        DateOnly dueDate = (DateOnly)value.Properties[0].GetValue();
                        TaskDueTimeOption dueTimeOption = (TaskDueTimeOption)value.Properties[1].GetValue();
                        TimeOnly? customDueTime = null;
                        if (value.Properties[2].HasSetValue)
                        {
                            customDueTime = (TimeOnly)value.Properties[2].GetValue();
                        }

                        DateTime finalDate = DateTime.SpecifyKind(dueDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

                        switch (dueTimeOption)
                        {
                            case TaskDueTimeOption.BeforeClass:
                                finalDate = finalDate.AddSeconds(1);
                                break;

                            case TaskDueTimeOption.DuringClass:
                                finalDate = finalDate.AddSeconds(2);
                                break;

                            case TaskDueTimeOption.EndOfClass:
                                finalDate = finalDate.AddSeconds(3);
                                break;

                            case TaskDueTimeOption.CustomTime:
                                if (customDueTime != null)
                                {
                                    finalDate = finalDate.Add(new TimeSpan(customDueTime.Value.Hour, customDueTime.Value.Minute, 4));
                                }
                                break;

                            // Start of class is the default, so we don't need to do anything for that case.
                        }

                       dataItem.Date = finalDate;
                    }
                }
            ];
        }
    }
}
