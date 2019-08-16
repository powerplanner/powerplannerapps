using InterfacesUWP.Views;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views
{
    public sealed partial class WelcomeView : ViewHostGeneric
    {
        public new WelcomeViewModel ViewModel
        {
            get { return base.ViewModel as WelcomeViewModel; }
            set { base.ViewModel = value; }
        }

        public WelcomeView()
        {
            this.InitializeComponent();
        }

        private void thisPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width <= 450)
                VisualStateManager.GoToState(this, "CompactState", true);
            else
                VisualStateManager.GoToState(this, "NormalState", true);
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Login();
        }

        private void ButtonCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CreateAccount();
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenSettings();
        }
    }
}
