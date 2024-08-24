using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using System;
using Vx.Views;
using Vx.Views.DragDrop;

namespace PowerPlannerAppDataLibrary.Helpers
{
    public static class DragDropTaskOrEventHelper
    {
        public static T AllowDropTaskOrEventOnDate<T>(this T view, DateTime dateOfThisSurface) where T : View
        {
            view.AllowDrop = true;
            view.DragOver = e => HandleDragOverDate(dateOfThisSurface, e);
            view.Drop = e => HandleDropOnDate(dateOfThisSurface, e);

            return view;
        }

        public static T AllowDragViewItem<T>(this T view, BaseViewItem item) where T : View
        {
            view.CanDrag = true;
            view.DragStarting = e =>
            {
                e.Data.Properties.Add("ViewItem", item);
            };

            return view;
        }

        private static bool TryGetViewItemTaskOrEvent(this DragEventArgs e, out ViewItemTaskOrEvent item)
        {
            if (e.Data.Properties.TryGetValue("ViewItem", out object o) && o is ViewItemTaskOrEvent draggedTaskOrEvent)
            {
                item = draggedTaskOrEvent;
                return true;
            }

            item = null;
            return false;
        }

        private static bool TryGetViewItemMegaItem(this DragEventArgs e, out BaseViewItemMegaItem item)
        {
            if (e.Data.Properties.TryGetValue("ViewItem", out object o) && o is BaseViewItemMegaItem megaItem)
            {
                item = megaItem;
                return true;
            }

            item = null;
            return false;
        }

        private static bool IsDuplicate(this DragEventArgs e)
        {
            return (e.Modifiers & DragDropModifiers.Control) != 0;  // Duplicate if holding Ctrl key
        }

        private static void HandleDragOverDate(DateTime dateOfThisSurface, DragEventArgs e)
        {
            if (e.TryGetViewItemTaskOrEvent(out ViewItemTaskOrEvent draggedTaskOrEvent))
            {
                if (e.IsDuplicate())
                {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                }
                else if (draggedTaskOrEvent.EffectiveDateForDisplayInDateBasedGroups.Date != dateOfThisSurface.Date)
                {
                    e.AcceptedOperation = DataPackageOperation.Move;
                }
                else
                {
                    e.AcceptedOperation = DataPackageOperation.None;
                }
            }
        }

        private static void HandleDropOnDate(DateTime dateOfThisSurface, DragEventArgs e)
        {
            try
            {
                if (e.TryGetViewItemTaskOrEvent(out ViewItemTaskOrEvent draggedTaskOrEvent))
                {
                    if (e.IsDuplicate())
                    {
                        PowerPlannerApp.Current.GetMainScreenViewModel()?.DuplicateTaskOrEvent(draggedTaskOrEvent, dateOfThisSurface.Date);
                    }
                    else
                    {
                        _ = CalendarViewModel.MoveItem(draggedTaskOrEvent, dateOfThisSurface.Date);
                    }
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public static T AllowDropMegaItemOnWeight<T>(this T view, ViewItemWeightCategory weight) where T : View
        {
            view.AllowDrop = true;
            view.DragOver = e => HandleDragOverWeight(weight, e);
            view.Drop = e => HandleDropOnWeight(weight, e);

            return view;
        }

        private static void HandleDragOverWeight(ViewItemWeightCategory weight, DragEventArgs e)
        {
            if (e.TryGetViewItemMegaItem(out BaseViewItemMegaItem item) && (item is ViewItemGrade || item is ViewItemTaskOrEvent))
            {
                if (e.IsDuplicate())
                {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                }
                else if (item.WeightCategory != weight)
                {
                    e.AcceptedOperation = DataPackageOperation.Move;
                }
                else
                {
                    e.AcceptedOperation = DataPackageOperation.None;
                }
            }
        }

        private static void HandleDropOnWeight(ViewItemWeightCategory weight, DragEventArgs e)
        {
            try
            {
                if (e.TryGetViewItemMegaItem(out BaseViewItemMegaItem item))
                {
                    if (item is ViewItemTaskOrEvent taskItem)
                    {
                        if (e.IsDuplicate())
                        {
                            PowerPlannerApp.Current.GetMainScreenViewModel()?.DuplicateTaskOrEvent(taskItem, weightCategoryIdentifier: weight.Identifier);
                        }
                        else if (item.WeightCategory?.Identifier != weight.Identifier)
                        {
                            DataChanges changes = new DataChanges();

                            var dataItem = (DataItemMegaItem)item.DataItem;
                            dataItem.WeightCategoryIdentifier = weight.Identifier;

                            changes.Add(dataItem);

                            _ = PowerPlannerApp.Current.SaveChanges(changes);
                        }
                    }
                    else if (item is ViewItemGrade grade)
                    {
                        if (e.IsDuplicate())
                        {
                            PowerPlannerApp.Current.GetMainScreenViewModel()?.DuplicateGrade(grade, weightCategoryIdentifier: weight.Identifier);
                        }
                        else if (item.WeightCategory?.Identifier != weight.Identifier)
                        {
                            DataChanges changes = new DataChanges();

                            var dataItem = (DataItemGrade)item.DataItem;
                            dataItem.UpperIdentifier = weight.Identifier;

                            changes.Add(dataItem);

                            _ = PowerPlannerApp.Current.SaveChanges(changes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
