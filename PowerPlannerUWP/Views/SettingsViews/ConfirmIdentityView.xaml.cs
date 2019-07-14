using PowerPlannerUWP.Views;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConfirmIdentityView : PopupViewHostGeneric
    {
        public new ConfirmIdentityViewModel ViewModel
        {
            get { return base.ViewModel as ConfirmIdentityViewModel; }
            set { base.ViewModel = value; }
        }

        public ConfirmIdentityView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            ViewModel.ActionIncorrectPassword += ViewModel_ActionIncorrectPassword;
        }

        private void ViewModel_ActionIncorrectPassword(object sender, EventArgs e)
        {
            TextBlockError.Visibility = Visibility.Visible;
        }

        private void Confirm()
        {
            ViewModel.Continue();
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            Confirm();
        }

        private void passwordConfirmCurrentPassword_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                Confirm();
                e.Handled = true;
            }
        }

        private void passwordConfirmCurrentPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            TextBlockError.Visibility = Visibility.Collapsed;
        }
    }
}
