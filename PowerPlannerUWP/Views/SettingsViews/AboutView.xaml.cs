using BareMvvm.Core.ViewModels;
using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerUWPLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

        private void buttonEmailDeveloper_Click(object sender, RoutedEventArgs e)
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

                await Launcher.LaunchUriAsync(new Uri("mailto:?to=barebonesdev@live.com&subject=Power Planner for Win 10 - Contact Developer - " + Variables.VERSION + accountInfo));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
