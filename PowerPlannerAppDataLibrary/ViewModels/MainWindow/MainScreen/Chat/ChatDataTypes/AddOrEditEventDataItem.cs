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
                    Name = "Identifier",
                    Description = "The identifier of the event.",
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
                    Description = "The upper identifier of the event, typically a class identifier, but also can be a semester identifier if it's a \"no class\" event that isn't within a specific class.",
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
                    Name = "DateAndTime",
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
                                Name = "Date",
                                Description = "The date of the event.",
                                WhenAdding = PropertyPresence.Required,
                                WhenEditing = PropertyPresence.Required
                            },
                            new Property<EventTimeOption>
                            {
                                Name = "TimeOption",
                                Description = "The time option for the event. \"DuringClass\" is the default option that should be used unless otherwise specified or already set.",
                                WhenAdding = PropertyPresence.Required,
                                WhenEditing = PropertyPresence.Required
                            },
                            new Property<TimeOnly>
                            {
                                Name = "CustomStartTime",
                                Description = "If the TimeOption is set to CustomTime, this specifies the custom start time in HH:MM format.",
                                WhenAdding = PropertyPresence.Optional,
                                WhenEditing = PropertyPresence.Optional
                            },
                            new Property<TimeOnly>
                            {
                                Name = "CustomEndTime",
                                Description = "If the TimeOption is set to CustomTime, this specifies the custom end time in HH:MM format.",
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
                    Name = "Details",
                    Description = "Additional details about the event.",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    MaxLength = 4000,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.Details = value;
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
