using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.SettingsViews.Grades
{
    public sealed partial class ConfigureClassAverageGradesView : PopupViewHostGeneric
    {
        public new ConfigureClassAverageGradesViewModel ViewModel
        {
            get { return base.ViewModel as ConfigureClassAverageGradesViewModel; }
            set { base.ViewModel = value; }
        }

        public ConfigureClassAverageGradesView()
        {
            this.InitializeComponent();

            Title = PowerPlannerResources.GetString("ClassPage_TextBlockAverageGradesHelpHeader.Text").ToUpper();
        }
    }
}
