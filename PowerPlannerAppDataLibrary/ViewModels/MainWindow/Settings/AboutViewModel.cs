using BareMvvm.Core.ViewModels;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.SyncLayer;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vx.Views;

namespace PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings
{
    public class AboutViewModel : PopupComponentViewModel
    {
        public AboutViewModel(BaseViewModel parent) : base(parent)
        {
            Title = PowerPlannerResources.GetString("Settings_AboutPage_Header.Text");
        }

        private VxState<string> _developerLogs = new VxState<string>();

        protected override View Render()
        {
            return RenderGenericPopupContent(
                RenderHeader(PowerPlannerResources.GetString("Settings_AboutPage_VersionHeader.Text"), false),
                RenderDescription(Variables.VERSION.ToString()),

                RenderHeader(PowerPlannerResources.GetString("Settings_AboutPage_DeveloperHeader.Text")),
                RenderDescription(PowerPlannerResources.GetString("Settings_AboutPage_DeveloperValue.Text")),

                RenderHeader(PowerPlannerResources.GetString("Settings_AboutPage_AboutAppHeader.Text")),
                RenderDescription(PowerPlannerResources.GetString("Settings_AboutPage_AboutAppValue.Text")),

                RenderHeader(PowerPlannerResources.GetString("Settings_AboutPage_PrivacyHeader.Text")),
                new Button
                {
                    Text = "https://powerplanner.net/privacy",
                    Click = OpenPrivacy,
                    Margin = new Thickness(0, 6, 0, 0)
                },

                RenderHeader(PowerPlannerResources.GetString("Settings_AboutPage_ContactHeader.Text")),
                new Button
                {
                    Text = "support@powerplanner.net",
                    Click = EmailDeveloper,
                    Margin = new Thickness(0, 6, 0, 0)
                },

                RenderHeader("developer logs"),
                new Button
                {
                    Text = "Show logs",
                    Click = ShowLogs,
                    Margin = new Thickness(0, 6, 0, 0)
                },

                _developerLogs.Value != null ? new MultilineTextBox
                {
                    Text = new VxValue<string>(_developerLogs.Value, e => { }),
                    Height = 150,
                    Margin = new Thickness(0, 12, 0, 0)
                } : null
            );
        }

        private void ShowLogs()
        {
            _developerLogs.Value = TelemetryExtension.Current?.GetDeveloperLogs();
        }

        private void OpenPrivacy()
        {
            _ = BrowserExtension.Current?.OpenUrlAsync(new Uri("https://powerplanner.net/privacy"));
        }

        public static void EmailDeveloper()
        {
            try
            {
                string accountInfo = "";
                var account = PowerPlannerApp.Current.GetCurrentAccount();
                if (account != null)
                {
                    accountInfo = " - " + account.GetTelemetryUserId() + " - " + account.DeviceId;
                }

                string subject = $"{SyncExtensions.GetAppName()} - Contact Developer - " + Variables.VERSION + accountInfo;

                _ = EmailExtension.Current.ComposeNewMailAsync("support@powerplanner.net", subject);
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private TextBlock RenderHeader(string text, bool includeTopMargin = true)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = Theme.Current.TitleFontSize,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, includeTopMargin ? 12 : 0, 0, 0)
            };
        }

        private TextBlock RenderDescription(string text)
        {
            return new TextBlock
            {
                Text = text
            };
        }
    }
}
