using InterfacesUWP;
using NotificationsVisualizerLibrary;
using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerUWPLibrary.TileHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PowerPlannerUWP.ViewModel.Settings;
using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.Extensions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainTileView : ViewHostGeneric
    {
        public new MainTileViewModel ViewModel
        {
            get { return base.ViewModel as MainTileViewModel; }
            set { base.ViewModel = value; }
        }

        public MainTileView()
        {
            this.InitializeComponent();

            this.InitializeTiles();
        }

        private void InitializeTiles()
        {
            try
            {
                if (LargePreviewTile.DeviceFamily != NotificationsVisualizerLibrary.DeviceFamily.Mobile)
                    LargePreviewTile.Visibility = Visibility.Visible;

                foreach (var tile in AllTiles())
                    InitializeTile(tile);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private IEnumerable<PreviewTile> AllTiles()
        {
            return new PreviewTile[]
            {
                MediumPreviewTile,
                WidePreviewTile,
                LargePreviewTile
            };
        }

        private async void InitializeTile(PreviewTile tile)
        {
            try
            {
                tile.DisplayName = "Power Planner";
                tile.VisualElements.BackgroundColor = (Color)Resources["SystemColorHighlightColor"];
                tile.VisualElements.ShowNameOnSquare150x150Logo = true;
                tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.png");
                tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/Square71x71Logo.png");
                tile.VisualElements.Square150x150Logo = new Uri("ms-appx:///Assets/Square150x150Logo.png");
                tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/Square310x310Logo.png");
                tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.png");

                await tile.UpdateAsync();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void UpdatePreviewAndRealTileNotifications()
        {
            try
            {
                UpdateRealTileNotifications();
                UpdatePreviewTileNotifications();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async void UpdateRealTileNotifications()
        {
            try
            {
                // Class tiles are also affected if they don't have their own settings
                await TileHelper.UpdateTileNotificationsForAccountAsync(ViewModel.Account, await AccountDataStore.Get(ViewModel.Account.LocalAccountId));
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async void UpdatePreviewTileNotifications()
        {
            try
            {
                XmlDocument notifContent = await TileHelper.GetCurrentPrimaryTileNotificationContentAsync(ViewModel.Account);

                if (notifContent == null)
                {
                    foreach (var tile in AllTiles())
                        tile.CreateTileUpdater().Clear();
                }

                else
                {
                    foreach (var tile in AllTiles())
                        tile.CreateTileUpdater().Update(new TileNotification(notifContent));
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            try
            {
                UpdatePreviewTileNotifications();
            }

            catch (Exception ex)
            {
                base.IsEnabled = false;
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async void ToggleTasks_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.Account == null || ViewModel.Account.MainTileSettings.ShowTasks == ToggleTasks.IsOn)
                    return;

                ViewModel.Account.MainTileSettings.ShowTasks = ToggleTasks.IsOn;

                await AccountsManager.Save(ViewModel.Account);

                UpdatePreviewAndRealTileNotifications();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async void ToggleEvents_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.Account == null || ViewModel.Account.MainTileSettings.ShowEvents == ToggleEvents.IsOn)
                    return;

                ViewModel.Account.MainTileSettings.ShowEvents = ToggleEvents.IsOn;

                await AccountsManager.Save(ViewModel.Account);

                UpdatePreviewAndRealTileNotifications();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async void TextBoxSkipAssignments_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int skipNumber = 0;
                TextBlockSkipItemsError.Visibility = Visibility.Collapsed;

                if (string.IsNullOrWhiteSpace(TextBoxSkipAssignments.Text))
                {
                    skipNumber = int.MinValue;
                }

                else if (int.TryParse(TextBoxSkipAssignments.Text, out skipNumber))
                {
                    // nothing
                }

                // Invalid entry
                else
                {
                    TextBlockSkipItemsError.Visibility = Visibility.Visible;
                    return;
                }


                if (ViewModel.Account == null || ViewModel.Account.MainTileSettings.SkipItemsOlderThan == skipNumber)
                    return;

                ViewModel.Account.MainTileSettings.SkipItemsOlderThan = skipNumber;

                await AccountsManager.Save(ViewModel.Account);

                UpdatePreviewAndRealTileNotifications();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
