using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools;
using System;
using System.Collections.Generic;
using System.Text;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal class AddOrEditSemesterDataItem : BaseAddOrEditDataItem<DataItemSemester>
    {
        public override string DataTypeName => "semesters";
        protected override BaseAddOrEditItemProperty[] GetProperties()
        {
            return
            [
                new Property<Guid>
                {
                    Name = "id",
                    Description = "The ID of the semester to edit.",
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
                    Name = "yearId",
                    Description = "The ID of the year that the semester is part of.",
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
                    Description = "The name of the semester.",
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

                new Property<string>
                {
                    Name = "details",
                    Description = "Details for the semester.",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    MaxLength = 4000,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.Details = value;
                    }
                },

                new Property<DateOnly>
                {
                    Name = "startDate",
                    Description = "The start date of the semester. To clear the start date, send " + DateOnly.MinValue.ToString("yyyy-MM-dd") + ".",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        if (value == DateOnly.MinValue)
                        {
                            dataItem.Start = PowerPlannerSending.DateValues.UNASSIGNED;
                        }
                        else
                        {
                            dataItem.Start = DateTime.SpecifyKind(value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                            if (!SqlDate.IsValid(dataItem.Start))
                            {
                                throw new ArgumentException("Start date is out of range for SQL DateTime.");
                            }
                        }
                    }
                },

                new Property<DateOnly>
                {
                    Name = "endDate",
                    Description = "The end date of the semester. To clear the end date, send " + DateOnly.MinValue.ToString("yyyy-MM-dd") + ".",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        if (value == DateOnly.MinValue)
                        {
                            dataItem.End = PowerPlannerSending.DateValues.UNASSIGNED;
                        }
                        else
                        {
                            dataItem.End = DateTime.SpecifyKind(value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                            if (!SqlDate.IsValid(dataItem.End))
                            {
                                throw new ArgumentException("End date is out of range for SQL DateTime.");
                            }
                        }
                    }
                },

                new Property<double>
                {
                    Name = "overriddenGpa",
                    Description = "The overridden GPA for the semester (ex: 3.0 or 4.0). To remove the overridden GPA, send -1.",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        if (value < -1)
                        {
                            throw new ArgumentException("Overridden GPA cannot be less than -1.");
                        }

                        dataItem.OverriddenGPA = value == -1 ? PowerPlannerSending.Grade.UNGRADED : value;
                    }
                },

                new Property<double>
                {
                    Name = "overriddenCredits",
                    Description = "The overridden credits for the semester. To remove the overridden credits, send -1.",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        if (value < -1)
                        {
                            throw new ArgumentException("Overridden credits cannot be less than -1.");
                        }

                        dataItem.OverriddenCredits = value == -1 ? PowerPlannerSending.Grade.UNGRADED : value;
                    }
                },
            ];
        }
    }
}
