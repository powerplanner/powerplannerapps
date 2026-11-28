using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools;
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
                    Name = "id",
                    Description = "The ID of the task.",
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
                    Name = "classOrSemesterId",
                    Description = "The ID of the parent the task belongs to. Typically this is a class ID, but it can be a semester ID for \"no class\" tasks.",
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
                    Name = "name",
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
                    Name = "percentComplete",
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
                    Name = "dueDateAndTime",
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
                                Name = "dueDate",
                                Description = "The due date of the task.",
                                WhenAdding = PropertyPresence.Required,
                                WhenEditing = PropertyPresence.Required
                            },
                            new Property<TaskDueTimeOption>
                            {
                                Name = "dueTimeOption",
                                Description = "The due time option for the task. \"StartOfClass\" is the default option that should be used unless otherwise specified or already set.",
                                WhenAdding = PropertyPresence.Required,
                                WhenEditing = PropertyPresence.Required
                            },
                            new Property<TimeOnly>
                            {
                                Name = "customDueTime",
                                Description = "If the dueTimeOption is set to CustomTime, this specifies the custom due time in HH:MM format.",
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
                },

                new Property<string>
                {
                    Name = "details",
                    Description = "Additional details about the task.",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    MaxLength = 4000,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.Details = value;
                    }
                },

                new Property<Guid>
                {
                    Name = "gradeWeightCategoryId",
                    Description = $"If set to a valid weight category from the task's class, this means the task is a graded item and is part of that weight. Alternatively, this can be an \"Unassigned\" grade by specifying \"{PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_UNASSIGNED}\". Unassigned grades show up in a separate list on the class grades page. Or, this can be \"Excluded from grades\", by specifying \"{PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_EXCLUDED}\", which will prevent the item from showing up in the grades page completely. Default is \"{PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_UNASSIGNED}\".",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.WeightCategoryIdentifier = value;
                    }
                },

                new Property<double>
                {
                    Name = "gradeReceived",
                    Description = "The number of points received for the grade. To clear the grade received and make this a not-yet-graded item, set this to -50.",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.GradeReceived = value;
                    }
                },

                new Property<double>
                {
                    Name = "gradeTotal",
                    Description = "The total number of points possible for the grade.",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.GradeTotal = value;
                    }
                },

                new Property<bool>
                {
                    Name = "gradeIsDropped",
                    Description = "Whether the grade is dropped (ignored from the overall grade calculation).",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.IsDropped = value;
                    }
                }
            ];
        }

        public override DataItemMegaItem CreateDataItem(AddOrEditMode mode)
        {
            var dataItem = base.CreateDataItem(mode);
            if (dataItem.UpperIdentifier != Guid.Empty)
            {
                var semesterId = PowerPlannerApp.Current.GetMainScreenViewModel()?.CurrentSemesterId;
                if (semesterId != null && semesterId == dataItem.UpperIdentifier)
                {
                    dataItem.MegaItemType = PowerPlannerSending.MegaItemType.Task;
                }
                else
                {
                    dataItem.MegaItemType = PowerPlannerSending.MegaItemType.Homework;
                }
            }
            return dataItem;
        }
    }
}
