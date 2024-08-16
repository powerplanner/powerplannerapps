using BareMvvm.Core.App;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Promos;
using PowerPlannerUWP.ViewModel.Promos;
using PowerPlannerUWP.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using BareMvvm.Core.ViewModels;
using PowerPlannerUWP.ViewModel.Settings;

namespace PowerPlannerUWP
{
    public class PowerPlannerUwpApp : PowerPlannerApp
    {
        public static new PowerPlannerUwpApp Current
        {
            get { return PortableApp.Current as PowerPlannerUwpApp; }
        }

        protected override async Task InitializeAsyncOverride()
        {
            InitializeUWP.Initialize();

            // We register promoting other accounts as one of the first promotions
            PromoRegistrations.Registrations.Insert(0, typeof(PromoOtherPlatformsViewModel.Registration));

            // Register settings extensions
            if (TileHelpers.TileHelper.AreLiveTilesSupported)
            {
                SettingsListViewModel.OpenLiveTiles = settingsListViewModel =>
                {
                    var pagedViewModel = settingsListViewModel.FindAncestor<PagedViewModel>();
                    pagedViewModel.Navigate(new TileSettingsViewModel(pagedViewModel));
                };
            }

            // Extensions are registered with InitializeUWP.Initialize, since they're also needed from the background task

            try
            {
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
    }
}
