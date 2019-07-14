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
    public sealed partial class ConfigureClassCreditsView : PopupViewHostGeneric
    {
        public new ConfigureClassCreditsViewModel ViewModel
        {
            get { return base.ViewModel as ConfigureClassCreditsViewModel; }
            set { base.ViewModel = value; }
        }

        public ConfigureClassCreditsView()
        {
            this.InitializeComponent();

            Title = PowerPlannerResources.GetString("ClassPage_TextBoxEditCredits.Header").ToUpper();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
        }

        private void TextBoxCredits_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                ViewModel.Save();
            }
        }
    }
}
