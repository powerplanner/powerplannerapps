using PowerPlannerAppDataLibrary.App;
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
        public static T AllowDropTaskOrEvent<T>(this T view, DateTime dateOfThisSurface) where T : View
        {
            view.AllowDrop = true;
            view.DragOver = e => HandleDragOver(dateOfThisSurface, e);
            view.Drop = e => HandleDrop(dateOfThisSurface, e);

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

        private static void HandleDragOver(DateTime dateOfThisSurface, DragEventArgs e)
        {
            if (e.Data.Properties.TryGetValue("ViewItem", out object o) && o is ViewItemTaskOrEvent draggedTaskOrEvent)
            {
                bool duplicate = (e.Modifiers & DragDropModifiers.Control) != 0;  // Duplicate if holding Ctrl key

                if (duplicate)
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

        private static void HandleDrop(DateTime dateOfThisSurface, DragEventArgs e)
        {
            try
            {
                if (e.Data.Properties.TryGetValue("ViewItem", out object o) && o is ViewItemTaskOrEvent draggedTaskOrEvent)
                {
                    bool duplicate = (e.Modifiers & DragDropModifiers.Control) != 0;  // Duplicate if holding Ctrl key

                    if (duplicate)
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
    }
}
