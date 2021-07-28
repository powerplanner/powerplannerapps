using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Grade;
using System;
using Microsoft.UI.Xaml;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewGradeView : PopupViewHostGeneric
    {
        public new ViewGradeViewModel ViewModel
        {
            get { return base.ViewModel as ViewGradeViewModel; }
            set { base.ViewModel = value; }
        }

        public ViewGradeView()
        {
            this.InitializeComponent();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Edit();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void ButtonDrop_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Grade.IsDropped)
            {
                ViewModel.UndropGrade();
            }
            else
            {
                ViewModel.DropGrade();
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Delete();
        }

        private void Edit()
        {
            ViewModel.Edit();
        }
    }
}
