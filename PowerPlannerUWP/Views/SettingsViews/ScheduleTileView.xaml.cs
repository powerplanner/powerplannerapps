using NotificationsVisualizerLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using Windows.Data.Xml.Dom;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using InterfacesUWP.Views;
using PowerPlannerUWP.ViewModel.Settings;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerUWP.TileHelpers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScheduleTileView : ViewHostGeneric
    {
        public new ScheduleTileViewModel ViewModel
        {
            get { return base.ViewModel as ScheduleTileViewModel; }
            set { base.ViewModel = value; }
        }

        public ScheduleTileView()
        {
            this.InitializeComponent();
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
                SmallPreviewTile,
                MediumPreviewTile,
                WidePreviewTile,
                LargePreviewTile
            };
        }

        private async void InitializeTile(PreviewTile tile)
        {
            try
            {
                tile.DisplayName = "Schedule";
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

        private async void UpdatePreviewTileNotifications()
        {
            try
            {
                XmlDocument notifContent = await ScheduleTileHelper.GetCurrentTileNotificationContentAsync(ViewModel.Account);

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

            base.Visibility = Visibility.Collapsed;

            try
            {
                this.UpdatePinButton();
                this.InitializeTiles();

                UpdatePreviewTileNotifications();
            }

            catch (Exception ex)
            {
                base.IsEnabled = false;
                TelemetryExtension.Current?.TrackException(ex);
            }

            finally
            {
                base.Visibility = Visibility.Visible;
            }
        }

        private async void appBarPin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((appBarPin.Icon as SymbolIcon).Symbol == Symbol.Pin)
                {
                    var data = await AccountDataStore.Get(ViewModel.Account.LocalAccountId);

                    await ScheduleTileHelper.PinTile(ViewModel.Account, data);
                }

                else
                {
                    await ScheduleTileHelper.UnpinTile(ViewModel.Account.LocalAccountId);
                }

                UpdatePinButton();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void UpdatePinButton()
        {
            try
            {
                if (ScheduleTileHelper.IsPinned(ViewModel.Account.LocalAccountId))
                {
                    appBarPin.Icon = new SymbolIcon(Symbol.UnPin);
                    appBarPin.Label = LocalizedResources.GetString("Tile_UnpinTile");
                }

                else
                {
                    appBarPin.Icon = new SymbolIcon(Symbol.Pin);
                    appBarPin.Label = LocalizedResources.GetString("Tile_PinTile");
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
