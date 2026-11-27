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

        protected override BaseAddOrEditDataItemProperty[] GetProperties()
        {
            return
            [
                new Property<Guid>
                {
                    Name = "Identifier",
                    Description = "The identifier of the class.",
                    WhenAdding = PropertyPresence.NotPresent,
                    WhenEditing = PropertyPresence.Required,
                    RequireNonDefaultValueIfSet = true,
                    Apply = (dataItem, value) =>
                    {
                        if (value == Guid.Empty)
                        {
                            throw new ArgumentException("Class identifier cannot be empty.");
                        }
                        dataItem.Identifier = value;
                    }
                },

                new Property<Guid>
                {
                    Name = "SemesterIdentifier",
                    Description = "The identifier of the semester this class belongs to.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    RequireNonDefaultValueIfSet = true,
                    Apply = (dataItem, value) =>
                    {
                        if (value == Guid.Empty)
                        {
                            throw new ArgumentException("Semester identifier cannot be empty.");
                        }
                        dataItem.UpperIdentifier = value;
                    }
                },

                new Property<string>
                {
                    Name = "Name",
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
                    Name = "Color",
                    Description = "The color of the class, in hex string format, ex \"#FF0000\" for red. Use darker colors, since white text will be displayed on top of the class color.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.RawColor = [value.R, value.G, value.B];
                    }
                },

                new Property<DateOnly>
                {
                    Name = "StartDate",
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
                    Name = "EndDate",
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
                    Name = "Credits",
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
                }
            ];
        }
    }
}
