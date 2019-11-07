using System;
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
using Android.Support.V4.Widget;
using InterfacesDroid.DataTemplates;
using Android.Graphics;
using Android.Graphics.Drawables;
using InterfacesDroid.Themes;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAndroid.ViewHosts;
using InterfacesDroid.Helpers;
using BareMvvm.Core.App;
using Android.Support.V7.App;
using ToolsPortable;
using Android.Support.V4.Content;
using InterfacesDroid.Bindings.Programmatic;
using PowerPlannerAndroid.Views.ListItems;
using PowerPlannerAppDataLibrary.Extensions;
using System.ComponentModel;
using static Android.Views.View;
using PowerPlannerAppDataLibrary.DataLayer;
using Android.Support.Design.Widget;

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
        private ItemsControlWrapper _itemsWrapperMenuItems;
        public  Android.Support.V7.Widget.Toolbar Toolbar { get; private set; }

        public MainScreenView(ViewGroup root) : base(Resource.Layout.MainScreen, root)
        {
            Toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.Toolbar);
            Toolbar.MenuItemClick += Toolbar_MenuItemClick;
            Toolbar.SetNavigationIcon(Resource.Drawable.ic_menu_white_24dp);
            Toolbar.SetNavigationOnClickListener(this);
            _syncProgressBar = FindViewById<ProgressBar>(Resource.Id.SyncProgressBar);
            _popupsPresenter = FindViewById<PopupsPresenter>(Resource.Id.MainScreenPopupsPresenter);
            _buttonIsOffline = FindViewById<Button>(Resource.Id.ButtonIsOffline);
            _buttonSyncError = FindViewById<Button>(Resource.Id.ButtonSyncError);
        }

        private void Toolbar_MenuItemClick(object sender, Android.Support.V7.Widget.Toolbar.MenuItemClickEventArgs e)
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

        public override void OnViewModelLoadedOverride()
        {
            var bottomNav = FindViewById<BottomNavigationView>(Resource.Id.BottomNav);
            bottomNav.NavigationItemSelected += BottomNav_NavigationItemSelected;

            _itemsWrapperMenuItems = new ItemsControlWrapper(FindViewById<ViewGroup>(Resource.Id.LinearLayoutMenuItems))
            {
                ItemsSource = ViewModel.AvailableItems,
                ItemTemplate = new CustomDataTemplate<NavigationManager.MainMenuSelections>(CreateMenuItem)
            };

            // Place the content presenter
            _contentPresenter = new PagedViewModelPresenter(Context);
            _contentPresenter.ContentChanged += _contentPresenter_ContentChanged;
            FindViewById<FrameLayout>(Resource.Id.ContentFrame).AddView(_contentPresenter); // Add view before assigning ViewModel, so that parent can be found
            _contentPresenter.ViewModel = ViewModel;

            _popupsPresenter.ViewModel = ViewModel;

            _viewModelPropertyChangedEventHandler = new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
            ViewModel.PropertyChanged += _viewModelPropertyChangedEventHandler;

            UpdateActionBarTitle();
            UpdateClassesVisibility();
            UpdateSyncBarStatus();
            UpdateIsOffline();
            UpdateSyncError();

            _buttonIsOffline.Click += delegate { ViewModel.SyncCurrentAccount(); };
            _buttonSyncError.Click += delegate { _drawerLayout.CloseDrawers(); ViewModel.ViewSyncErrors(); };
            FindViewById<View>(Resource.Id.ImageViewPowerPlannerMenuIcon).Click += delegate { ViewModel.SyncCurrentAccount(); };

            TryAskingForRatingIfNeeded();
        }

        private void BottomNav_NavigationItemSelected(object sender, BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.MenuCalendar:
                    ViewModel.SelectedItem = NavigationManager.MainMenuSelections.Calendar;
                    break;

                case Resource.Id.MenuDay:
                    ViewModel.SelectedItem = NavigationManager.MainMenuSelections.Day;
                    break;

                case Resource.Id.MenuAgenda:
                    ViewModel.SelectedItem = NavigationManager.MainMenuSelections.Agenda;
                    break;

                case Resource.Id.MenuSchedule:
                    ViewModel.SelectedItem = NavigationManager.MainMenuSelections.Schedule;
                    break;

                case Resource.Id.MenuClasses:
                    ViewModel.SelectedItem = NavigationManager.MainMenuSelections.Classes;
                    break;
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

        private WeakReference<View> _classesViewGroupReference;
        private WeakReference<View> _classAddButtonReference;

        private const int MENU_ITEM_PADDING_LEFT_PX = 20;

        private View CreateMenuItem(ViewGroup root, NavigationManager.MainMenuSelections mainMenuSelection)
        {
            return new MenuItemCreator(root, mainMenuSelection, this).View;
        }

        private class MenuItemCreator
        {
            private MainScreenView _mainScreenView;
            private NavigationManager.MainMenuSelections _mainMenuSelection;
            private ItemsControlWrapper _itemsWrapperClasses;

            public View View { get; private set; }

            public MenuItemCreator(ViewGroup root, NavigationManager.MainMenuSelections mainMenuSelection, MainScreenView mainScreenView)
            {
                _mainScreenView = mainScreenView;
                _mainMenuSelection = mainMenuSelection;
                _mainScreenView.ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged1).Handler;

                TextView textView = new TextView(root.Context)
                {
                    Text = PowerPlannerResources.GetStringMenuItem(mainMenuSelection),
                    TextSize = 20,
                    Clickable = true,
                    LayoutParameters = new LinearLayout.LayoutParams(
                        LinearLayout.LayoutParams.MatchParent,
                        LinearLayout.LayoutParams.WrapContent)
                };
                textView.SetTextColor(new Color(255, 255, 255));

                int paddingTopBottom = ThemeHelper.AsPx(root.Context, 11);
                int paddingLeft = ThemeHelper.AsPx(root.Context, MENU_ITEM_PADDING_LEFT_PX);
                textView.SetPaddingRelative(paddingLeft, paddingTopBottom, 0, paddingTopBottom);

                textView.Click += delegate
                {
                    // If we're currently on classes and they clicked classes
                    if (mainMenuSelection == NavigationManager.MainMenuSelections.Classes && _mainScreenView.ViewModel.SelectedItem == NavigationManager.MainMenuSelections.Classes)
                    {
                        // Toggle the classes list
                        _mainScreenView.ToggleClasses();
                    }

                    _mainScreenView.ViewModel.SelectedItem = mainMenuSelection;

                    if (mainMenuSelection != NavigationManager.MainMenuSelections.Classes)
                    {
                        _mainScreenView._drawerLayout.CloseDrawers();
                    }
                };

                if (mainMenuSelection == NavigationManager.MainMenuSelections.Classes)
                {
                    LinearLayout linearLayout = new LinearLayout(root.Context)
                    {
                        Orientation = Orientation.Vertical,
                        LayoutParameters = new LinearLayout.LayoutParams(
                            LinearLayout.LayoutParams.MatchParent,
                            LinearLayout.LayoutParams.WrapContent)
                    };

                    LinearLayout layoutClassesHeaderAndAddButton = new LinearLayout(root.Context)
                    {
                        Orientation = Orientation.Horizontal,
                        LayoutParameters = new LinearLayout.LayoutParams(
                            LinearLayout.LayoutParams.MatchParent,
                            LinearLayout.LayoutParams.WrapContent)
                    };

                    // This width will stretch
                    textView.LayoutParameters.Width = 0;
                    (textView.LayoutParameters as LinearLayout.LayoutParams).Weight = 1;

                    layoutClassesHeaderAndAddButton.AddView(textView);

                    // And then the add class button
                    ImageButton addClassButton = new ImageButton(root.Context)
                    {
                        LayoutParameters = new LinearLayout.LayoutParams(
                            LinearLayout.LayoutParams.WrapContent,
                            LinearLayout.LayoutParams.MatchParent),
                        Visibility = ViewStates.Invisible
                    };
                    addClassButton.Background = ThemeHelper.GetAttributeDrawable(root.Context, Resource.Attribute.selectableItemBackground);
                    addClassButton.SetImageDrawable(ContextCompat.GetDrawable(root.Context, Android.Resource.Drawable.IcInputAdd));
                    if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        addClassButton.ImageTintList = new Android.Content.Res.ColorStateList(new int[][] { new int[0] }, new int[] { new Color(255, 255, 255) });
                    }
                    addClassButton.Click += AddClassButton_Click;

                    _mainScreenView._classAddButtonReference = new WeakReference<View>(addClassButton);
                    layoutClassesHeaderAndAddButton.AddView(addClassButton);



                    linearLayout.AddView(layoutClassesHeaderAndAddButton);


                    LinearLayout classesViewGroup = new LinearLayout(root.Context)
                    {
                        Orientation = Orientation.Vertical,
                        LayoutParameters = new LinearLayout.LayoutParams(
                            LinearLayout.LayoutParams.MatchParent,
                            LinearLayout.LayoutParams.WrapContent),
                        Visibility = ViewStates.Gone
                    };

                    _itemsWrapperClasses = new ItemsControlWrapper(classesViewGroup)
                    {
                        ItemsSource = _mainScreenView.ViewModel.Classes,
                        ItemTemplate = new CustomDataTemplate<ViewItemClass>(_mainScreenView.CreateClassMenuItem)
                    };

                    _mainScreenView._classesViewGroupReference = new WeakReference<View>(classesViewGroup);

                    linearLayout.AddView(classesViewGroup);

                    View = linearLayout;
                }

                else
                {
                    View = textView;
                }

                UpdateIsSelected();
            }

            private void AddClassButton_Click(object sender, EventArgs e)
            {
                _mainScreenView._drawerLayout.CloseDrawers();
                _mainScreenView.ViewModel.AddClass(navigateToClassAfterAdd: true);
            }

            private void ViewModel_PropertyChanged1(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(_mainScreenView.ViewModel.SelectedItem):
                        UpdateIsSelected();
                        break;
                }
            }

            private void UpdateIsSelected()
            {
                if (_mainMenuSelection == _mainScreenView.ViewModel.SelectedItem)
                {
                    View.Background = new ColorDrawable(new Color(ContextCompat.GetColor(View.Context, Resource.Color.primaryLight)));
                }

                else
                {
                    View.Background = null;
                }
            }
        }

        private bool AreClassesShown()
        {
            View classesViewGroup = null;
            View classAddButton = null;

            if (_classesViewGroupReference != null)
            {
                _classesViewGroupReference.TryGetTarget(out classesViewGroup);
            }

            if (_classAddButtonReference != null)
            {
                _classAddButtonReference.TryGetTarget(out classAddButton);
            }

            return classesViewGroup != null && classesViewGroup.Visibility == ViewStates.Visible
                && classAddButton != null && classAddButton.Visibility == ViewStates.Visible;
        }

        private void ToggleClasses()
        {
            if (AreClassesShown())
            {
                HideClasses();
            }
            else
            {
                ShowClasses();
            }
        }

        private void ShowClasses()
        {
            View classesViewGroup = null;
            View classAddButton = null;
            
            if (_classesViewGroupReference != null)
            {
                _classesViewGroupReference.TryGetTarget(out classesViewGroup);
            }

            if (_classAddButtonReference != null)
            {
                _classAddButtonReference.TryGetTarget(out classAddButton);
            }

            if (classesViewGroup != null)
                classesViewGroup.Visibility = ViewStates.Visible;

            if (classAddButton != null)
                classAddButton.Visibility = ViewStates.Visible;
        }

        private void HideClasses()
        {
            View classesViewGroup = null;
            View classAddButton = null;

            if (_classesViewGroupReference != null)
            {
                _classesViewGroupReference.TryGetTarget(out classesViewGroup);
            }

            if (_classAddButtonReference != null)
            {
                _classAddButtonReference.TryGetTarget(out classAddButton);
            }

            if (classesViewGroup != null)
                classesViewGroup.Visibility = ViewStates.Gone;

            if (classAddButton != null)
                classAddButton.Visibility = ViewStates.Invisible;
        }

        private View CreateClassMenuItem(ViewGroup root, ViewItemClass c)
        {
            var layout = new ListItemClassMenuItemView(root)
            {
                DataContext = c
            };

            layout.Click += delegate
            {
                var dontWait = ViewModel.SelectClass(c.Identifier);
                _drawerLayout.CloseDrawers();
            };

            UpdateIsClassSelected(layout, c.Identifier);

            PropertyChangedEventHandler propertyChangedHandler = null;
            propertyChangedHandler = new WeakEventHandler<PropertyChangedEventArgs>((s, e) =>
            {
                try
                {
                    if (e.PropertyName.Equals(nameof(ViewModel.SelectedClass)))
                    {
                        UpdateIsClassSelected(layout, c.Identifier);
                    }
                }
                catch (ObjectDisposedException)
                {
                    ViewModel.PropertyChanged -= propertyChangedHandler;
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            }).Handler;
            ViewModel.PropertyChanged += propertyChangedHandler;

            return layout;
        }



        private void UpdateIsClassSelected(View classMenuView, Guid classIdentifier)
        {
            if (ViewModel != null && ViewModel.SelectedClass != null && classIdentifier == ViewModel.SelectedClass.Identifier)
            {
                classMenuView.Background = new ColorDrawable(new Color(61, 153, 219));
            }

            else
            {
                classMenuView.Background = null;
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
            UpdateClassesVisibility();
        }

        private void UpdateClassesVisibility()
        {
            if (ViewModel.SelectedItem == NavigationManager.MainMenuSelections.Classes)
                ShowClasses();
            else
                HideClasses();
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
    }
}