using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
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

namespace PowerPlannerUWP.Views.SettingsViews
{
    public sealed partial class SuccessfullyCreatedAccountView : PopupViewHostGeneric
    {
        public new SuccessfullyCreatedAccountViewModel ViewModel
        {
            get => base.ViewModel as SuccessfullyCreatedAccountViewModel;
            set => base.ViewModel = value;
        }

        public SuccessfullyCreatedAccountView()
        {
            this.InitializeComponent();
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Continue();
        }
    }
}
