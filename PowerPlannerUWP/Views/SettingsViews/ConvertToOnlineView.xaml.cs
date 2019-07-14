using PowerPlannerSending;
using PowerPlannerUWP.Views;
using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.SyncLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsUniversal;
using Windows.ApplicationModel.Resources;
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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConvertToOnlineView : PopupViewHostGeneric
    {
        public new ConvertToOnlineViewModel ViewModel
        {
            get { return base.ViewModel as ConvertToOnlineViewModel; }
            set { base.ViewModel = value; }
        }

        public ConvertToOnlineView()
        {
            this.InitializeComponent();
        }

        private void buttonConvertToOnline_Click(object sender, RoutedEventArgs e)
        {
            createOnlineAccount();
        }

        private void textBoxEmail_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                createOnlineAccount();
            }
        }
        
        private async void createOnlineAccount()
        {
            base.IsEnabled = false;
            await ViewModel.CreateOnlineAccountAsync();
            base.IsEnabled = true;
        }

        private async void buttonMergeExisting_Click(object sender, RoutedEventArgs e)
        {
            base.IsEnabled = false;
            await ViewModel.MergeExisting();
            base.IsEnabled = true;
        }

        private void buttonCancelMergeExisting_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CancelMergeExisting();
        }
    }
}
