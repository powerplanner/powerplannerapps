using InterfacesUWP;
using PowerPlannerUWP.Views;
using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Homework;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewHomeworkView : PopupViewHostGeneric
    {
        public new ViewHomeworkViewModel ViewModel
        {
            get { return base.ViewModel as ViewHomeworkViewModel; }
            set { base.ViewModel = value; }
        }

        public ViewHomeworkView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            switch (ViewModel.Type)
            {
                case ViewHomeworkViewModel.ItemType.Homework:
                    this.Title = LocalizedResources.GetString("String_ViewTask").ToUpper();
                    break;

                case ViewHomeworkViewModel.ItemType.Exam:
                    this.Title = LocalizedResources.GetString("String_ViewEvent").ToUpper();
                    break;
            }

            if ((ViewModel.Type == ViewHomeworkViewModel.ItemType.Homework) && !ViewModel.IsUnassigedMode)
            {
                completionSlider.Visibility = Visibility.Visible;
            }
            else if (ViewModel.IsUnassigedMode)
            {
                ButtonConvertToGrade.Visibility = Visibility.Visible;
            }
        }

        private void ButtonDelete_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PopupMenuConfirmDelete.Show(ButtonDelete, ViewModel.Delete);
        }

        private void ButtonEdit_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Edit();
        }

        private void completionSlider_OnValueChangedByUser(object sender, double value)
        {
            ViewModel.SetPercentComplete(value);
        }

        private void ButtonConvertToGrade_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ConvertToGrade();
        }

        private void ButtonConvertToGrade_Loaded(object sender, RoutedEventArgs e)
        {
            if (ButtonConvertToGrade.Visibility == Visibility.Visible)
            {
                ButtonConvertToGrade.Focus(FocusState.Programmatic);
            }
        }
    }
}
