using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using ToolsPortable;
using System.ComponentModel;
using InterfacesiOS.ViewModelPresenters;
using InterfacesiOS.Views;
using System.Collections.Specialized;
using PowerPlanneriOS.Helpers;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.Extensions;

namespace PowerPlanneriOS.Controllers
{
    public class MainScreenViewController : PagedViewModelWithPopupsPresenter, IUIAlertViewDelegate
    {
        public static nfloat TAB_BAR_HEIGHT = 0;
        private static WeakReferenceList<Action> OnTabBarHeightChangedListeners = new WeakReferenceList<Action>();

        /// <summary>
        /// Must provide a strong reference storage point in your child view controller so that the action will be persisted correctly
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static void ListenToTabBarHeightChanged(ref object strongReferenceStorage, Action action)
        {
            OnTabBarHeightChangedListeners.Add(action);
            OnTabBarHeightChangedListeners.CleanUpStaleReferences();
            if (TAB_BAR_HEIGHT != 0)
            {
                action.Invoke();
            }

            strongReferenceStorage = action;
        }

        ~MainScreenViewController()
        {
            System.Diagnostics.Debug.WriteLine("Disposed main screen view controller");
        }

        public new MainScreenViewModel ViewModel
        {
            get { return base.ViewModel as MainScreenViewModel; }
            set
            {
                if (base.ViewModel != null)
                {
                    throw new InvalidOperationException("This view controller can't be recycled, a new one must be created when assigning a new view model");
                }

                if (value != null)
                {
                    base.ViewModel = value;
                    value.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
                    value.AvailableItems.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(AvailableItems_CollectionChanged).Handler;
                    UpdateSelectedTab();
                    UpdateAvailableTabs();
                    UpdateSyncStates();
                    UpdateOfflineOrSyncErrors();

                    TryAskingForRatingIfNeeded();
                }
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            var newHeight = _tabBar.Bounds.Size.Height + (_bottomGlass != null ? _bottomGlass.Bounds.Size.Height : 0);

            if (TAB_BAR_HEIGHT != newHeight)
            {
                TAB_BAR_HEIGHT = newHeight;

                foreach (var listener in OnTabBarHeightChangedListeners)
                {
                    try
                    {
                        listener();
                    }
                    catch { }
                }
            }

            // Unfortunately we don't find out about notification click for a while, and apparently showing a popup
            // only works after this view loads, so we have our initialization logic here
            AppDelegate._hasActivatedWindow = true;
            if (AppDelegate._handleLaunchAction != null)
            {
                var action = AppDelegate._handleLaunchAction;
                AppDelegate._handleLaunchAction = null;
                var viewModel = (ViewModel?.GetAppWindow() as PowerPlannerAppDataLibrary.Windows.MainAppWindow)?.GetViewModel();
                if (viewModel != null)
                {
                    action(viewModel);
                }
            }

            base.ViewDidLayoutSubviews();
        }

        private void AvailableItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateAvailableTabs();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedItem):
                    UpdateSelectedTab();
                    break;

                case nameof(ViewModel.SyncState):
                    UpdateSyncStates();
                    break;

