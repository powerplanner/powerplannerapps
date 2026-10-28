using System;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary;
using InterfacesDroid.ViewModelPresenters;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAndroid.ViewHosts;
using ToolsPortable;
using PowerPlannerAppDataLibrary.Extensions;
using System.ComponentModel;
using PowerPlannerAppDataLibrary.DataLayer;
using System.Collections.Specialized;
using Google.Android.Material.BottomNavigation;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using AndroidX.Core.View;
using BareMvvm.Core.Binding;
using PowerPlannerAndroid.Helpers;
using PowerPlannerAppDataLibrary.Helpers;
using Vx.Droid;
using InterfacesDroid.Themes;
using AndroidX.CoordinatorLayout.Widget;
using InterfacesDroid.Windows;

namespace PowerPlannerAndroid.Views
{
    public class MainScreenView : InterfacesDroid.Views.PopupViewHost<MainScreenViewModel>
    {
        private PopupsPresenter _popupsPresenter;

        public MainScreenView(ViewGroup root) : base(Resource.Layout.MainScreen, root)
        {
            _popupsPresenter = FindViewById<PopupsPresenter>(Resource.Id.MainScreenPopupsPresenter);
        }

        public override void OnViewModelLoadedOverride()
        {
            var mainContentView = FindViewById<FrameLayout>(Resource.Id.MainContentView);
            if (mainContentView.ChildCount > 0)
            {
                throw new InvalidOperationException("This view controller can't be recycled, a new one must be created when assigning a new view model");
            }

            if (ViewModel != null)
            {
                var renderedView = ViewModel.Render();
                renderedView.LayoutParameters = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.MatchParent, FrameLayout.LayoutParams.MatchParent);
                mainContentView.AddView(renderedView);

                _popupsPresenter.ViewModel = ViewModel;

                TryAskingForRatingIfNeeded();
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
                            var builder = new Android.App.AlertDialog.Builder(Context);

                            builder
                                .SetTitle("★ Review App ★")
                                .SetMessage("Thanks for using Power Planner! If you love the app, please leave a rating in the Store! If you have any suggestions or issues, please email me!")
                                .SetNeutralButton("Review", delegate { OpenReview(); }) // Neutral is displayed more prominently
                                .SetPositiveButton("Email Dev", delegate { AboutViewModel.EmailDeveloper(); })
                                .SetNegativeButton("Close", delegate { });

                            builder.Create().Show();

                            PowerPlannerAppDataLibrary.Helpers.Settings.HasAskedForRating = true;
                        }
                    }
                }
            }

            catch { }
        }

        private async void OpenReview()
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