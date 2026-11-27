using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal class AddOrEditTaskDataItem : BaseAddOrEditDataItem<DataItemMegaItem>
    {
        public override string DataTypeName => "tasks";
        protected override ChatDataTypeProperty[] GetProperties()
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
                    Apply = (dataItem, value) =>
                    {
                        dataItem.Name = value;
                    }
                },
            ];
        }
    }
}
