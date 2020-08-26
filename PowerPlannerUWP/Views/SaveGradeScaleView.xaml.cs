using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SaveGradeScaleView : PopupViewHostGeneric
    {
        public new SaveGradeScaleViewModel ViewModel
        {
            get { return base.ViewModel as SaveGradeScaleViewModel; }
            set { base.ViewModel = value; }
        }

        public SaveGradeScaleView()
        {
            this.InitializeComponent();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            this.Save();
        }

        private void tbName_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                Save();
            }
        }

        private void Save()
        {
            ViewModel.Save();
        }

        private void tbName_Loaded(object sender, RoutedEventArgs e)
        {
            tbName.Focus(FocusState.Programmatic);
        }
    }
}
