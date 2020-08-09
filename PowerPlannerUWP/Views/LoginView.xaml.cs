using InterfacesUWP;
using System;
using ToolsPortable;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
using System.ComponentModel;
using PowerPlannerAppDataLibrary.Extensions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginView : PopupViewHostGeneric
    {
        public LoginView()
        {
            this.InitializeComponent();
        }
        
        public new LoginViewModel ViewModel
        {
            get { return base.ViewModel as LoginViewModel; }
            set { base.ViewModel = value; }
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            ViewModel.AlertUserUpgradeAccountNeeded = AlertUserUpgradeAccountNeeded;

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsCheckingOnlinePassword":
                    UpdateIsCheckingOnlinePassword();
                    break;

                case "IsLoggingInOnline":
                    UpdateIsLoggingInOnline();
                    break;
            }
        }

        private LoadingPopup _loadingCheckingOnlinePassword;
        private void UpdateIsCheckingOnlinePassword()
        {
            if (ViewModel.IsCheckingOnlinePassword)
            {
                if (_loadingCheckingOnlinePassword == null)
                {
                    _loadingCheckingOnlinePassword = new LoadingPopup()
                    {
                        Text = LocalizedResources.GetString("LoginPage_String_CheckingOnlinePassword")
                    };
                }

                _loadingCheckingOnlinePassword.Show();
            }

            else
            {
                _loadingCheckingOnlinePassword?.Close();
            }
        }

        private LoadingPopup _loadingPopupIsLoggingInOnline;
        private void UpdateIsLoggingInOnline()
        {
            if (ViewModel.IsLoggingInOnline)
            {
                if (_loadingPopupIsLoggingInOnline == null)
                {
                    _loadingPopupIsLoggingInOnline = new LoadingPopup()
                    {
                        Text = LocalizedResources.GetString("LoginPage_String_LoggingIn")
                    };
                }

                _loadingPopupIsLoggingInOnline.Show();
            }

            else
            {
                _loadingPopupIsLoggingInOnline?.Close();
            }
        }

        private void AlertInvalidUsername()
        {
            ShowMessage(string.Format(LocalizedResources.GetString("String_InvalidUsernameExplanation"), string.Join(" ", StringTools.VALID_SPECIAL_URL_CHARS)), LocalizedResources.GetString("String_InvalidUsername"));
        }

        private void AlertUsernameEmpty()
        {
            ShowMessage(LocalizedResources.GetString("LoginPage_String_UsernameEmptyExplanation"), LocalizedResources.GetString("LoginPage_String_UsernameEmpty"));
        }

        private void AlertUsernameExistsLocally()
        {
            ShowMessage(LocalizedResources.GetString("LoginPage_String_ExplanationUsernameExistsLocally"), LocalizedResources.GetString("LoginPage_String_UsernameExists"));
        }

        private void AlertUserUpgradeAccountNeeded(string error)
        {
            CustomMessageBox mb = new CustomMessageBox(error, "Upgrade account", "cancel", "upgrade");
            mb.Response += (s, r) =>
            {
                if (r.Response == 1)
                {
                    var dontWait = Launcher.LaunchUriAsync(new Uri("http://powerplanner.net/web/upgrade"));
                }
            };
            mb.Show();
        }

        private async void tbPassword_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == VirtualKey.Enter)
                {
                    e.Handled = true;
                    await ViewModel.LoginAsync();
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async void tbUsername_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                e.Handled = true;

                // If password has already been auto filled, might as well directly log in
                if (ViewModel.CanLogin())
                    await ViewModel.LoginAsync();

                // Otherwise switch to password box so user can type password
                else
                    tbPassword.Focus(FocusState.Programmatic);
            }
        }

        private async void buttonLogin_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoginAsync();
        }
        
        private static async void ShowMessage(string message, string title)
        {
            await new MessageDialog(message, title).ShowAsync();
        }
        
        private void textBlockForgotUsername_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.ForgotUsername();
        }

        private void textBlockForgotPassword_Tapped(object sender, TappedRoutedEventArgs e)
        {
            showForgotPassword();
        }

        private void showForgotPassword()
        {
            ViewModel.ForgotPassword();
        }

        private void thisPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width <= 450)
                VisualStateManager.GoToState(this, "CompactState", true);
            else
                VisualStateManager.GoToState(this, "NormalState", true);
        }

        private void tbUsername_Loaded(object sender, RoutedEventArgs e)
        {
            tbUsername.Focus(FocusState.Programmatic);
        }
    }
}
