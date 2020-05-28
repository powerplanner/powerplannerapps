﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InterfacesDroid.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary;
using InterfacesDroid.ViewModelPresenters;
using InterfacesDroid.DataTemplates;
using Android.Graphics;
using Android.Graphics.Drawables;
using InterfacesDroid.Themes;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAndroid.ViewHosts;
using InterfacesDroid.Helpers;
using BareMvvm.Core.App;
using ToolsPortable;
using InterfacesDroid.Bindings.Programmatic;
using PowerPlannerAndroid.Views.ListItems;
using PowerPlannerAppDataLibrary.Extensions;
using System.ComponentModel;
using static Android.Views.View;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAndroid.Helpers;
using System.Collections.Specialized;
using Google.Android.Material.BottomNavigation;
using AndroidX.DrawerLayout.Widget;

namespace PowerPlannerAndroid.Views
{
    public class MainScreenView : InterfacesDroid.Views.PopupViewHost<MainScreenViewModel>, IOnClickListener
    {
        private DrawerLayout _drawerLayout;
        private PagedViewModelPresenter _contentPresenter;
        private PopupsPresenter _popupsPresenter;
        private ProgressBar _syncProgressBar;
        private Button _buttonIsOffline;
        private Button _buttonSyncError;
        public AndroidX.AppCompat.Widget.Toolbar Toolbar { get; private set; }

        public MainScreenView(ViewGroup root) : base(Resource.Layout.MainScreen, root)
        {
            Toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.Toolbar);
            Toolbar.MenuItemClick += Toolbar_MenuItemClick;
            Toolbar.SetNavigationIcon(Resource.Drawable.ic_menu_white_24dp);
            Toolbar.SetNavigationOnClickListener(this);
            _syncProgressBar = FindViewById<ProgressBar>(Resource.Id.SyncProgressBar);
            _popupsPresenter = FindViewById<PopupsPresenter>(Resource.Id.MainScreenPopupsPresenter);
            _buttonIsOffline = FindViewById<Button>(Resource.Id.ButtonIsOffline);
            _buttonSyncError = FindViewById<Button>(Resource.Id.ButtonSyncError);
        }

        private void Toolbar_MenuItemClick(object sender, AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs e)
        {
            IMainScreenToolbarHandler handler = _contentPresenter?.CurrentView as IMainScreenToolbarHandler;

            if (handler != null)
                handler.OnMenuItemClick(e);
        }

        protected override void OnAttachedToWindow()
        {
            PortableApp.Current.GetCurrentWindow().BackPressed += new WeakEventHandler<System.ComponentModel.CancelEventArgs>(MainScreenView_BackPressed).Handler;

            base.OnAttachedToWindow();
        }

