using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
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
using Windows.Web.Http;
using Windows.Web.Http.Filters;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GoogleCalendarIntegrationView : PopupViewHostGeneric
    {
        public new GoogleCalendarIntegrationViewModel ViewModel
        {
            get { return base.ViewModel as GoogleCalendarIntegrationViewModel; }
            set { base.ViewModel = value; }
        }

        public GoogleCalendarIntegrationView()
        {
            this.InitializeComponent();

            Title = PowerPlannerResources.GetString("Settings_MainPage_GoogleCalendarIntegrationItem/Title").ToUpper();
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
            try
            {
                Uri uri = new Uri(GoogleCalendarIntegrationViewModel.Url);
                var filter = new HttpBaseProtocolFilter();

                foreach (var c in ViewModel.GetCookies())
                {
                    filter.CookieManager.SetCookie(new HttpCookie(c.Key, uri.Host, "/")
                    {
                        Value = c.Value
                    }, false);
                }

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

                // TODO
                //MyWebView.NavigateWithHttpRequestMessage(httpRequestMessage);

                base.OnViewModelLoadedOverride();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
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
