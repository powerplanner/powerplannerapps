using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ToolsPortable;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal class AddOrEditClassDataItem : BaseAddOrEditDataItem<DataItemClass>
    {
        public override string DataTypeName => "classes";

        protected override BaseAddOrEditItemProperty[] GetProperties()
        {
            return
            [
                new Property<Guid>
                {
                    Name = "id",
                    Description = "The ID of the class.",
                    WhenAdding = PropertyPresence.NotPresent,
                    WhenEditing = PropertyPresence.Required,
                    RequireNonDefaultValueIfSet = true,
                    Apply = (dataItem, value) =>
                    {
                        if (value == Guid.Empty)
                        {
                            throw new ArgumentException("Class ID cannot be empty.");
                        }
                        dataItem.Identifier = value;
                    }
                },

                new Property<Guid>
                {
                    Name = "semesterId",
                    Description = "The ID of the semester this class belongs to.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    RequireNonDefaultValueIfSet = true,
                    Apply = (dataItem, value) =>
                    {
                        if (value == Guid.Empty)
                        {
                            throw new ArgumentException("Semester ID cannot be empty.");
                        }
                        dataItem.UpperIdentifier = value;
                    }
                },

                new Property<string>
                {
                    Name = "name",
                    Description = "The name of the class.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    RequireNonEmptyStringIfSet = true,
                    RequireSingleLineIfSet = true,
                    Apply = (dataItem, value) =>
                    {
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            throw new ArgumentException("Class name cannot be empty or whitespace.");
                        }
                        if (value.Length > 150)
                        {
                            throw new ArgumentException("Class name cannot be longer than 150 characters.");
                        }
                        dataItem.Name = value;
                    }
                },

                new Property<Color>
                {
                    Name = "color",
                    Description = "The color of the class, in hex string format, ex \"#FF0000\" for red. Use darker colors, since white text will be displayed on top of the class color.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.RawColor = [value.R, value.G, value.B];
                    }
                },

                new Property<string>
                {
                    Name = "details",
                    Description = "Details for the class (office hour contact info, notes about the class, etc).",
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
                    Description = "The start date of the class. To clear the start date, send " + DateOnly.MinValue.ToString("yyyy-MM-dd") + ".",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        if (value == DateOnly.MinValue)
                        {
                            dataItem.StartDate = PowerPlannerSending.DateValues.UNASSIGNED;
                        }
                        else
                        {
                            dataItem.StartDate = DateTime.SpecifyKind(value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                            if (!SqlDate.IsValid(dataItem.StartDate))
                            {
                                throw new ArgumentException("Start date is out of range for SQL DateTime.");
                            }
                        }
                    }
                },

                new Property<DateOnly>
                {
                    Name = "endDate",
                    Description = "The end date of the class. To clear the end date, send " + DateOnly.MinValue.ToString("yyyy-MM-dd") + ".",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        if (value == DateOnly.MinValue)
                        {
                            dataItem.EndDate = PowerPlannerSending.DateValues.UNASSIGNED;
                        }
                        else
                        {
                            dataItem.EndDate = DateTime.SpecifyKind(value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                            if (!SqlDate.IsValid(dataItem.EndDate))
                            {
                                throw new ArgumentException("End date is out of range for SQL DateTime.");
                            }
                        }
                    }
                },

                new Property<double>
                {
                    Name = "credits",
                    Description = "The credits the class is worth. If no credits are assigned, send -1.",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        if (value < -1)
                        {
                            throw new ArgumentException("Credits cannot be less than -1.");
                        }

                        dataItem.Credits = value;
                    }
                },

                new Property<double>
                {
                    Name = "overriddenGrade",
                    Description = "The overridden grade for the class (as a percent from 0.0 to 1.0). To remove the overridden grade, send -1.",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        if (value < -1)
                        {
                            throw new ArgumentException("Overridden grade cannot be less than -1.");
                        }

                        dataItem.OverriddenGrade = value;
                    }
                },

                new Property<double>
                {
                    Name = "overriddenGpa",
                    Description = "The overridden GPA for the class (ex: 3.0 or 4.0). To remove the overridden GPA, send -1.",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        if (value < -1)
                        {
                            throw new ArgumentException("Overridden GPA cannot be less than -1.");
                        }

                        dataItem.OverriddenGPA = value;
                    }
                },

                new Property<PowerPlannerSending.GpaType>
                {
                    Name = "gpaType",
                    Description = "Whether the class uses GPA numbers (\"Standard\", and the default value), or if it's a pass/fail class. ",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.GpaType = value;
                    }
                },

                new Property<double>
                {
                    Name = "passingGrade",
                    Description = "If the class is a pass/fail class, this specifies the passing grade for the class, as a percent from 0.0 to 1.0. If the class is not a pass/fail class, this value is insignificant.",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.PassingGrade = value;
                    }
                }
            ];
        }
    }
}
