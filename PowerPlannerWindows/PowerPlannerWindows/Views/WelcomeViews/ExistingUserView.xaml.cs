using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.WelcomeViews
{
    public sealed partial class ExistingUserView : PopupViewHostGeneric
    {
        public new ExistingUserViewModel ViewModel
        {
            get => base.ViewModel as ExistingUserViewModel;
            set => base.ViewModel = value;
        }

        public ExistingUserView()
        {
            this.InitializeComponent();
        }

        private void ButtonNoAccount_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NoAccount();
        }

        private void ButtonHasAccount_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.HasAccount();
        }
    }
}
