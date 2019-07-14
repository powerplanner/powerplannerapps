using PowerPlannerSending;
using PowerPlannerUWP.Views;
using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsUniversal;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UpdateCredentialsView : PopupViewHostGeneric
    {
        public new UpdateCredentialsViewModel ViewModel
        {
            get { return base.ViewModel as UpdateCredentialsViewModel; }
            set { base.ViewModel = value; }
        }

        public UpdateCredentialsView()
        {
            this.InitializeComponent();
        }

        private void buttonLogIn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LogIn();
        }

        private void passwordBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                buttonLogIn_Click(null, null);
            }
        }

        private void textBoxUsername_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                passwordBox.Focus(FocusState.Programmatic);
            }
        }

        private void textBlockForgotUsername_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.ForgotUsername();
        }

        private void textBlockForgotPassword_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.ForgotPassword();
        }
    }
}
