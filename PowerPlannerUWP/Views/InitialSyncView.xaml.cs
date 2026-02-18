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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
