using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views
{
    public sealed partial class DifferentSemesterOverlayControl : UserControl
    {
        public DifferentSemesterOverlayControl()
        {
            this.InitializeComponent();
        }

        private void RectangleBackground_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void ButtonViewSemesters_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var model = PowerPlannerApp.Current.GetMainScreenViewModel();

                model.SelectedItem = PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Years;
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
