using BareMvvm.Core.ViewModels;
using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using Windows.System;
using Microsoft.UI.Xaml;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutView : ViewHostGeneric
    {
        public AboutView()
        {
            this.InitializeComponent();

            tbVersion.Text = Variables.VERSION.ToString();
        }

        private void ButtonEmailDeveloper_Click(object sender, RoutedEventArgs e)
        {
            EmailDeveloper(ViewModel);
        }

        public static async void EmailDeveloper(BaseViewModel current)
        {
            try
            {
                string accountInfo = "";
                var mainScreen = current is MainScreenViewModel ? current as MainScreenViewModel : current.FindAncestor<MainScreenViewModel>();
                if (mainScreen != null && mainScreen.CurrentAccount != null)
                {
                    accountInfo = " - " + mainScreen.CurrentAccount.GetTelemetryUserId() + " - " + mainScreen.CurrentAccount.DeviceId;
                }

                await Launcher.LaunchUriAsync(new Uri("mailto:?to=support@powerplanner.net&subject=Power Planner for Win 10 - Contact Developer - " + Variables.VERSION + accountInfo));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async void ButtonPrivacy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Launcher.LaunchUriAsync(new Uri("https://powerplanner.net/privacy"));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
