using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal class AddOrEditWeightCategoryDataItem : BaseAddOrEditDataItem<DataItemWeightCategory>
    {
        public override string DataTypeName => "weight_categories";
        protected override BaseAddOrEditItemProperty[] GetProperties()
        {
            return
            [
                new Property<Guid>
                {
                    Name = "id",
                    Description = "The ID of the weight category.",
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
                    Name = "classId",
                    Description = "The ID of the class that the weight category belongs to.",
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
                    Description = "The name of the weight category.",
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
                    Name = "weightValue",
                    Description = "The weight value of the weight category. This must be 0 or greater. Typical values are 20, 50, etc. Weight totals can sum to greater than 100, some students have weight categories where each one is worth 100 or 200, and the total is 500. When the value is 0, it's treated as an extra credit weight category.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        if (value < 0)
                        {
                            throw new ArgumentOutOfRangeException("WeightValue", "WeightValue must be 0 or greater.");
                        }
                        dataItem.WeightValue = value;
                    }
                }
            ];
        }
    }
}
