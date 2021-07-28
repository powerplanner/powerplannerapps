﻿using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
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

            TextBlockUsername.Text = PowerPlannerResources.GetString("TextBox_Username.Header");
            TextBlockEmail.Text = PowerPlannerResources.GetString("CreateAccountPage_TextBoxEmail.Header");
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Continue();
        }
    }
}
