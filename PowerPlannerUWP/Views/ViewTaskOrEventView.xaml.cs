using Windows.UI.Xaml;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.TasksOrEvents;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewTaskOrEventView : PopupViewHostGeneric
    {
        public new ViewTaskOrEventViewModel ViewModel
        {
            get { return base.ViewModel as ViewTaskOrEventViewModel; }
            set { base.ViewModel = value; }
        }

        public ViewTaskOrEventView()
        {
            this.InitializeComponent();
        }

        private void ButtonDelete_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PopupMenuConfirmDelete.Show(SecondaryOptionsButtonContainer, ViewModel.Delete);
        }

        private void ButtonEdit_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.Edit();
        }

        private void completionSlider_OnValueChangedByUser(object sender, double value)
        {
            ViewModel.SetPercentComplete(value);
        }

        private void ButtonAddGrade_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddGrade();
        }

        private void ButtonAddGrade_Loaded(object sender, RoutedEventArgs e)
        {
            if (ButtonAddGrade.Visibility == Visibility.Visible)
            {
                ButtonAddGrade.Focus(FocusState.Programmatic);
            }
        }

        private void ButtonConvertType_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ConvertType();
        }
    }
}
