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
using ToolsPortable;
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
            ViewModel.BackRequested += new WeakEventHandler<CancelEventArgs>(ViewModel_BackRequested).Handler;

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            UpdateSelectedItemDisplay();
            UpdateSyncStates();
        }

        public override void OnViewModelLoadedOverride()
        {
            bool showedPopup = false;
            //try
            //{
            //    // If we haven't promoted website yet
            //    if (!(ApplicationData.Current.LocalSettings.Values.ContainsKey("HasPromotedWebsiteBeta") || ApplicationData.Current.RoamingSettings.Values.ContainsKey("HasPromotedWebsiteBeta")))
            //    {
            //        ApplicationData.Current.RoamingSettings.Values["HasPromotedWebsiteBeta"] = true;

            //        var allAccounts = await AccountsManager.GetAllAccounts();

            //        // If they have an online account, show the promo page
            //        if (allAccounts.Any(i => i.IsOnlineAccount))
            //        {
            //            App.ShowPopup(typeof(Pages.PromoPages.PromoWebsitePage), null, null);
            //            showedPopup = true;
            //        }

            //        // Otherwise, if they don't have any accounts (or just downloaded app), we won't ever inform them of the website.
            //    }
            //}

            //catch { }

            if (!showedPopup)
                TryAskingForRatingIfNeeded();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedItem):
                case nameof(ViewModel.SelectedClass):
                    UpdateSelectedItemDisplay();
                    break;

                case nameof(ViewModel.SyncState):
                    UpdateSyncStates();
                    break;

                case nameof(ViewModel.Content):
                    OnContentChanged();
                    break;
            }
        }

        private void OnContentChanged()
        {
            if (ViewModel.Content is SettingsViewModel)
            {
                HideCommandBar();
                SetCommandBarCommands(null, null);
            }
        }

        private void UpdateSyncStates()
        {
            switch (ViewModel.SyncState)
            {
                case MainScreenViewModel.SyncStates.Done:
                    IsIntermediate = false;
                    setProgressBarUploadImagesVisibility(false);
                    break;

                case MainScreenViewModel.SyncStates.Syncing:
                    IsIntermediate = true;
                    setProgressBarUploadImagesVisibility(false);
                    break;

                case MainScreenViewModel.SyncStates.UploadingImages:
                    IsIntermediate = false;
                    setProgressBarUploadImagesVisibility(true);
                    break;
            }
        }

        private bool _isIndeterminte;
        public bool IsIntermediate
        {
            get
            {
                return _isIndeterminte;
            }

            set
            {
                if (_isIndeterminte == value)
                    return;

                _isIndeterminte = value;
                setProgressBarVisibility(value);
            }
        }

        private Storyboard _storyboardProgressBar;
        private bool _isProgressBarVisible;
        private void setProgressBarVisibility(bool visible)
        {
            if (_isProgressBarVisible == visible)
                return;

            if (visible)
            {
                //if the uploading images is visible, we need to hide that
                if (_isProgressBarUploadImagesVisible)
                    setProgressBarUploadImagesVisibility(false);
            }

            else
            {
                //if uploading images has progress to be shown, we need to re-display that
                if (progressBarUploadImages.Value != 0 && progressBarUploadImages.Value != 1)
                    setProgressBarUploadImagesVisibility(true);
            }



            _isProgressBarVisible = visible;

            if (_storyboardProgressBar != null)
                _storyboardProgressBar.Pause();

            if (visible)
                progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;

            DoubleAnimation a = new DoubleAnimation()
            {
                From = progressBar.Opacity,
                To = visible ? 1 : 0,
                Duration = TimeSpan.FromMilliseconds(500)
            };

            Storyboard.SetTarget(a, progressBar);
            Storyboard.SetTargetProperty(a, "Opacity");

            _storyboardProgressBar = new Storyboard();
            _storyboardProgressBar.Children.Add(a);


            if (!visible)
                _storyboardProgressBar.Completed += delegate { progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed; };

            _storyboardProgressBar.Begin();
        }

        private Storyboard _storyboardProgressBarUploadImages;
        private bool _isProgressBarUploadImagesVisible;
        private void setProgressBarUploadImagesVisibility(bool visible)
        {
            if (_isProgressBarUploadImagesVisible == visible)
                return;

            _isProgressBarUploadImagesVisible = visible;

            if (_storyboardProgressBarUploadImages != null)
                _storyboardProgressBarUploadImages.Pause();

            if (visible)
                progressBarUploadImages.Visibility = Windows.UI.Xaml.Visibility.Visible;

            DoubleAnimation a = new DoubleAnimation()
            {
                From = progressBarUploadImages.Opacity,
                To = visible ? 1 : 0,
                Duration = TimeSpan.FromMilliseconds(800)
            };

            Storyboard.SetTarget(a, progressBarUploadImages);
            Storyboard.SetTargetProperty(a, "Opacity");

            _storyboardProgressBarUploadImages = new Storyboard();
            _storyboardProgressBarUploadImages.Children.Add(a);

            if (!visible)
                _storyboardProgressBarUploadImages.Completed += delegate { progressBarUploadImages.Visibility = Windows.UI.Xaml.Visibility.Collapsed; };

            _storyboardProgressBarUploadImages.Begin();
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
                                return dataStore.TableMegaItems.Count() > 15;
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
                            if (ApiInformation.IsMethodPresent("Windows.Services.Store.StoreRequestHelper", "SendRequestAsync"))
                            {
                                var result = await Windows.Services.Store.StoreRequestHelper.SendRequestAsync(
                                    Windows.Services.Store.StoreContext.GetDefault(), 16, String.Empty);

                                // If showing dialog succeeded
                                if (result.ExtendedError == null)
                                {
                                    // We don't want exceptions parsing here to cause fallback behavior
                                    try
                                    {
                                        var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(result.Response);
                                        string status = jsonObject.SelectToken("status")?.ToString() ?? "";

                                        var props = new Dictionary<string, string>()
                                        {
                                            { "Status", status }
                                        };
                                        if (status == "success")
                                        {
                                            bool updated = jsonObject.SelectToken("data")?.Value<bool>("updated") ?? false;
                                            props.Add("Updated", updated.ToString());
                                        }

                                        TelemetryExtension.Current?.TrackEvent("ReviewAppResponse", props);
                                    }
                                    catch (Exception ex)
                                    {
                                        TelemetryExtension.Current?.TrackException(ex);
                                    }

                                    // We don't continue falling back at all
                                    return;
                                }

                                TelemetryExtension.Current?.TrackException(result.ExtendedError);
                            }
                        }
                        catch (Exception ex)
                        {
                            TelemetryExtension.Current?.TrackException(ex);
                            // Fall back to normal
                        }

                        await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9wzdncrfj25v"));
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

        private void ViewModel_BackRequested(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsMenuOpen())
            {
                HideMenu();
                e.Cancel = true;
            }
        }

        public CommandBar GetContentCommandBar()
        {
            return CommandBarForContent;
        }

        private void logo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.SyncCurrentAccount();
        }

        private void syncErrorIndicator_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ViewSyncErrors();
        }

        private void UpdateSelectedItemDisplay()
        {
            if (ViewModel.SelectedItem == NavigationManager.MainMenuSelections.Classes && ViewModel.SelectedClass != null)
            {
                setCompactHeaderTemplate(CompactHeaderTemplates.Class);
                setCompactSelectedItemContent(ViewModel.SelectedClass);
            }

            else
            {
                setCompactHeaderTemplate(CompactHeaderTemplates.Normal);
                setCompactSelectedItemContent(ViewModel.SelectedItem);
            }
        }

        private enum CompactHeaderTemplates
        {
            Normal,
            Class
        }

        private void setCompactHeaderTemplate(CompactHeaderTemplates template)
        {
            switch (template)
            {
                case CompactHeaderTemplates.Normal:
                    compactSelectedItem.ContentTemplate = Resources["SideBarCompactHeaderNormalTemplate"] as DataTemplate;
                    break;

                case CompactHeaderTemplates.Class:
                    compactSelectedItem.ContentTemplate = Resources["SideBarCompactHeaderClassTemplate"] as DataTemplate;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void setCompactSelectedItemContent(object content)
        {
            compactSelectedItem.Content = content;
        }

        private void ToggleMenu()
        {
            if (this.IsMenuOpen())
            {
                HideMenu();
            }
            else
            {
                ShowMenu();
            }
        }

        private bool IsMenuOpen()
        {
            return sideBarMenuItems.Visibility == Windows.UI.Xaml.Visibility.Visible;
        }

        private void ShowMenu()
        {
            ContentPresenter.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            sideBarMenuItems.Visibility = Windows.UI.Xaml.Visibility.Visible;
            CommandBarForContentContainer.Visibility = Visibility.Collapsed;
        }

        private void HideMenu()
        {
            if (VisualState == PageVisualState.Compact)
            {
                sideBarMenuItems.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                ContentPresenter.Visibility = Windows.UI.Xaml.Visibility.Visible;
                CommandBarForContentContainer.Visibility = Visibility.Visible;
            }
        }

        private enum PageVisualState
        {
            Primary,
            Compact
        }

        private PageVisualState VisualState
        {
            get
            {
                if (ViewStates.CurrentState == null)
                {
                    return PageVisualState.Primary;
                }

                switch (ViewStates.CurrentState.Name)
                {
                    case "PrimaryView":
                        return PageVisualState.Primary;

                    case "CompactView":
                        return PageVisualState.Compact;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void sideBarMenuItems_RequestClose(object sender, EventArgs e)
        {
            HideMenu();
        }

        private void thisPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width >= 676 && !App.IsMobile())
            {
                VisualStateManager.GoToState(this, "PrimaryView", true);
            }

            else
            {
                VisualStateManager.GoToState(this, "CompactView", true);
            }

            UpdateDeviceFamilyViewState();
        }

        private void UpdateDeviceFamilyViewState()
        {
            if (App.IsMobile())
            {
                VisualStateManager.GoToState(this, "MobileState", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "DesktopState", true);
            }
        }

        public void ClearCommandBar()
        {
            CommandBarContentContainer.Content = null;
            CommandBarForContent.PrimaryCommands.Clear();
            CommandBarForContent.SecondaryCommands.Clear();
        }

        public void HideCommandBar()
        {
            // For mobile, we'll keep it visible
            if (!App.IsMobile())
                CommandBarForContent.Visibility = Visibility.Collapsed;
        }

        public void ShowCommandBar()
        {
            CommandBarForContent.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Implicitly shows the command bar if currently hidden
        /// </summary>
        /// <param name="content"></param>
        public void SetCommandBarContent(UIElement content)
        {
            CommandBarContentContainer.Content = content;
        }

        /// <summary>
        /// Implicitly shows command bar if currently hidden
        /// </summary>
        /// <param name="primaryCommands"></param>
        public void SetCommandBarPrimaryCommands(params ICommandBarElement[] primaryCommands)
        {
            CommandBarForContent.PrimaryCommands.Clear();

            if (primaryCommands != null)
                foreach (var c in primaryCommands)
                    CommandBarForContent.PrimaryCommands.Add(c);
        }

        /// <summary>
        /// Implicitly shows command bar if currently hidden
        /// </summary>
        /// <param name="primaryCommands"></param>
        /// <param name="secondaryCommands"></param>
        public void SetCommandBarCommands(IEnumerable<ICommandBarElement> primaryCommands, IEnumerable<ICommandBarElement> secondaryCommands)
        {
            CommandBarForContent.PrimaryCommands.Clear();

            if (primaryCommands != null)
                foreach (var c in primaryCommands)
                    CommandBarForContent.PrimaryCommands.Add(c);

            CommandBarForContent.SecondaryCommands.Clear();

            if (secondaryCommands != null)
                foreach (var c in secondaryCommands)
                    CommandBarForContent.SecondaryCommands.Add(c);
        }

        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenu();
        }
    }
}
