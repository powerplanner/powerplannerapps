using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerUWP.Flyouts;
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
        private void Button_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            // Dynamically create the context menu upon request
            // (This is to improve performance of large lists, so that
            // a context menu and all bindings aren't created until actually requested)
            MenuFlyout flyout = new TaskOrEventFlyout(Item, new TaskOrEventFlyoutOptions
            {
                ShowGoToClass = true
            }).GetFlyout();

            // Show context flyout
            if (args.TryGetPosition(sender, out Point point))
            {
                flyout.ShowAt(sender as FrameworkElement, point);
            }
            else
            {
                flyout.ShowAt(sender as FrameworkElement);
            }
        }

        public string TelemetryOnClickEventName { get; set; }

    }
}
