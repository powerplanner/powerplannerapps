using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerUWP.Views.HomeworkViews;
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
        public BaseViewItemHomeworkExam Item => DataContext as BaseViewItemHomeworkExam;

        public MainCalendarItemView()
        {
            this.InitializeComponent();
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PowerPlannerApp.Current.GetMainScreenViewModel()?.ShowItem(DataContext as BaseViewItemHomeworkExam);

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

        public string TelemetryOnClickEventName { get; set; }
    }
}
