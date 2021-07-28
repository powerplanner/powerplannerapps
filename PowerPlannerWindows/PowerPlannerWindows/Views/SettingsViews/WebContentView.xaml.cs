﻿using PowerPlannerAppDataLibrary.Extensions;
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
    public sealed partial class WebContentView : PopupViewHostGeneric
    {
        public new BaseWebContentViewModel ViewModel
        {
            get { return base.ViewModel as BaseWebContentViewModel; }
            set { base.ViewModel = value; }
        }

        public WebContentView()
        {
            this.InitializeComponent();
        }

        protected override void UpdateMaxWindowSizeForNonFullScreen()
        {
            base.VerticalAlignment = VerticalAlignment.Stretch;
            base.MaxWidth = MaxWindowSize.Width;
            base.MaxHeight = double.MaxValue;
            base.Margin = new Thickness(24);
        }

        public override void OnViewModelLoadedOverride()
        {
            Title = ViewModel.Title;

            try
            {
                MyWebView.Source = new Uri(ViewModel.WebsiteToDisplay);

                base.OnViewModelLoadedOverride();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);

                LoadingContainer.Visibility = Visibility.Collapsed;
                ErrorContainer.Visibility = Visibility.Visible;
                if (ViewModel.FallbackText != null)
                {
                    TextBlockError.Text = ViewModel.FallbackText;
                }
                else
                {
                    TextBlockError.Text = "Failed to load";
                }
            }
        }

        private void MyWebView_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            LoadingContainer.Visibility = Visibility.Visible;
            ErrorContainer.Visibility = Visibility.Collapsed;
        }

        private void MyWebView_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            LoadingContainer.Visibility = Visibility.Collapsed;
            ErrorContainer.Visibility = Visibility.Collapsed;
        }

        //private void MyWebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        //{
        //    LoadingContainer.Visibility = Visibility.Collapsed;
        //    ErrorContainer.Visibility = Visibility.Visible;

        //    if (ViewModel.FallbackText != null)
        //    {
        //        TextBlockError.Text = ViewModel.FallbackText;
        //        return;
        //    }

        //    if (e.WebErrorStatus == Windows.Web.WebErrorStatus.NotFound)
        //    {
        //        TextBlockError.Text = "Looks like you're offline. Check your internet connection or try again later.";
        //    }
        //    else
        //    {
        //        TextBlockError.Text = "Error: " + e.WebErrorStatus;
        //    }
        //}
    }
}
