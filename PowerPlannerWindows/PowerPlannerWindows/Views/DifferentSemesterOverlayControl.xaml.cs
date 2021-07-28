using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

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
