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

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            base.Visibility = Visibility.Collapsed;

            try
            {
                this.UpdatePinButton();
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
