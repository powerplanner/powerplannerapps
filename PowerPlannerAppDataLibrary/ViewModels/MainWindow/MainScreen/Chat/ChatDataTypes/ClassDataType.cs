using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal static class ClassDataType
    {
        public static readonly ChatAddOrEditDataType<DataItemClass> Instance = new ChatAddOrEditDataType<DataItemClass>
        {
            Properties =
            [
                new ChatDataTypeProperty<DataItemClass, Guid>
                {
                    Name = "Identifier",
                    Description = "The identifier of the class.",
                    WhenAdding = PropertyPresence.NotPresent,
                    WhenEditing = PropertyPresence.Required,
                    Apply = (dataItem, value) =>
                    {
                        if (value == Guid.Empty)
                        {
                            throw new ArgumentException("Class identifier cannot be empty.");
                        }
                        dataItem.Identifier = value;
                    }
                },

                new ChatDataTypeProperty<DataItemClass, Guid>
                {
                    Name = "SemesterIdentifier",
                    Description = "The identifier of the semester this class belongs to.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        if (value == Guid.Empty)
                        {
                            throw new ArgumentException("Semester identifier cannot be empty.");
                        }
                        dataItem.UpperIdentifier = value;
                    }
                },

                new ChatDataTypeProperty<DataItemClass, string>
                {
                    Name = "Name",
                    Description = "The name of the class.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
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

                new ChatDataTypeProperty<DataItemClass, Color>
                {
                    Name = "Color",
                    Description = "The color of the class, in hex string format, ex \"#FF0000\" for red. Use darker colors, since white text will be displayed on top of the class color.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.RawColor = [value.R, value.G, value.B];
                    }
                }
            ]
        };
    }
}
