using BareMvvm.Core.App;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Promos;
using PowerPlannerUWP.ViewModel.Promos;
using PowerPlannerUWPLibrary;
using PowerPlannerUWPLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpgradeFromSilverlight;
using UpgradeFromWin8;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace PowerPlannerUWP
{
    public class PowerPlannerUwpApp : PowerPlannerApp
    {
        public PowerPlannerUwpApp()
        {
            // Use popup style for configuring class grades
            PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings.Grades.ConfigureClassGradesViewModel.UsePopups = true;
        }

        public static new PowerPlannerUwpApp Current
        {
            get { return PortableApp.Current as PowerPlannerUwpApp; }
        }

        protected override async Task InitializeAsyncOverride()
        {
            InitializeUWP.Initialize();

            // We register promoting other accounts as one of the first promotions
            PromoRegistrations.Registrations.Insert(0, typeof(PromoOtherPlatformsViewModel.Registration));
            PromoRegistrations.Registrations.Add(typeof(PromoContributeViewModel.Registration));

            // Extensions are registered with InitializeUWP.Initialize, since they're also needed from the background task

            try
            {
                // Only waits if this hasn't run before
                await HandleUpgradeFromWin8();
                await HandleUpgradeFromSilverlight();

                try
                {
                    await BackgroundExecutionManager.RequestAccessAsync();
                }

                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }

            await base.InitializeAsyncOverride();
        }

        private static async System.Threading.Tasks.Task HandleUpgradeFromWin8()
        {
            const string HANDLED_WIN8_DATA = "HandledWin8Data";

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(HANDLED_WIN8_DATA))
            {
                Debug.WriteLine("Upgrading accounts from Win8 version");

                await Upgrader.UpgradeAccounts();

                ApplicationData.Current.LocalSettings.Values[HANDLED_WIN8_DATA] = true;
            }
        }

        private static async System.Threading.Tasks.Task HandleUpgradeFromSilverlight()
        {
            const string HANDLED_SILVERLIGHT_DATA = "HandledSilverlightData";

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(HANDLED_SILVERLIGHT_DATA))
            {
                Debug.WriteLine("Checking for Silverlight data");

                await SilverlightUpgrader.UpgradeAccounts();

                ApplicationData.Current.LocalSettings.Values[HANDLED_SILVERLIGHT_DATA] = true;
            }
        }
    }
}
