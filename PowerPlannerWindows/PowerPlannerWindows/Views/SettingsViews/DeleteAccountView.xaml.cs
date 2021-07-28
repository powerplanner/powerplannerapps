using Microsoft.UI.Xaml;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DeleteAccountView : PopupViewHostGeneric
    {
        public new DeleteAccountViewModel ViewModel
        {
            get { return base.ViewModel as DeleteAccountViewModel; }
            set { base.ViewModel = value; }
        }

        public DeleteAccountView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            checkBoxDeleteOnlineToo.Visibility = ViewModel.Account.IsOnlineAccount ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void buttonConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            await ViewModel.DeleteAsync();
            IsEnabled = true;
        }
    }
}
