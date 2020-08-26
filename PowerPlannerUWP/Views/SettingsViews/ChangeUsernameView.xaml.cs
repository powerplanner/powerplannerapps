using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChangeUsernameView : PopupViewHostGeneric
    {
        public new ChangeUsernameViewModel ViewModel
        {
            get { return base.ViewModel as ChangeUsernameViewModel; }
            set { base.ViewModel = value; }
        }

        public ChangeUsernameView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            ViewModel.ActionError += ViewModel_ActionError;

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            UpdateProgress();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsUpdatingUsername):
                    UpdateProgress();
                    break;
            }
        }

        private void ViewModel_ActionError(object sender, string e)
        {
            setError(e);
        }

        private void UpdateProgress()
        {
            if (ViewModel.IsUpdatingUsername)
            {
                disablePage();
                clearError();
            }
            else
            {
                enablePage();
            }
        }

        private void textBoxUsername_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                buttonUpdateUsername_Click(null, null);
            }
        }

        private void buttonUpdateUsername_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Update();
        }

        private void setError(string errorMessage)
        {
            textBlockError.Visibility = Visibility.Visible;
            textBlockError.Text = errorMessage;
        }

        private void clearError()
        {
            textBlockError.Visibility = Visibility.Collapsed;
        }

        private void disablePage()
        {
            textBoxUsername.IsEnabled = false;
            buttonUpdateUsername.IsEnabled = false;
        }

        private void enablePage()
        {
            textBoxUsername.IsEnabled = true;
            buttonUpdateUsername.IsEnabled = true;
        }
    }
}
