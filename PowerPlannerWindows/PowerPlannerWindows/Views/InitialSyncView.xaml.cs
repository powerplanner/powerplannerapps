using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InitialSyncView : ViewHostGeneric
    {
        public new InitialSyncViewModel ViewModel
        {
            get => base.ViewModel as InitialSyncViewModel;
            set => base.ViewModel = value;
        }

        public InitialSyncView()
        {
            this.InitializeComponent();

            TextBlockSyncing.Text = PowerPlannerResources.GetString("LoginPage_String_SyncingAccount");
            TextBlockError.Text = PowerPlannerResources.GetString("String_SyncError");
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenSettings();
        }

        private void ButtonSyncAgain_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TryAgain();
        }
    }
}
