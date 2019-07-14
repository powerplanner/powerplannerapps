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
    public sealed partial class ConfigureClassGradesListView : PopupViewHostGeneric
    {
        public new ConfigureClassGradesListViewModel ViewModel
        {
            get { return base.ViewModel as ConfigureClassGradesListViewModel; }
            set { base.ViewModel = value; }
        }

        public ConfigureClassGradesListView()
        {
            this.InitializeComponent();

            ButtonCredits.Title = PowerPlannerResources.GetString("ClassPage_TextBoxEditCredits.Header");
            ButtonAverageGrades.Title = PowerPlannerResources.GetString("ClassPage_TextBlockAverageGradesHelpHeader.Text");
            ButtonRoundGradesUp.Title = PowerPlannerResources.GetString("ClassPage_TextBlockRoundGradesUpHelpHeader.Text");
        }

        private void ButtonCredits_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ConfigureCredits();
        }

        private void ButtonWeightCategories_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ConfigureWeightCategories();
        }

        private void ButtonGradeScale_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ConfigureGradeScale();
        }

        private void ButtonAverageGrades_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ConfigureAverageGrades();
        }

        private void ButtonRoundGradesUp_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ConfigureRoundGradesUp();
        }

        private void ButtonGpaType_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ConfigureGpaType();
        }

        private void ButtonPassingGrade_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ConfigurePassingGrade();
        }
    }
}
