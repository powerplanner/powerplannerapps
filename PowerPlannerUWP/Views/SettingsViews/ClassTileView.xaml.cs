using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ToolsUniversal;
using InterfacesUWP.Views;
using PowerPlannerUWP.ViewModel.Settings;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerUWP.TileHelpers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClassTileView : ViewHostGeneric
    {
        public new ClassTileViewModel ViewModel
        {
            get { return base.ViewModel as ClassTileViewModel; }
            set { base.ViewModel = value; }
        }

        public ClassTileView()
        {
            this.InitializeComponent();

            base.Visibility = Visibility.Collapsed;
        }

        private void UpdatePreviewAndRealTiles()
        {
            try
            {
                UpdateRealTile();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private async void UpdateRealTile()
        {
            try
            {
                await ClassTileHelper.UpdateTileAsync(ViewModel.Account, await AccountDataStore.Get(ViewModel.Account.LocalAccountId), ViewModel.Class.Identifier);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public override void OnViewModelLoadedOverride()
        {
            try
            {
                base.OnViewModelLoadedOverride();

                TextBlockTitle.Text = LocalizedResources.GetString("Settings_Tiles_ClassTile_HeaderText") + " - " + ViewModel.Class.Name;

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

        private async void ToggleTasks_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.Settings.ShowTasks == ToggleTasks.IsOn)
                    return;

                ViewModel.Settings.ShowTasks = ToggleTasks.IsOn;

                await ViewModel.Account.SaveClassTileSettings(ViewModel.Class.Identifier, ViewModel.Settings);

                UpdatePreviewAndRealTiles();
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
                if (ViewModel.Settings.ShowEvents == ToggleEvents.IsOn)
                    return;

                ViewModel.Settings.ShowEvents = ToggleEvents.IsOn;

                await ViewModel.Account.SaveClassTileSettings(ViewModel.Class.Identifier, ViewModel.Settings);

                UpdatePreviewAndRealTiles();
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


                if (ViewModel.Settings.SkipItemsOlderThan == skipNumber)
                    return;

                ViewModel.Settings.SkipItemsOlderThan = skipNumber;

                await ViewModel.Account.SaveClassTileSettings(ViewModel.Class.Identifier, ViewModel.Settings);

                UpdatePreviewAndRealTiles();
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
                if (!await PowerPlannerApp.Current.IsFullVersionAsync())
                {
                    PowerPlannerApp.Current.PromptPurchase(LocalizedResources.GetString("Settings_Tiles_ClassTile_PromptPremiumFeature"));
                    return;
                }

                if (ViewModel.Account == null || ViewModel.Class == null)
                    return;

                if ((appBarPin.Icon as SymbolIcon).Symbol == Symbol.Pin)
                {
                    var data = await AccountDataStore.Get(ViewModel.Account.LocalAccountId);

                    await ClassTileHelper.PinTileAsync(ViewModel.Account, data, ViewModel.Class.Identifier, ViewModel.Class.Name, ColorTools.GetColor(ViewModel.Class.Color));
                }

                else
                {
                    await ClassTileHelper.UnpinTile(ViewModel.Account.LocalAccountId, ViewModel.Class.Identifier);
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
                if (ViewModel.Account == null || ViewModel.Class == null)
                    return;

                if (ClassTileHelper.IsPinned(ViewModel.Account.LocalAccountId, ViewModel.Class.Identifier))
                {
                    appBarPin.Icon = new SymbolIcon(Symbol.UnPin);
                    appBarPin.Label = LocalizedResources.GetString("Settings_Tiles_ClassTile_ButtonUnpinLabel");
                }

                else
                {
                    appBarPin.Icon = new SymbolIcon(Symbol.Pin);
                    appBarPin.Label = LocalizedResources.GetString("Settings_Tiles_ClassTile_ButtonPinLabel");
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
