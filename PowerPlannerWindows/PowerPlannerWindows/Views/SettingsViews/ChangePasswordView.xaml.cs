using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChangePasswordView : PopupViewHostGeneric
    {
        public new ChangePasswordViewModel ViewModel
        {
            get { return base.ViewModel as ChangePasswordViewModel; }
            set { base.ViewModel = value; }
        }

        public ChangePasswordView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            ViewModel.ActionError += ViewModel_ActionError;
            ViewModel.ActionPasswordsDidNotMatch += ViewModel_ActionPasswordsDidNotMatch;

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            UpdateProgress();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsUpdatingPassword):
                    UpdateProgress();
                    break;
            }
        }

        private void ViewModel_ActionPasswordsDidNotMatch(object sender, string e)
        {
            setError(e);
        }

        private void ViewModel_ActionError(object sender, string e)
        {
            setError(e);
        }

        private void buttonUpdatePassword_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Update();
        }

        private void UpdateProgress()
        {
            if (ViewModel.IsUpdatingPassword)
            {
                disablePage();
                clearError();
            }
            else
            {
                enablePage();
            }
        }

        private void setError(string message)
        {
            textBlockError.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            textBlockError.Text = message;
        }

        private void clearError()
        {
            textBlockError.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        private void enablePage()
        {
            password.IsEnabled = true;
            passwordConfirm.IsEnabled = true;
            buttonUpdatePassword.IsEnabled = true;
        }

        private void disablePage()
        {
            passwordConfirm.IsEnabled = false;
            password.IsEnabled = false;
            buttonUpdatePassword.IsEnabled = false;
        }

        private void password_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                passwordConfirm.Focus(FocusState.Programmatic);
            }
        }

        private void passwordConfirm_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                buttonUpdatePassword_Click(null, null);
            }
        }
    }
}
