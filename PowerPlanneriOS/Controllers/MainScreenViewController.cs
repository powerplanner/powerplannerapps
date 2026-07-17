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
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using PowerPlannerAppDataLibrary;
using Vx.iOS;

namespace PowerPlanneriOS.Controllers
{
    public class MainScreenViewController : PagedViewModelWithPopupsPresenter
    {
        public static nfloat TAB_BAR_HEIGHT = 0;

        /// <summary>
        /// Must provide a strong reference storage point in your child view controller so that the action will be persisted correctly
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static void ListenToTabBarHeightChanged(ref object strongReferenceStorage, Action action)
        {
            action.Invoke();
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

                    var renderedView = ViewModel.Render();
                    renderedView.TranslatesAutoresizingMaskIntoConstraints = false;
                    base.Add(renderedView);
                    renderedView.StretchWidthAndHeight(base.View);

                    TryAskingForRatingIfNeeded();
                }
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

                        // If they actually have a decent amount of tasks
                        if (await System.Threading.Tasks.Task.Run(async delegate
                        {
                            using (await Locks.LockDataForReadAsync())
                            {
                                return dataStore.HasManyOldMegaItems();
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

                                AboutViewModel.EmailDeveloper();
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
#if !DEBUG
                    // This will only sometimes show a dialog, at most 3 times a year
                    // It will still display if they already rated, meaning users who previously clicked
                    // No thanks on my own dialog will persistently get this dialog, but that should be ok
                    var windowScene = View?.Window?.WindowScene;
                    if (windowScene != null)
                    {
                        if (OperatingSystem.IsIOSVersionAtLeast(18))
                        {
                            StoreKit.AppStore.RequestReview(windowScene);
                        }
                        else
                        {
#pragma warning disable CA1422 // RequestReview(UIWindowScene) is obsoleted on iOS 18.0
                            StoreKit.SKStoreReviewController.RequestReview(windowScene);
#pragma warning restore CA1422
                        }
                    }
#endif
                }
            }

            catch { }
        }

        private static async void OpenStoreReview()
        {
            try
            {
                await ReviewAppExtension.Current?.ReviewAppAsync();
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}