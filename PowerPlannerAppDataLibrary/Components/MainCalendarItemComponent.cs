using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.Helpers;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.Components
{
    public class MainCalendarItemComponent : VxComponent
    {
        public ViewItemTaskOrEvent Item { get; set; }
        public BaseMainScreenViewModelChild ViewModel { get; set; }
        public Action<ViewItemTaskOrEvent> ShowItem { get; set; }

        protected override View Render()
        {
            View content = new Border
            {
                BackgroundColor = Item.Class.Color.ToColor(),
                Content = new TextBlock
                {
                    Text = Item.Name,
                    Margin = new Thickness(6),
                    WrapText = false,
                    TextColor = Color.White,
                    Strikethrough = Item.IsComplete,
                    FontSize = Theme.Current.CaptionFontSize,
                    FontWeight = FontWeights.SemiBold
                }
            };

            if (Item.IsComplete)
            {
                content = new LinearLayout
                {
                    Orientation = Orientation.Horizontal,
                    BackgroundColor = Item.Class.Color.ToColor(),
                    Opacity = 0.7f,
                    Children =
                        {
                            new Border
                            {
                                BackgroundColor = Color.Black,
                                Opacity = 0.3f,
                                Width = 12
                            },

                            content.LinearLayoutWeight(1)
                        }
                };
            }

            content.Tapped = () => ShowItem(Item);

            content.CanDrag = true;
            content.DragStarting = e =>
            {
                e.Data.Properties.Add("ViewItem", Item);
            };



            // Add context menu
            content.ContextMenu = GetContextMenu;

            return content;
        }

        private ContextMenu GetContextMenu()
        {
            ContextMenuSubItem weightsMenu = null;
            try
            {
                if (IsClassValid(Item))
                {
                    var weights = ViewModels.MainWindow.MainScreen.TasksOrEvents.AddTaskOrEventViewModel.GetWeightCategories(Item.Class, ViewModel);

                    weightsMenu = new ContextMenuSubItem
                    {
                        Text = PowerPlannerResources.GetString("ContextFlyout_GradeWeightCategories"),
                        Glyph = MaterialDesign.MaterialDesignIcons.Calculate
                    };

                    weightsMenu.Items.AddRange(weights.Select(weight => new ContextMenuRadioItem
                    {
                        Text = weight.Name,
                        GroupName = "GradeWeightCategories",
                        IsChecked = weight.Identifier == Item.WeightCategory?.Identifier,
                        Click = () =>
                        {
                            if (Item.WeightCategory?.Identifier != weight.Identifier)
                            {
                                DataChanges changes = new DataChanges();

                                var dataItem = (DataItemMegaItem)Item.DataItem;
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
                        Item.IsTask ? new ContextMenuItem
                        {
                            Text = PowerPlannerResources.GetString(Item.IsComplete ? "ContextFlyout_MarkIncomplete" : "ContextFlyout_MarkComplete"),
                            Glyph = MaterialDesign.MaterialDesignIcons.Check,
                            Click = () =>
                            {
                                // New percent complete toggles completion; If there's any progress, remove it, otherwise set it to complete
                                double newPercentComplete = Item.IsComplete ? 0 : 1;
                                ViewModel.MainScreenViewModel.SetTaskPercentComplete(Item, newPercentComplete);
                                Telemetry_TrackContextEvent("ToggleComplete");
                            }
                        } : null,

                        Item.IsTask ? new ContextMenuSeparator() : null,
                        
                        // Edit
                        new ContextMenuItem
                        {
                            Text = PowerPlannerResources.GetString("ContextFlyout_Edit"),
                            Glyph = MaterialDesign.MaterialDesignIcons.Edit,
                            Click = () =>
                            {
                                ViewModel.MainScreenViewModel.EditTaskOrEvent(Item);
                                Telemetry_TrackContextEvent("Edit");
                            }
                        },

                        // Duplicate
                        new ContextMenuItem
                        {
                            Text = PowerPlannerResources.GetString("ContextFlyout_Duplicate"),
                            Glyph = MaterialDesign.MaterialDesignIcons.Copy,
                            Click = () =>
                            {
                                ViewModel.MainScreenViewModel.DuplicateAndEditTaskOrEvent(Item);
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
                                    await ViewModel.MainScreenViewModel.DeleteItem(Item);
                                }
                                Telemetry_TrackContextEvent("Delete");
                            }
                        },

                        new ContextMenuSeparator(),

                        // Class-specific weight categories sub-menu
                        weightsMenu,

                        // Convert task/event
                        new ContextMenuItem
                        {
                            Text = !Item.IsTask ? PowerPlannerResources.GetString("String_ConvertToTask") : PowerPlannerResources.GetString("String_ConvertToEvent"),
                            Glyph = MaterialDesign.MaterialDesignIcons.SwapHoriz,
                            Click = () =>
                            {
                                ViewModel.MainScreenViewModel.ConvertTaskOrEventType(Item);
                                Telemetry_TrackContextEvent("ConvertType");
                            }
                        },

                        // Go to class
                        IsClassValid(Item) ? new ContextMenuSeparator() : null,
                        IsClassValid(Item) ? new ContextMenuItem
                        {
                            Text = PowerPlannerResources.GetString("ContextFlyout_GoToClass"),
                            Glyph = MaterialDesign.MaterialDesignIcons.Launch,
                            Click = () =>
                            {
                                // Get initial page (if it's a task/event, go to that page)
                                ClassViewModel.ClassPages? initialPage = null;
                                if (Item.IsTask) initialPage = ClassViewModel.ClassPages.Tasks;
                                else if (!Item.IsTask) initialPage = ClassViewModel.ClassPages.Events;

                                // Navigate to class
                                ViewModel.MainScreenViewModel.ViewClass(Item.Class, initialPage);

                                Telemetry_TrackContextEvent("GoToClass");
                            }
                        } : null
                    }
            };
        }

        // Returns true if item's Class is a valid class (i.e. not "No Class")
        private bool IsClassValid(ViewItemTaskOrEvent item)
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
