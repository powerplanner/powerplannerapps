using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using PowerPlannerAppDataLibrary.App;
using BareMvvm.Core.ViewModels;
using PowerPlannerUWP.ViewModel.Settings;
using ToolsPortable;
using Windows.UI.Core;
using PowerPlannerAppDataLibrary.Extensions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsListView : ViewHostGeneric
    {
        public new SettingsListViewModel ViewModel
        {
            get { return base.ViewModel as SettingsListViewModel; }
            set { base.ViewModel = value; }
        }

        public SettingsListView()
        {
            this.InitializeComponent();
        }

#if DEBUG
        ~SettingsListView()
        {
            System.Diagnostics.Debug.WriteLine("SettingsListView disposed");
        }
#endif

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            try
            {
                if (!ViewModel.HasAccount)
                {
                    ButtonCalendarIntegration.Visibility = Visibility.Collapsed;
                    ButtonLiveTiles.Visibility = Visibility.Collapsed;
                }

                UpdateUpgradeToPremiumVisibility();

                Window.Current.Activated += new WeakEventHandler<WindowActivatedEventArgs>(Current_Activated).Handler;
            }

            catch (Exception ex)
            {
                base.IsEnabled = false;
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async void UpdateUpgradeToPremiumVisibility()
        {
            try
            {
                if (await PowerPlannerApp.Current.IsFullVersionAsync())
                {
                    ButtonUpgradeToPremium.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ButtonUpgradeToPremium.Visibility = Visibility.Visible;
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            UpdateUpgradeToPremiumVisibility();
        }

        private void ButtonMyAccount_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenMyAccount();
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenAbout();
        }

        private void ButtonUpgradeToPremium_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenPremiumVersion();
        }

        private void ButtonReminders_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenReminderSettings();
        }

        private void ButtonTwoWeekSchedule_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenTwoWeekScheduleSettings();
        }

        private void ButtonSyncOptions_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenSyncOptions();
        }

        private void ButtonLiveTiles_Click(object sender, RoutedEventArgs e)
        {
            var pagedViewModel = ViewModel.FindAncestor<PagedViewModel>();
            pagedViewModel.Navigate(new TileSettingsViewModel(pagedViewModel));
        }

        private void ButtonCalendarIntegration_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenCalendarIntegration();
        }

        private void ButtonGoogleCalendarIntegration_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenGoogleCalendarIntegration();
        }

        private async void ButtonContribute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Launcher.LaunchUriAsync(new Uri(ViewModel.OpenContribute()));
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void ButtonCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenCreateAccount();
        }

        private void ButtonLogIn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenLogIn();
        }

        private async void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TelemetryExtension.Current?.TrackEvent("Action_OpenHelp");

                await Launcher.LaunchUriAsync(new Uri(SettingsListViewModel.HelpUrl));
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
                new PortableMessageDialog("Web browser couldn't be launched.").Show();
            }
        }

        private void ButtonSchoolTimeZone_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.OpenSchoolTimeZone();
        }
    }
}
