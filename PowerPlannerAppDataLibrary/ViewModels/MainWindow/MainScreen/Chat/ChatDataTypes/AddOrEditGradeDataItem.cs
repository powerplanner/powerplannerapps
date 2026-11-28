using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.DataLayer.DataItems.BaseItems;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatItemTypes;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Chat.ChatDataTypes
{
    // Grades are a bit special, since they can either be a MegaItem (which lives under the class), or a grade item (which lives under the weight).
    // We'll abstract this detail, and keep it simple...
    // For the agent, grades always have a weight identifier. Our backend code will map it correctly.
    // When an item is a MegaItem, "deleting" the grade doesn't actually delete the grade, it just removes the WeightIdentifier from the grade. But for the agent, it would just call "Delete grade" and we'll figure that out on the backend.
    internal class AddOrEditGradeDataItem : BaseAddOrEditItem
    {
        public BaseViewItemMegaItem ExistingItem { get; set; }

        public bool IsEditingExistingTaskOrEvent => ExistingItem is ViewItemTaskOrEvent;

        private class Property<T> : BaseAddOrEditDataItemProperty<BaseDataItemHomeworkExamGrade, T>
        {

        }

        public override string DataTypeName => "grades";
        protected override BaseAddOrEditItemProperty[] GetProperties()
        {
            return
            [
                new Property<Guid>
                {
                    Name = "id",
                    Description = "The ID of the grade to edit.",
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
                    Description = "The name of the grade.",
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

                new Property<Guid>
                {
                    Name = "weightId",
                    Description = "The ID of the weight that the grade is part of.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    RequireNonDefaultValueIfSet = true,
                    Apply = (dataItem, value) =>
                    {
                        if (dataItem is DataItemMegaItem megaItem)
                        {
                            megaItem.WeightCategoryIdentifier = value;
                        }
                        else
                        {
                            dataItem.UpperIdentifier = value;
                        }
                    }
                },

                new Property<DateOnly>
                {
                    Name = "date",
                    Description = "The date of the grade.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        if (ExistingItem is ViewItemTaskOrEvent originalTaskOrEvent)
                        {
                            // Preserve the time offset of the date
                            dataItem.Date = DateTime.SpecifyKind(value.ToDateTime(TimeOnly.FromTimeSpan(originalTaskOrEvent.DateInSchoolTime.TimeOfDay)), DateTimeKind.Utc);
                        }
                        else
                        {
                            dataItem.Date = DateTime.SpecifyKind(value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                        }
                    }
                },

                new Property<double>
                {
                    Name = "gradeReceived",
                    Description = "The number of points received for the grade. To clear the grade received and make this a not-yet-graded item, set this to -50.",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.GradeReceived = value;
                    }
                },

                new Property<double>
                {
                    Name = "gradeTotal",
                    Description = "The total number of points possible for the grade.",
                    WhenAdding = PropertyPresence.Required,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.GradeTotal = value;
                    }
                },

                new Property<bool>
                {
                    Name = "isDropped",
                    Description = "Whether the grade is dropped (ignored from the overall grade calculation).",
                    WhenAdding = PropertyPresence.Optional,
                    WhenEditing = PropertyPresence.Optional,
                    Apply = (dataItem, value) =>
                    {
                        dataItem.IsDropped = value;
                    }
                }
            ];
        }

        public override BaseDataItem CreateDataItem(AddOrEditMode mode)
        {
            BaseDataItemHomeworkExamGrade dataItem;
            if (mode == AddOrEditMode.Add)
            {
                dataItem = new DataItemGrade();
            }
            else
            {
                if (IsEditingExistingTaskOrEvent)
                {
                    dataItem = new DataItemMegaItem();
                }
                else
                {
                    dataItem = new DataItemGrade();
                }
            }

            PopulateDataItem(dataItem, mode);

            return dataItem;
        }
    }
}
