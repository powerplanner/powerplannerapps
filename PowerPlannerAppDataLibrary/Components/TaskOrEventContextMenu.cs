using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public static class TaskOrEventContextMenu
    {
        public static ContextMenu Generate(ViewItemTaskOrEvent item, BaseMainScreenViewModelDescendant viewModel)
        {
            ContextMenuSubItem weightsMenu = null;
            try
            {
                if (IsClassValid(item))
                {
                    var weights = ViewModels.MainWindow.MainScreen.TasksOrEvents.AddTaskOrEventViewModel.GetWeightCategories(item.Class, viewModel);

                    weightsMenu = new ContextMenuSubItem
                    {
                        Text = PowerPlannerResources.GetString("ContextFlyout_GradeWeightCategories"),
                        Glyph = MaterialDesign.MaterialDesignIcons.Calculate
                    };

                    weightsMenu.Items.AddRange(weights.Select(weight => new ContextMenuRadioItem
                    {
                        Text = weight.Name,
                        GroupName = "GradeWeightCategories",
                        IsChecked = weight.Identifier == item.WeightCategory?.Identifier,
                        Click = () =>
                        {
                            if (item.WeightCategory?.Identifier != weight.Identifier)
                            {
                                DataChanges changes = new DataChanges();

                                var dataItem = (DataItemMegaItem)item.DataItem;
                                dataItem.WeightCategoryIdentifier = weight.Identifier;

                                changes.Add(dataItem);

                                _ = PowerPlannerApp.Current.SaveChanges(changes);

                                Telemetry_TrackContextEvent("ChangeWeight");
                            }
                        }
                    }));
                }
            }
            catch (Exception weightsEx)
            {
                TelemetryExtension.Current?.TrackException(weightsEx);
            }

            return new ContextMenu
            {
                Items =
                    {
                        // Only show "Toggle Complete" item if it's a task
                        item.IsTask ? new ContextMenuItem
                        {
                            Text = PowerPlannerResources.GetString(item.IsComplete ? "ContextFlyout_MarkIncomplete" : "ContextFlyout_MarkComplete"),
                            Glyph = item.IsComplete ? MaterialDesign.MaterialDesignIcons.Close : MaterialDesign.MaterialDesignIcons.Check,
                            Click = () =>
                            {
                                // New percent complete toggles completion; If there's any progress, remove it, otherwise set it to complete
                                double newPercentComplete = item.IsComplete ? 0 : 1;
                                viewModel.MainScreenViewModel.SetTaskPercentComplete(item, newPercentComplete);
                                Telemetry_TrackContextEvent("ToggleComplete");
                            }
                        } : null,

                        item.IsTask ? new ContextMenuSeparator() : null,
                        
                        // Edit
                        new ContextMenuItem
                        {
                            Text = PowerPlannerResources.GetString("ContextFlyout_Edit"),
                            Glyph = MaterialDesign.MaterialDesignIcons.Edit,
                            Click = () =>
                            {
                                viewModel.MainScreenViewModel.EditTaskOrEvent(item);
                                Telemetry_TrackContextEvent("Edit");
                            }
                        },

                        // Duplicate
                        new ContextMenuItem
                        {
                            Text = PowerPlannerResources.GetString("ContextFlyout_Duplicate"),
                            Glyph = MaterialDesign.MaterialDesignIcons.ContentCopy,
                            Click = () =>
                            {
                                viewModel.MainScreenViewModel.DuplicateAndEditTaskOrEvent(item);
                                Telemetry_TrackContextEvent("Duplicate");
                            }
                        },

                        // Delete
                        new ContextMenuItem
                        {
                            Text = PowerPlannerResources.GetString("ContextFlyout_Delete"),
                            Glyph = MaterialDesign.MaterialDesignIcons.Delete,
                            Click = async () =>
                            {
                                if (await PowerPlannerApp.ConfirmDeleteAsync())
                                {
                                    await viewModel.MainScreenViewModel.DeleteItem(item);
                                }
                                Telemetry_TrackContextEvent("Delete");
                            },
                            Style = ContextMenuItemStyle.Destructive
                        },

                        new ContextMenuSeparator(),

                        // Class-specific weight categories sub-menu
                        weightsMenu,

                        // Convert task/event
                        new ContextMenuItem
                        {
                            Text = !item.IsTask ? PowerPlannerResources.GetString("String_ConvertToTask") : PowerPlannerResources.GetString("String_ConvertToEvent"),
                            Glyph = MaterialDesign.MaterialDesignIcons.SwapHoriz,
                            Click = () =>
                            {
                                viewModel.MainScreenViewModel.ConvertTaskOrEventType(item);
                                Telemetry_TrackContextEvent("ConvertType");
                            }
                        },

                        // Go to class
                        IsClassValid(item) ? new ContextMenuSeparator() : null,
                        IsClassValid(item) ? new ContextMenuItem
                        {
                            Text = PowerPlannerResources.GetString("ContextFlyout_GoToClass"),
                            Glyph = MaterialDesign.MaterialDesignIcons.Launch,
                            Click = () =>
                            {
                                // Get initial page (if it's a task/event, go to that page)
                                ClassViewModel.ClassPages? initialPage = null;
                                if (item.IsTask) initialPage = ClassViewModel.ClassPages.Tasks;
                                else if (!item.IsTask) initialPage = ClassViewModel.ClassPages.Events;

                                // Navigate to class
                                viewModel.MainScreenViewModel.ViewClass(item.Class, initialPage);

                                Telemetry_TrackContextEvent("GoToClass");
                            }
                        } : null
                    }
            };
        }

        // Returns true if item's Class is a valid class (i.e. not "No Class")
        private static bool IsClassValid(ViewItemTaskOrEvent item)
        {
            return item.Class != PowerPlannerApp.Current.GetMainScreenViewModel()?.CurrentSemester.NoClassClass;
        }

        private static void Telemetry_TrackContextEvent(string action)
        {
            TelemetryExtension.Current?.TrackEvent("ContextMenuClicked", new Dictionary<string, string>()
            {
                ["ItemType"] = "TaskOrEvent", // Yes, leave this as-is. In the future, grades will probably have context menus and ItemType would be Grade in that case
                ["Action"] = action
            });
        }
    }
}
