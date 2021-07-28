using BareMvvm.Core.Snackbar;
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

namespace InterfacesUWP.Snackbar
{
    public sealed partial class BareSnackbarPresenter : UserControl
    {
        public BareSnackbarManager Manager { get; private set; } = new BareSnackbarManager();

        public BareSnackbarPresenter()
        {
            this.InitializeComponent();

            DataContext = Manager;
        }

        private void ButtonAction_Click(object sender, RoutedEventArgs e)
        {
            BareSnackbar snackbar = (sender as FrameworkElement).DataContext as BareSnackbar;
            Manager.Close(snackbar);
            snackbar.ButtonCallback();
        }

        private void Snackbar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            BareSnackbar snackbar = (sender as FrameworkElement).DataContext as BareSnackbar;
            Manager.Close(snackbar);
        }
    }
}
