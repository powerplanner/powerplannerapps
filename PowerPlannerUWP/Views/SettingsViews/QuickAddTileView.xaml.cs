using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PowerPlannerUWP.ViewModel.Settings;
using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerUWP.TileHelpers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QuickAddTileView : ViewHostGeneric
    {
        public new QuickAddTileViewModel ViewModel
        {
            get { return base.ViewModel as QuickAddTileViewModel; }
            set { base.ViewModel = value; }
        }

        public QuickAddTileView()
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

        private void UpdatePinButton()
        {
            try
            {
                if (QuickAddTileHelper.IsPinned(ViewModel.Account.LocalAccountId))
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

        private async void appBarPin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((appBarPin.Icon as SymbolIcon).Symbol == Symbol.Pin)
                {
                    await QuickAddTileHelper.PinTileAsync(ViewModel.Account.LocalAccountId);
                }

                else
                {
                    await QuickAddTileHelper.UnpinTile(ViewModel.Account.LocalAccountId);
                }

                UpdatePinButton();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
