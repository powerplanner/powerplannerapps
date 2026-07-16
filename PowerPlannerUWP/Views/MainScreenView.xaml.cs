using InterfacesUWP;
using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using PowerPlannerUWP.Views.SettingsViews;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ToolsPortable;
using Vx.Uwp;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views
{
    public sealed partial class MainScreenView : ViewHostGeneric
    {
        public new MainScreenViewModel ViewModel
        {
            get { return base.ViewModel as MainScreenViewModel; }
            set { base.ViewModel = value; }
        }

        public MainScreenView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelSetOverride()
        {
            MainContentContainer.Child = ViewModel.Render();
        }

        public override void OnViewModelLoadedOverride()
        {
            bool showedPopup = false;

            if (!showedPopup)
                TryAskingForRatingIfNeeded();
        }

        private async void TryAskingForRatingIfNeeded()
        {
            try
            {
                // If we haven't asked for rating yet
                if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey("HasAskedForRating"))
                {
                    if (ViewModel.CurrentAccount != null)
                    {
                        var dataStore = await AccountDataStore.Get(ViewModel.CurrentLocalAccountId);

                        // If they actually have a decent amount of tasks/events
                        if (await System.Threading.Tasks.Task.Run(async delegate
                        {
                            using (await Locks.LockDataForReadAsync())
                            {
                                return dataStore.TableMegaItems.Count() > 30 && dataStore.TableMegaItems.Any(i => i.DateCreated < DateTime.Today.AddDays(-60));
                            }
                        }))
                        {
                            CustomMessageBox mb = new CustomMessageBox("Thanks for using Power Planner! If you love the app, please leave a rating in the Store! If you have any suggestions or issues, please email me!", "★ Review App ★", "Review", "Email Dev", "Close");
                            mb.Response += mbAskForReview_Response;
                            mb.Show();

                            ApplicationData.Current.RoamingSettings.Values["HasAskedForRating"] = true;
                        }
                    }
                }
            }

            catch { }
        }

        private async void mbAskForReview_Response(object sender, MessageBoxResponse e)
        {
            try
            {
                switch (e.Response)
                {
                    // Review
                    case 0:

                        try
                        {
                            await ReviewAppExtension.Current?.ReviewAppAsync();
                        }
                        catch (Exception ex)
                        {
                            TelemetryExtension.Current?.TrackException(ex);
                        }
                        break;

                    // Email dev
                    case 1:
                        AboutViewModel.EmailDeveloper();
                        break;
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
