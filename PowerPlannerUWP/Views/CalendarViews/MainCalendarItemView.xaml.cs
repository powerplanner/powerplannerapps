using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.CalendarViews
{
    public sealed partial class MainCalendarItemView : UserControl
    {
        public ViewItemTaskOrEvent Item => DataContext as ViewItemTaskOrEvent;

        public MainCalendarItemView()
        {
            this.InitializeComponent();
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PowerPlannerApp.Current.GetMainScreenViewModel()?.ShowItem(Item);

            e.Handled = true;

            if (TelemetryOnClickEventName != null)
            {
                TelemetryExtension.Current?.TrackEvent(TelemetryOnClickEventName);
            }
        }

        private void UserControl_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            try
            {
                args.Data.Properties.Add("ViewItem", DataContext);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void ContextMenu_Edit(object sender, RoutedEventArgs e)
        {
            PowerPlannerApp.Current.GetMainScreenViewModel()?.EditTaskOrEvent(Item);
        }

        private void ContextMenu_Duplicate(object sender, RoutedEventArgs e)
        {
            PowerPlannerApp.Current.GetMainScreenViewModel()?.DuplicateTaskOrEvent(Item);
        }

        private async void ContextMenu_Delete(object sender, RoutedEventArgs e)
        {
            if (await App.ConfirmDelete(LocalizedResources.GetString("String_ConfirmDeleteItemMessage"), LocalizedResources.GetString("String_ConfirmDeleteItemHeader")))
            {
                PowerPlannerApp.Current.GetMainScreenViewModel()?.DeleteItem(Item);
            }
        }

        private void ContextMenu_ConvertType(object sender, RoutedEventArgs e)
        {
            PowerPlannerApp.Current.GetMainScreenViewModel()?.ConvertTaskOrEventType(Item);
        }

        private void ContextMenu_ToggleComplete(object sender, RoutedEventArgs e)
        {
            // New percent complete toggles completion; If there's any progress, remove it, otherwise set it to complete
            double newPercentComplete = Item.PercentComplete == 0 ? 1 : 0;
            PowerPlannerApp.Current.GetMainScreenViewModel()?.SetTaskOrEventPercentComplete(Item, newPercentComplete);
        }

        public string TelemetryOnClickEventName { get; set; }
    }
}
