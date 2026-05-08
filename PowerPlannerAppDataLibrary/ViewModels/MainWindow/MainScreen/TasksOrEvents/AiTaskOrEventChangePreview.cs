using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.AiChangeHelpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents
{
    internal static class AiTaskOrEventChangePreview
    {
        internal class CurrentItems
        {
            internal SemesterItemsViewGroup SemesterItems { get; set; }
            internal ViewItemSemester Semester { get; set; }
            internal IList<ViewItemClass> Classes { get; set; }
        }

        internal static View RenderChangeItem(AiProposedChange change, CurrentItems currentItems)
        {
            Color accentColor;
            string operationLabel;
            switch (change.Operation)
            {
                case AiChangeOperation.Add:
                    accentColor = Color.FromArgb(255, 76, 175, 80); // Green
                    operationLabel = "ADD";
                    break;
                case AiChangeOperation.Edit:
                    accentColor = Color.FromArgb(255, 255, 193, 7); // Yellow/Orange
                    operationLabel = "EDIT";
                    break;
                case AiChangeOperation.Delete:
                    accentColor = Color.FromArgb(255, 244, 67, 54); // Red
                    operationLabel = "DELETE";
                    break;
                default:
                    accentColor = Color.Gray;
                    operationLabel = "";
                    break;
            }

            // Find existing item for edit/delete context
            BaseViewItemMegaItem existingItem = null;
            if (change.ExistingItemId.HasValue)
            {
                existingItem = currentItems.SemesterItems.Items.FirstOrDefault(i => i.Identifier == change.ExistingItemId.Value);
            }

            // Build the item name - use existing name if the change doesn't provide one
            string itemName = change.Name ?? (existingItem?.Name) ?? "Unknown item";

            var contentLayout = new LinearLayout
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    // Item name and operation badge
                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = operationLabel,
                                FontSize = 10,
                                TextColor = accentColor,
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(0, 0, 6, 0),
                                WrapText = false
                            },

                            new TextBlock
                            {
                                Text = itemName,
                                FontWeight = FontWeights.SemiBold,
                                WrapText = true,
                                VerticalAlignment = VerticalAlignment.Center
                            }.LinearLayoutWeight(1)
                        }
                    }
                }
            };

            // For edits, show before → after details
            if (change.Operation == AiChangeOperation.Edit && existingItem != null)
            {
                var detailLines = GetEditDetails(change, existingItem, currentItems);
                foreach (var line in detailLines)
                {
                    contentLayout.Children.Add(line);
                }
            }
            // For adds, show key details
            else if (change.Operation == AiChangeOperation.Add)
            {
                var detailLines = GetAddDetails(change, currentItems);
                foreach (var line in detailLines)
                {
                    contentLayout.Children.Add(line);
                }
            }
            // For deletes, show existing item info
            else if (change.Operation == AiChangeOperation.Delete && existingItem != null)
            {
                var datePart = existingItem.DateInSchoolTime.ToString("MMM d, yyyy");
                contentLayout.Children.Add(new TextBlock
                {
                    Text = datePart,
                    FontSize = Theme.Current.CaptionFontSize,
                    TextColor = Theme.Current.SubtleForegroundColor,
                    Margin = new Thickness(0, 2, 0, 0)
                });
            }

            return contentLayout;
        }

        private static List<View> GetEditDetails(AiProposedChange change, BaseViewItemMegaItem existingItem, CurrentItems currentItems)
        {
            var lines = new List<View>();

            // Name change
            if (!string.IsNullOrEmpty(change.Name) && change.Name != existingItem.Name)
            {
                lines.Add(AiChangePreviewHelper.RenderChange("Name", existingItem.Name, change.Name));
            }

            // Class change
            if (change.ClassId.HasValue)
            {
                string oldClassName = null;
                string newClassName = null;

                if (existingItem is ViewItemTaskOrEvent taskOrEvent)
                {
                    oldClassName = taskOrEvent.Class?.Name ?? "No Class";
                }

                if (change.ClassId.Value == currentItems.Semester.Identifier)
                {
                    newClassName = "No Class";
                }
                else
                {
                    var newClass = currentItems.Classes.FirstOrDefault(c => c.Identifier == change.ClassId.Value);
                    newClassName = newClass?.Name ?? "Unknown Class";
                }

                if (oldClassName != null && oldClassName != newClassName)
                {
                    lines.Add(AiChangePreviewHelper.RenderChange("Class", oldClassName, newClassName));
                }
            }

            // Date change
            if (change.Date.HasValue)
            {
                var oldDate = existingItem.DateInSchoolTime.ToString("MMM d, yyyy");
                var newDate = change.Date.Value.ToString("MMM d, yyyy");
                if (oldDate != newDate)
                {
                    lines.Add(AiChangePreviewHelper.RenderChange("Date", oldDate, newDate));
                }
            }

            // Time change
            if (!string.IsNullOrEmpty(change.Time))
            {
                lines.Add(AiChangePreviewHelper.RenderChange("Time", "TODO", change.Time));
            }

            // Details change
            if (change.Details != null && change.Details != existingItem.Details)
            {
                if (string.IsNullOrEmpty(existingItem.Details))
                {
                    lines.Add(AiChangePreviewHelper.RenderChange("Details", "", change.Details));
                }
                else
                {
                    lines.Add(AiChangePreviewHelper.RenderChange("Details", existingItem.Details, change.Details));
                }
            }

            // Percent complete change
            if (change.PercentComplete.HasValue)
            {
                double existingPercent = existingItem is ViewItemTaskOrEvent taskOrEvent ? taskOrEvent.PercentComplete : 0;
                lines.Add(AiChangePreviewHelper.RenderChange("Progress", $"{(int)(existingPercent * 100)}%", $"{(int)(change.PercentComplete.Value * 100)}%"));
            }

            if (existingItem is ViewItemHoliday holiday)
            {
                if (change.EndDate.HasValue)
                {
                    var oldDate = holiday.EndTime.ToString("MMM d, yyyy");
                    var newDate = change.EndDate.Value.ToString("MMM d, yyyy");
                    if (oldDate != newDate)
                    {
                        lines.Add(AiChangePreviewHelper.RenderChange("End", oldDate, newDate));
                    }
                }
            }

            if (lines.Count == 0)
            {
                lines.Add(new TextBlock { Text = "(minor changes)" });
            }

            return lines;
        }

        private static List<View> GetAddDetails(AiProposedChange change, CurrentItems currentItems)
        {
            var lines = new List<View>();

            if (change.Type.HasValue)
            {
                lines.Add(AiChangePreviewHelper.RenderNewProperty("Type", change.Type.Value.ToString()));
            }

            if (change.Type == AiItemType.Task || change.Type == AiItemType.Event)
            {
                // Get the class from current items
                var c = change.ClassId == currentItems.Semester.Identifier ? currentItems.Semester.NoClassClass : currentItems.Classes.FirstOrDefault(c => c.Identifier == change.ClassId);
                var cName = c != null ? c.Name : "Unknown Class";
                lines.Add(AiChangePreviewHelper.RenderNewProperty("Class", cName));
            }

            if (change.Date.HasValue)
            {
                var datePart = change.Date.Value.ToString("MMM d, yyyy");
                if (!string.IsNullOrEmpty(change.Time) && change.Time.ToLowerInvariant() != "allday")
                {
                    lines.Add(AiChangePreviewHelper.RenderNewProperty("Date", $"{datePart}, {change.Time}"));
                }
                else
                {
                    lines.Add(AiChangePreviewHelper.RenderNewProperty("Date", datePart));
                }
            }

            if (change.Type == AiItemType.Holiday)
            {
                if (change.EndDate != null && change.EndDate.Value != change.Date)
                {
                    lines.Add(AiChangePreviewHelper.RenderNewProperty("End", change.EndDate.Value.ToString("MMM d, yyyy")));
                }
            }

            if (!string.IsNullOrEmpty(change.Details))
            {
                lines.Add(AiChangePreviewHelper.RenderNewProperty("Details", change.Details));
            }

            return lines;
        }
    }
}