                case nameof(ViewModel.IsOffline):
                case nameof(ViewModel.HasSyncErrors):
                    UpdateOfflineOrSyncErrors();
                    break;
            }
        }

        private void UpdateOfflineOrSyncErrors()
        {
            if (ViewModel.IsOffline || ViewModel.HasSyncErrors)
            {
                _tabBarItemMore.BadgeValue = "!";
            }
            else
            {
                _tabBarItemMore.BadgeValue = null;
            }
        }

        private void UpdateSyncStates()
        {
            switch (ViewModel.SyncState)
            {
                case MainScreenViewModel.SyncStates.Done:
                    UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
                    break;

                case MainScreenViewModel.SyncStates.Syncing:
                case MainScreenViewModel.SyncStates.UploadingImages:
                    UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
                    break;
            }
        }

        private void UpdateSelectedTab()
        {
            switch (ViewModel.SelectedItem)
            {
                case PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Calendar:
                    _tabBar.SelectedItem = _tabBarItemCalendar;
                    break;

                case PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Agenda:
                    _tabBar.SelectedItem = _tabBarItemAgenda;
                    break;

                case PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Schedule:
                    _tabBar.SelectedItem = _tabBarItemSchedule;
                    break;

                case PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Classes:
                    _tabBar.SelectedItem = _tabBarItemClasses;
                    break;

                case PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Settings:
                    _tabBar.SelectedItem = _tabBarItemMore;
                    break;
            }
        }

        private void UpdateAvailableTabs()
        {
            _tabBarItemCalendar.Enabled = ViewModel.AvailableItems.Contains(PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Calendar);

            _tabBarItemAgenda.Enabled = ViewModel.AvailableItems.Contains(PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Agenda);

            _tabBarItemSchedule.Enabled = ViewModel.AvailableItems.Contains(PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Schedule);

            _tabBarItemClasses.Enabled = ViewModel.AvailableItems.Contains(PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Classes);

            _tabBarItemMore.Enabled = ViewModel.AvailableItems.Contains(PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Settings);
        }

        private UITabBar _tabBar;
        private UITabBarItem _tabBarItemCalendar;
        private UITabBarItem _tabBarItemAgenda;
        private UITabBarItem _tabBarItemSchedule;
        private UITabBarItem _tabBarItemClasses;
        private UITabBarItem _tabBarItemMore;
        private UIView _bottomGlass;

        public MainScreenViewController()
        {
            MyNavigationController.View.TranslatesAutoresizingMaskIntoConstraints = false;
            MyNavigationController.View.StretchWidth(base.View);
            MyNavigationController.View.StretchHeight(base.View);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // https://developer.xamarin.com/Recipes/ios/Content_Controls/Tab_Bar/Create_a_Tab_Bar/

            _tabBarItemCalendar = new UITabBarItem("Calendar", UIImage.FromBundle("TabCalendar"), UIImage.FromBundle("TabCalendarSelected"));

            _tabBarItemAgenda = new UITabBarItem("Agenda", UIImage.FromBundle("TabAgenda"), UIImage.FromBundle("TabAgendaSelected"));

            _tabBarItemSchedule = new UITabBarItem("Schedule", UIImage.FromBundle("TabSchedule"), UIImage.FromBundle("TabScheduleSelected"));

            _tabBarItemClasses = new UITabBarItem("Classes", UIImage.FromBundle("TabClasses"), UIImage.FromBundle("TabClassesSelected"));

            _tabBarItemMore = new UITabBarItem("More", UIImage.FromBundle("TabMore"), UIImage.FromBundle("TabMoreSelected"));

            _tabBar = new UITabBar()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Items = new UITabBarItem[]
                {
                    _tabBarItemCalendar,
                    _tabBarItemAgenda,
                    _tabBarItemSchedule,
                    _tabBarItemClasses,
                    _tabBarItemMore
                },
                //BarTintColor = ColorResources.PowerPlannerBlueChromeColor,
                //UnselectedItemTintColor = UIColor.White,
                //SelectedImageTintColor = UIColor.White
                SelectedImageTintColor = ColorResources.PowerPlannerAccentBlue
            };
            _tabBar.ItemSelected += new WeakEventHandler<UITabBarItemEventArgs>(_tabBar_ItemSelected).Handler;

            // Have to wrap tab bar in a view as per this: https://novemberfive.co/blog/apple-september-event-iphonex-apps/
            // Then that fixes the items getting squished on iPhone X
            var tabBarContainer = new UIView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            tabBarContainer.Add(_tabBar);

            _tabBar.StretchWidth(tabBarContainer);
            _tabBar.StretchHeight(tabBarContainer);

            base.Add(tabBarContainer);
            tabBarContainer.StretchWidth(base.View);
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
                {
                    tabBarContainer.BottomAnchor.ConstraintEqualTo(this.View.SafeAreaLayoutGuide.BottomAnchor)
                });

                // We also add a bottom blur in the safe area, otherwise there's a blank white gap on iPhone X
                _bottomGlass = new BareUIBlurView()
                {
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                base.Add(_bottomGlass);
                _bottomGlass.StretchWidth(this.View);
                _bottomGlass.PinToBottom(this.View);
                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
                {
                    _bottomGlass.TopAnchor.ConstraintEqualTo(this.View.SafeAreaLayoutGuide.BottomAnchor)
                });
            }
            else
            {
                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
                {
                    tabBarContainer.BottomAnchor.ConstraintEqualTo(this.BottomLayoutGuide.GetBottomAnchor())
                });
            }
        }

        private void _tabBar_ItemSelected(object sender, UITabBarItemEventArgs e)
        {
            var item = e.Item;

            if (item == _tabBarItemCalendar)
            {
                ViewModel.SelectedItem = PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Calendar;
            }
            else if (item == _tabBarItemAgenda)
            {
                ViewModel.SelectedItem = PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Agenda;
            }
            else if (item == _tabBarItemSchedule)
            {
                ViewModel.SelectedItem = PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Schedule;
            }
            else if (item == _tabBarItemClasses)
            {
                ViewModel.SelectedItem = PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Classes;
            }
            else if (item == _tabBarItemMore)
            {
                ViewModel.SelectedItem = PowerPlannerAppDataLibrary.NavigationManager.MainMenuSelections.Settings;
            }
        }

        private async void TryAskingForRatingIfNeeded()
        {
            try
            {
                // If we haven't asked for rating yet
                if (!PowerPlannerAppDataLibrary.Helpers.Settings.HasAskedForRating)
                {
                    if (ViewModel.CurrentAccount != null)
                    {
                        var dataStore = await AccountDataStore.Get(ViewModel.CurrentLocalAccountId);

                        // If they actually have a decent amount of homework
                        if (await System.Threading.Tasks.Task.Run(async delegate
                        {
                            using (await Locks.LockDataForReadAsync())
                            {
                                return dataStore.TableMegaItems.Count() > 30 && dataStore.TableMegaItems.Any(i => i.DateCreated < DateTime.Today.AddDays(-60));
                            }
                        }))
                        {
                            var alert = UIAlertController.Create(
                                title: "★ Review App ★",
                                message: "Thanks for using Power Planner! If you love the app, please leave a rating in the Store! If you have any suggestions or issues, please email me!",
                                preferredStyle: UIAlertControllerStyle.Alert);

                            alert.AddAction(UIAlertAction.Create("Review", UIAlertActionStyle.Default, delegate
                            {
                                PowerPlannerAppDataLibrary.Helpers.Settings.HasAskedForRating = true;
                                PowerPlannerAppDataLibrary.Helpers.Settings.HasReviewedOrEmailedDev = true;
                                TelemetryExtension.Current?.TrackEvent("PromptReviewApp_ClickedReview");

                                OpenStoreReview();
                            }));

                            alert.AddAction(UIAlertAction.Create("Email dev", UIAlertActionStyle.Default, delegate
                            {
                                PowerPlannerAppDataLibrary.Helpers.Settings.HasAskedForRating = true;
                                PowerPlannerAppDataLibrary.Helpers.Settings.HasReviewedOrEmailedDev = true;
                                TelemetryExtension.Current?.TrackEvent("PromptReviewApp_ClickedEmailDev");

                                Settings.AboutViewController.EmailDeveloper();
                            }));

                            alert.AddAction(UIAlertAction.Create("No thanks", UIAlertActionStyle.Cancel, delegate
                            {
                                PowerPlannerAppDataLibrary.Helpers.Settings.HasAskedForRating = true;
                                TelemetryExtension.Current?.TrackEvent("PromptReviewApp_ClickedNoThanks");
                            }));

                            PresentViewController(alert, true, null);
                        }
                    }
                }

                // If the user previously clicked No thanks, we'll try the new in-app review dialog
                else if (!PowerPlannerAppDataLibrary.Helpers.Settings.HasReviewedOrEmailedDev)
                {
                    if (UIDevice.CurrentDevice.CheckSystemVersion(10, 3))
                    {
                        // This will only sometimes show a dialog, at most 3 times a year
                        // It will still display if they already rated, meaning users who previously clicked
                        // No thanks on my own dialog will persistently get this dialog, but that should be ok
                        StoreKit.SKStoreReviewController.RequestReview();
                    }
                }
            }

            catch { }
        }

        private static void OpenStoreReview()
        {
            var url = $"itms-apps://itunes.apple.com/app/id1278178608?action=write-review";
            bool opened = false;
            try
            {
                opened = UIApplication.SharedApplication.OpenUrl(new NSUrl(url));
            }
            catch { }

            if (!opened)
            {
                var dontWait = new PortableMessageDialog("The Store failed to open. Try again later.", "Unable to open the Store").ShowAsync();
            }
        }
    }
}