using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    internal class AddOrEditYearDataItem : BaseAddOrEditDataItem<DataItemYear>
    {
        public override string DataTypeName => "years";
        protected override BaseAddOrEditItemProperty[] GetProperties()
        {
            return
            [
                new Property<Guid>
                {
                    Name = "id",
                    Description = "The ID of the year to edit.",
                    WhenAdding = PropertyPresence.NotPresent,
                    WhenEditing = PropertyPresence.Required,
                    RequireNonDefaultValueIfSet = true,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.Identifier = value;
                    }
                },

                new Property<string>
                {
                    Name = "name",
                    Description = "The name of the year.",
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
                    Description = "Details for the year.",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    MaxLength = 4000,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.Details = value;
                    }
                },

                new Property<double>
                {
                    Name = "overriddenGpa",
                    Description = "The overridden GPA for the year (ex: 3.0 or 4.0). To remove the overridden GPA, send -1.",
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
                    Description = "The overridden credits for the year. To remove the overridden credits, send -1.",
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
