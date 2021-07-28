using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ForgotUsernameView : PopupViewHostGeneric
    {
        public new ForgotUsernameViewModel ViewModel
        {
            get { return base.ViewModel as ForgotUsernameViewModel; }
            set { base.ViewModel = value; }
        }

        public ForgotUsernameView()
        {
            this.InitializeComponent();

            this.Title = LocalizedResources.GetString("ForgotUsername_String_MessageHeader").ToUpper();
            TextBlockExplanation.Text = LocalizedResources.GetString("ForgotUsername_String_EnterEmailAddressExplanation");
            ButtonRecover.Content = LocalizedResources.GetString("String_Recover");
        }

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsRecoveringUsernames):
                    UpdateEnabledStatus();
                    break;
            }
        }

        private void UpdateEnabledStatus()
        {
            base.IsEnabled = !ViewModel.IsRecoveringUsernames;
        }

        private void ButtonRecover_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Recover();
        }

        private void TextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                ViewModel.Recover();
            }
        }
    }
}
