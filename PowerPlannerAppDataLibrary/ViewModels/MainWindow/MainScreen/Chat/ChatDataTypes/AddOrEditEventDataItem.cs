using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal class AddOrEditEventDataItem : BaseAddOrEditDataItem<DataItemMegaItem>
    {
        public override string DataTypeName => "events";
        protected override BaseAddOrEditItemProperty[] GetProperties()
        {
            return
            [
                new Property<Guid>
                {
                    Name = "id",
                    Description = "The ID of the event.",
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
                    Description = "The ID of the parent the event belongs to. Typically this is a class ID, but it can be a semester ID for \"no class\" events.",
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
                    Description = "The name of the event.",
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

                new NestedProperty
                {
                    Name = "dateAndTime",
                    Description = "The date and time of the event.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    RequireNonDefaultValueIfSet = true,
                    NestedObject = new NestedObject
                    {
                        NestedProperties =
                        [
                            new Property<DateOnly>
                            {
                                Name = "date",
                                Description = "The date of the event.",
                                WhenAdding = PropertyPresence.Required,
                                WhenEditing = PropertyPresence.Required
                            },
                            new Property<EventTimeOption>
                            {
                                Name = "timeOption",
                                Description = "The time option for the event. \"DuringClass\" is the default option that should be used unless otherwise specified or already set.",
                                WhenAdding = PropertyPresence.Required,
                                WhenEditing = PropertyPresence.Required
                            },
                            new Property<TimeOnly>
                            {
                                Name = "customStartTime",
                                Description = "If the timeOption is set to CustomTime, this specifies the custom start time in HH:MM format.",
                                WhenAdding = PropertyPresence.Optional,
                                WhenEditing = PropertyPresence.Optional
                            },
                            new Property<TimeOnly>
                            {
                                Name = "customEndTime",
                                Description = "If the timeOption is set to CustomTime, this specifies the custom end time in HH:MM format.",
                                WhenAdding = PropertyPresence.Optional,
                                WhenEditing = PropertyPresence.Optional
                            }
                        ]
                    },
                    Apply = (dataItem, value) =>
                    {
                        DateOnly date = (DateOnly)value.Properties[0].GetValue();
                        EventTimeOption timeOption = (EventTimeOption)value.Properties[1].GetValue();
                        TimeOnly? customStartTime = null;
                        TimeOnly? customEndTime = null;
                        if (value.Properties[2].HasSetValue)
                        {
                            customStartTime = (TimeOnly)value.Properties[2].GetValue();
                        }
                        if (value.Properties[3].HasSetValue)
                        {
                            customEndTime = (TimeOnly)value.Properties[3].GetValue();
                        }

                        DateTime finalDate = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                        DateTime endTime = PowerPlannerSending.DateValues.UNASSIGNED;

                        switch (timeOption)
                        {
                            case EventTimeOption.AllDay:
                                endTime = finalDate.AddDays(1).AddSeconds(-1);
                                break;

                            case EventTimeOption.CustomTime:
                                if (customStartTime != null && customEndTime != null && customStartTime.Value < customEndTime.Value)
                                {
                                    endTime = finalDate.Add(new TimeSpan(customEndTime.Value.Hour, customEndTime.Value.Minute, 0));
                                    finalDate = finalDate.Add(new TimeSpan(customStartTime.Value.Hour, customStartTime.Value.Minute, 0));
                                }
                                break;

                            // During class is the default, so we don't need to do anything for that case.
                        }

                        dataItem.Date = finalDate;
                        dataItem.EndTime = endTime;
                    }
                },

                new Property<string>
                {
                    Name = "details",
                    Description = "Additional details about the event.",
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
                    Description = $"If set to a valid weight category from the event's class, this means the event is a graded item and is part of that weight. Alternatively, this can be an \"Unassigned\" grade by specifying \"{PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_UNASSIGNED}\". Unassigned grades show up in a separate list on the class grades page. Or, this can be \"Excluded from grades\", by specifying \"{PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_EXCLUDED}\", which will prevent the item from showing up in the grades page completely. Default is \"{PowerPlannerSending.BaseHomeworkExam.WEIGHT_CATEGORY_UNASSIGNED}\".",
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
                    dataItem.MegaItemType = PowerPlannerSending.MegaItemType.Event;
                }
                else
                {
                    dataItem.MegaItemType = PowerPlannerSending.MegaItemType.Exam;
                }
            }
            return dataItem;
        }
    }
}
