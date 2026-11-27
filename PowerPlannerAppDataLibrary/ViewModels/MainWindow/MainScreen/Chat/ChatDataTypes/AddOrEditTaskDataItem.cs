using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal class AddOrEditTaskDataItem : BaseAddOrEditDataItem<DataItemMegaItem>
    {
        public override string DataTypeName => "tasks";
        protected override BaseAddOrEditDataItemProperty[] GetProperties()
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
                }
            ];
        }
    }
}
