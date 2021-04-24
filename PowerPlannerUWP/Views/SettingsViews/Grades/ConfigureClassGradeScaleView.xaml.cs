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
    public sealed partial class ConfigureClassGradeScaleView : PopupViewHostGeneric
    {
        public new ConfigureClassGradeScaleViewModel ViewModel
        {
            get { return base.ViewModel as ConfigureClassGradeScaleViewModel; }
            set { base.ViewModel = value; }
        }

        public ConfigureClassGradeScaleView()
        {
            this.InitializeComponent();

            Title = PowerPlannerResources.GetString("ConfigureClassGrades_Items_GradeScale.Title").ToUpper();
        }

        public override void OnViewModelSetOverride()
        {
            ViewModel.ShowSaveScalePopupInSeparatePopupPane = true;

            base.OnViewModelSetOverride();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
        }

        private void ButtonSaveGradeScale_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveGradeScale();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void IconDeleteGradeScale_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //ViewModel.RemoveGradeScale((sender as FrameworkElement).DataContext as PowerPlannerSending.GradeScale);
        }

        private void ButtonAddGradeScale_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddGradeScale();
        }
    }
}
