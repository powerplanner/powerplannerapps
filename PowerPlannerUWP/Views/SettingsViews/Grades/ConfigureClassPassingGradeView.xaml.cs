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
    public sealed partial class ConfigureClassPassingGradeView : PopupViewHostGeneric
    {
        public new ConfigureClassPassingGradeViewModel ViewModel
        {
            get { return base.ViewModel as ConfigureClassPassingGradeViewModel; }
            set { base.ViewModel = value; }
        }

        public ConfigureClassPassingGradeView()
        {
            this.InitializeComponent();

            Title = PowerPlannerResources.GetString("Settings_GradeOptions_ListItemPassingGrade.Title").ToUpper();
            TextBoxPassingGrade.Header = PowerPlannerResources.GetString("Settings_GradeOptions_ListItemPassingGrade.Title");

            int exGrade = 60;
            TextBoxPassingGrade.PlaceholderText = PowerPlannerResources.GetExamplePlaceholderString(exGrade.ToString());
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
        }

        private void TextBoxPassingGrade_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                ViewModel.Save();
            }
        }

        private void TextBoxPassingGrade_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBoxPassingGrade.SelectAll();
        }
    }
}
