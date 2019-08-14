using InterfacesUWP;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.WelcomeViews
{
    public sealed partial class GetStartedView : PopupViewHostGeneric
    {
        public new GetStartedViewModel ViewModel
        {
            get => base.ViewModel as GetStartedViewModel;
            set => base.ViewModel = value;
        }

        public GetStartedView()
        {
            this.InitializeComponent();
        }

        private void ButtonLogIn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.LogIn();
        }

        private async void ButtonGetStarted_Click(object sender, RoutedEventArgs e)
        {
            var loadingPopup = new LoadingPopup()
            {
                Text = "Almost there"
            };

            loadingPopup.Show();

            await ViewModel.GetStartedAsync();

            loadingPopup.Close();
        }
    }
}
