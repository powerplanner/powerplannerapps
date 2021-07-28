using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChangeEmailView : PopupViewHostGeneric
    {
        public new ChangeEmailViewModel ViewModel
        {
            get { return base.ViewModel as ChangeEmailViewModel; }
            set { base.ViewModel = value; }
        }

        public ChangeEmailView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateProgress();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsRetrievingEmail):
                case nameof(ViewModel.IsUpdatingEmail):
                    UpdateProgress();
                    break;
            }
        }

        private void UpdateProgress()
        {
            if (ViewModel.IsRetrievingEmail || ViewModel.IsUpdatingEmail)
            {
                disable();
            }
            else
            {
                enable();
            }
        }

        private void textBoxEmail_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                updateEmail();
            }
        }

        private void disable()
        {
            textBoxEmail.IsEnabled = false;
            buttonUpdateEmail.IsEnabled = false;
        }

        private void enable()
        {
            textBoxEmail.IsEnabled = true;
            buttonUpdateEmail.IsEnabled = true;
        }

        private void buttonUpdateEmail_Click(object sender, RoutedEventArgs e)
        {
            updateEmail();
        }

        private void updateEmail()
        {
            ViewModel.UpdateEmail();
        }
    }
}
