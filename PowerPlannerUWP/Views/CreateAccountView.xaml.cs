using InterfacesUWP;
using PowerPlannerSending;
using PowerPlannerUWP.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.CreateAccount;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateAccountView : PopupViewHostGeneric
    {
        public new CreateAccountViewModel ViewModel
        {
            get { return base.ViewModel as CreateAccountViewModel; }
            set { base.ViewModel = value; }
        }

        public CreateAccountView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateStatus();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsCreatingOnlineAccount):
                    UpdateStatus();
                    break;
            }
        }

        private LoadingPopup _loadingPopupCreatingOnlineAccount;
        public void UpdateStatus()
        {
            if (ViewModel.IsCreatingOnlineAccount)
            {
                if (_loadingPopupCreatingOnlineAccount == null)
                {
                    _loadingPopupCreatingOnlineAccount = new LoadingPopup()
                    {
                        Text = "Creating account",
                        IsCancelable = false
                    };
                }
                _loadingPopupCreatingOnlineAccount.Show();
            }
            else
            {
                if (_loadingPopupCreatingOnlineAccount != null)
                {
                    _loadingPopupCreatingOnlineAccount.Close();
                }
            }
        }

        private void createUsername_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                e.Handled = true;
                createEmail.Focus(FocusState.Programmatic);
            }
        }

        private void createEmail_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                e.Handled = true;
                createPassword.Focus(FocusState.Programmatic);
            }
        }

        private void createPassword_EnterPressed(object sender, EventArgs e)
        {
            createPasswordConfirm.Focus(FocusState.Programmatic);
        }

        private void createPasswordConfirm_EnterPressed(object sender, EventArgs e)
        {
            ViewModel.CreateAccount();
        }

        private void createOnlineAccount_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CreateAccount();
        }

        private async void createOfflineAccount_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog(LocalizedResources.GetString("CreateAccountPage_String_WarningOfflineAccountExplanation"), LocalizedResources.GetString("CreateAccountPage_String_WarningOfflineAccount"));

            UICommand createCommand = new UICommand(LocalizedResources.GetString("String_Create"));
            dialog.Commands.Add(new UICommand(LocalizedResources.GetString("String_GoBack")));
            dialog.Commands.Add(createCommand);

            var response = await dialog.ShowAsync();

            if (response == createCommand)
            {
                ViewModel.CreateLocalAccount();
            }
        }

        private void createUsername_Loaded(object sender, RoutedEventArgs e)
        {
            createUsername.Focus(FocusState.Programmatic);
        }

        private void createUsername_EnterPressed(object sender, EventArgs e)
        {
            createEmail.Focus(FocusState.Programmatic);
        }
    }
}