        private void MainScreenView_BackPressed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_drawerLayout.IsDrawerOpen(_drawerLayout.GetChildAt(1)))
                {
                    _drawerLayout.CloseDrawers();
                    e.Cancel = true;
                }
            }

            catch { }
        }

        protected override void OnViewCreated()
        {
            _drawerLayout = FindViewById<DrawerLayout>(Resource.Id.MenuDrawerLayout);
        }

        private PropertyChangedEventHandler _viewModelPropertyChangedEventHandler;

        private BottomNavigationView _bottomNav;

        public override void OnViewModelLoadedOverride()
        {
            _bottomNav = FindViewById<BottomNavigationView>(Resource.Id.BottomNav);
            _bottomNav.NavigationItemSelected += BottomNav_NavigationItemSelected;

            // Place the content presenter
            _contentPresenter = new PagedViewModelPresenter(Context);
            _contentPresenter.ContentChanged += _contentPresenter_ContentChanged;
            FindViewById<FrameLayout>(Resource.Id.ContentFrame).AddView(_contentPresenter); // Add view before assigning ViewModel, so that parent can be found
            _contentPresenter.ViewModel = ViewModel;

            _popupsPresenter.ViewModel = ViewModel;

            _viewModelPropertyChangedEventHandler = new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
            ViewModel.PropertyChanged += _viewModelPropertyChangedEventHandler;
            ViewModel.AvailableItems.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(AvailableItems_CollectionChanged).Handler;

            UpdateActionBarTitle();
            UpdateBottomNavMenu();
            UpdateSyncBarStatus();
            UpdateIsOffline();
            UpdateSyncError();

            _buttonIsOffline.Click += delegate { ViewModel.SyncCurrentAccount(); };
            _buttonSyncError.Click += delegate { _drawerLayout.CloseDrawers(); ViewModel.ViewSyncErrors(); };
            FindViewById<View>(Resource.Id.ImageViewPowerPlannerMenuIcon).Click += delegate { ViewModel.SyncCurrentAccount(); };

            FindViewById(Resource.Id.MenuItemYears).Click += delegate { CloseDrawer(); ViewModel.OpenYears(); };
            FindViewById(Resource.Id.MenuItemSettings).Click += delegate { CloseDrawer(); ViewModel.OpenSettings(); };

            TryAskingForRatingIfNeeded();
        }

        private void AvailableItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateBottomNavMenu();
        }

        private bool? _bottomMenuHasAll;
        private void UpdateBottomNavMenu()
        {
            bool shouldHaveAll = ViewModel.AvailableItems.Contains(NavigationManager.MainMenuSelections.Calendar);

            if (_bottomMenuHasAll != null && _bottomMenuHasAll.Value == shouldHaveAll)
            {
                // If we're already set up correctly
                return;
            }

            _bottomMenuHasAll = shouldHaveAll;

            _bottomNav.Menu.Clear();

            if (shouldHaveAll)
            {
                AddMenuItem(NavigationManager.MainMenuSelections.Calendar, "MainMenuItem_Calendar", Resource.Drawable.ic_icons8_calendar_24);
                AddMenuItem(NavigationManager.MainMenuSelections.Day, "MainMenuItem_Day", Resource.Drawable.ic_icons8_today_apps_24);
                AddMenuItem(NavigationManager.MainMenuSelections.Agenda, "MainMenuItem_Agenda", Resource.Drawable.ic_icons8_todo_list_24);
            }

            AddMenuItem(NavigationManager.MainMenuSelections.Schedule, "MainMenuItem_Schedule", Resource.Drawable.ic_icons8_timesheet_24);
            AddMenuItem(NavigationManager.MainMenuSelections.Classes, "MainMenuItem_Classes", Resource.Drawable.ic_icons8_book_shelf_24);

            UpdateSelectedMenuItem();
        }

        private void AddMenuItem(NavigationManager.MainMenuSelections selection, string localizedId, int icon)
        {
            _bottomNav.Menu.Add(Menu.None, (int)selection, Menu.None, PowerPlannerResources.GetString(localizedId)).SetIcon(icon);
        }

        private void UpdateSelectedMenuItem()
        {
            if (ViewModel.SelectedItem != null)
            {
                _bottomNav.SelectedItemId = (int)ViewModel.SelectedItem.Value;
            }
        }

        private void BottomNav_NavigationItemSelected(object sender, BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            ViewModel.SelectedItem = (NavigationManager.MainMenuSelections)e.Item.ItemId;
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
                                return dataStore.TableMegaItems.Count() > 30 && dataStore.TableMegaItems.Any(i => i.DateCreated < DateTime.Today.AddDays(-60));
                            }
                        }))
                        {
                            var builder = new Android.App.AlertDialog.Builder(Context);

                            builder
                                .SetTitle("★ Review App ★")
                                .SetMessage("Thanks for using Power Planner! If you love the app, please leave a rating in the Store! If you have any suggestions or issues, please email me!")
                                .SetNeutralButton("Review", delegate { OpenReview(); }) // Neutral is displayed more prominently
                                .SetPositiveButton("Email Dev", delegate { AboutView.EmailDeveloper(Context, base.ViewModel); })
                                .SetNegativeButton("Close", delegate { });

                            builder.Create().Show();

                            PowerPlannerAppDataLibrary.Helpers.Settings.HasAskedForRating = true;
                        }
                    }
                }
            }

            catch { }
        }

        private void OpenReview()
        {
            Intent intent = new Intent(Intent.ActionView);
            intent.SetData(Android.Net.Uri.Parse("market://details?id=" + Context.PackageName));

            if (!TryStartActivity(intent))
            {
                // Google Play app not installed, try open web browser
                intent.SetData(Android.Net.Uri.Parse("https://play.google.com/store/apps/details?id=" + Context.PackageName));

                if (!TryStartActivity(intent))
                {
                    Toast.MakeText(Context, "Google Play not available", ToastLength.Short).Show();
                }
            }
        }

        private bool TryStartActivity(Intent intent)
        {
            try
            {
                Context.StartActivity(intent);
                return true;
            }
            catch (ActivityNotFoundException)
            {
                return false;
            }
        }

        private void _contentPresenter_ContentChanged(object sender, EventArgs e)
        {
            IMainScreenToolbarHandler handler = _contentPresenter?.CurrentView as IMainScreenToolbarHandler;

            if (handler != null)
                handler.RequestUpdateMenu();
            else
                Toolbar.Menu.Clear();
        }

        private void UpdateIsOffline()
        {
            _buttonIsOffline.Visibility = ViewModel.IsOffline ? ViewStates.Visible : ViewStates.Gone;
        }

        private void UpdateSyncError()
        {
            _buttonSyncError.Visibility = ViewModel.HasSyncErrors ? ViewStates.Visible : ViewStates.Gone;
        }

        private void UpdateSyncBarStatus()
        {
            switch (ViewModel.SyncState)
            {
                case MainScreenViewModel.SyncStates.Done:
                    _syncProgressBar.Indeterminate = false;
                    _syncProgressBar.Progress = 0;
                    _syncProgressBar.Visibility = ViewStates.Gone;
                    break;

                case MainScreenViewModel.SyncStates.Syncing:
                    _syncProgressBar.Indeterminate = true;
                    _syncProgressBar.Visibility = ViewStates.Visible;
                    break;

                case MainScreenViewModel.SyncStates.UploadingImages:
                    _syncProgressBar.Indeterminate = false;
                    _syncProgressBar.Visibility = ViewStates.Visible;
                    _syncProgressBar.Progress = (int)(ViewModel.UploadImageProgress * 100);
                    break;
            }
        }

        private BindingInstance _selectedClassNameBinding;
        private void UpdateActionBarTitle()
        {
            if (_selectedClassNameBinding != null)
            {
                _selectedClassNameBinding.Dispose();
                _selectedClassNameBinding = null;
            }

            if (ViewModel.SelectedItem == NavigationManager.MainMenuSelections.Classes)
            {
                if (ViewModel.SelectedClass != null)
                {
                    _selectedClassNameBinding = ViewModel.SelectedClass.SetBinding(nameof(ViewItemClass.Name), (c) =>
                    {
                        Toolbar.Title = c.Name;
                    });
                }
                else
                    Toolbar.Title = PowerPlannerResources.GetStringMenuItem(NavigationManager.MainMenuSelections.Classes);
            }

            else
            {
                Toolbar.Title = PowerPlannerResources.GetStringMenuItem(ViewModel.SelectedItem.GetValueOrDefault());
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                switch (e.PropertyName)
                {
                    case nameof(ViewModel.SelectedItem):
                        OnSelectedMenuItemChanged();
                        break;

                    case nameof(ViewModel.SelectedClass):
                        OnSelectedClassChanged();
                        break;

                    case nameof(ViewModel.SyncState):
                    case nameof(ViewModel.UploadImageProgress):
                        UpdateSyncBarStatus();
                        break;

                    case nameof(ViewModel.IsOffline):
                        UpdateIsOffline();
                        break;

                    case nameof(ViewModel.HasSyncErrors):
                        UpdateSyncError();
                        break;
                }
            }
            catch (ObjectDisposedException)
            {
                if (ViewModel != null && _viewModelPropertyChangedEventHandler != null)
                {
                    ViewModel.PropertyChanged -= _viewModelPropertyChangedEventHandler;
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void OnSelectedMenuItemChanged()
        {
            UpdateActionBarTitle();
            UpdateSelectedMenuItem();
        }

        private void OnSelectedClassChanged()
        {
            UpdateActionBarTitle();
        }

        public void OnClick(View v)
        {
            // Navigation on click
            _drawerLayout.OpenDrawer(_drawerLayout.GetChildAt(1));
        }

        public void CloseDrawer()
        {
            _drawerLayout.CloseDrawers();
        }
    }
}