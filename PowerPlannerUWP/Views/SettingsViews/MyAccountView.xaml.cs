using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MyAccountView : ViewHostGeneric
    {
        public new MyAccountViewModel ViewModel
        {
            get { return base.ViewModel as MyAccountViewModel; }
            set { base.ViewModel = value; }
        }

        public MyAccountView()
        {
            this.InitializeComponent();
        }

        private void ButtonLogOut_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LogOut();
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ChangePassword();
        }

        private void ButtonChangeUsername_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ChangeUsername();
        }



        private void ButtonChangeEmail_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ChangeEmail();
        }

        private void ButtonDeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PromptConfirmDelete();
        }

        private void ButtonConvertToOnline_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ConvertToOnline();
        }
    }
}
