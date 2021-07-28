﻿using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Welcome.Login;
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
    public sealed partial class RecoveredUsernamesView : PopupViewHostGeneric
    {
        public new RecoveredUsernamesViewModel ViewModel
        {
            get { return base.ViewModel as RecoveredUsernamesViewModel; }
            set { base.ViewModel = value; }
        }

        public RecoveredUsernamesView()
        {
            this.InitializeComponent();

            ButtonOk.Content = LocalizedResources.Common.GetStringClose();
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            if (ViewModel.Usernames.Length == 0)
            {
                // Shouldn't happen
            }
            else
            {
                TextBlockExplanation.Text = LocalizedResources.GetString("ForgotUsername_String_YourUsernames");

                string answer = "";
                for (int i = 0; i < ViewModel.Usernames.Length; i++)
                    answer += ViewModel.Usernames[i] + "\n";
                TextBlockUsernames.Text = answer;
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveViewModel();
        }
    }
}
