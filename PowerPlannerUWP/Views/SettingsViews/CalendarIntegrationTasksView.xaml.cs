using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using PowerPlannerUWPLibrary.Extensions;
using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CalendarIntegrationTasksView : ViewHostGeneric
    {
        public new CalendarIntegrationTasksViewModel ViewModel
        {
            get { return base.ViewModel as CalendarIntegrationTasksViewModel; }
            set { base.ViewModel = value; }
        }

        public CalendarIntegrationTasksView()
        {
            this.InitializeComponent();
        }
    }
}
