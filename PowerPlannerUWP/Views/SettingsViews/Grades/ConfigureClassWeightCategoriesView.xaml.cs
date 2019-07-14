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
    public sealed partial class ConfigureClassWeightCategoriesView : PopupViewHostGeneric
    {
        public new ConfigureClassWeightCategoriesViewModel ViewModel
        {
            get { return base.ViewModel as ConfigureClassWeightCategoriesViewModel; }
            set { base.ViewModel = value; }
        }

        public ConfigureClassWeightCategoriesView()
        {
            this.InitializeComponent();

            Title = PowerPlannerResources.GetString("ConfigureClassGrades_Items_WeightCategories.Title").ToUpper();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
        }

        private void ButtonAddWeightCategory_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddWeightCategory();
        }

        private void IconDeleteWeightCategory_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.RemoveWeightCategory((sender as FrameworkElement).DataContext as EditingWeightCategoryViewModel);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }
    }
}
