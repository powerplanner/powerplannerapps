using PowerPlannerAppDataLibrary.DataLayer;
using System;
using System.Collections.Generic;
using Windows.Data.Xml.Dom;
using Windows.UI;
using Windows.UI.Notifications;
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
        }

        private void UpdatePreviewAndRealTileNotifications()
        {
            try
            {
                UpdateRealTileNotifications();
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
