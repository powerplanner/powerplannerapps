using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal class AddOrEditHolidayDataItem : BaseAddOrEditDataItem<DataItemMegaItem>
    {
        public override string DataTypeName => "holidays";
        protected override BaseAddOrEditItemProperty[] GetProperties()
        {
            return
            [
                new Property<Guid>
                {
                    Name = "id",
                    Description = "The ID of the holiday to edit.",
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
                    Name = "semesterId",
                    Description = "The ID of the semester that the holiday is part of.",
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
                    Description = "The name of the holiday.",
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

                new Property<DateOnly>
                {
                    Name = "startDate",
                    Description = "The start date of the holiday.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.Date = DateTime.SpecifyKind(value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                    }
                },

                new Property<DateOnly>
                {
                    Name = "endDate",
                    Description = "The end date of the holiday.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.EndTime = DateTime.SpecifyKind(value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                    }
                },
            ];
        }

        public override DataItemMegaItem CreateDataItem(AddOrEditMode mode)
        {
            var dataItem = base.CreateDataItem(mode);
            dataItem.MegaItemType = PowerPlannerSending.MegaItemType.Holiday;
            return dataItem;
        }
    }
}
