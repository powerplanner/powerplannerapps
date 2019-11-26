using InterfacesUWP;
using PowerPlannerUWP.ViewModel;
using PowerPlannerUWP.Pages;
using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsUniversal;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.Extensions;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.SideBarViews
{
    public sealed partial class SideBarMenuItemsView : UserControl
    {
        /// <summary>
        /// This is for when in mobile/compact mode and the menu is temporarily open... RequestClose gets triggered indicating that the menu should now be closed.
        /// </summary>
        public event EventHandler RequestClose;

        public static readonly DependencyProperty MenuItemsModelProperty = DependencyProperty.Register("MenuItemsModel", typeof(MainScreenViewModel), typeof(SideBarMenuItemsView), new PropertyMetadata(null, OnMenuItemsModelChanged));

        private static void OnMenuItemsModelChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as SideBarMenuItemsView).OnMenuItemsModelChanged(e);
        }

        private void OnMenuItemsModelChanged(DependencyPropertyChangedEventArgs e)
        {
            MainScreenViewModel oldModel = e.OldValue as MainScreenViewModel;

            if (oldModel != null)
                oldModel.PropertyChanged -= Model_PropertyChanged;

            MainScreenViewModel newModel = e.NewValue as MainScreenViewModel;

            if (newModel != null)
                newModel.PropertyChanged += Model_PropertyChanged;
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateAll();
        }

        public MainScreenViewModel MenuItemsModel
        {
            get { return GetValue(MenuItemsModelProperty) as MainScreenViewModel; }
            set { SetValue(MenuItemsModelProperty, value); }
        }

        public Guid LocalAccountIdentifier
        {
            get
            {
                return MenuItemsModel.CurrentLocalAccountId;
            }
        }

        public SideBarMenuItemsView()
        {
            this.InitializeComponent();
        }

        private void ListViewClasses_AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItemsModel.AddClass(navigateToClassAfterAdd: true);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private Button _listViewAddClassButton;
        private void ListViewClasses_AddButton_Loaded(object sender, RoutedEventArgs e)
        {
            _listViewAddClassButton = (Button)sender;

            UpdateShowAddClassButton();
        }

        private ListView _listViewClasses;
        private void ListViewClasses_Loaded(object sender, RoutedEventArgs e)
        {
            _listViewClasses = (ListView)sender;

            _listViewClasses.SetBinding(ListView.ItemsSourceProperty, new Binding()
            {
                Path = new PropertyPath("MenuItemsModel.Classes"),
                Source = this
            });

            UpdateSelectedClass();

            UpdateShowClasses();
        }

        private void UpdateSelectedClass()
        {
            if (_listViewClasses == null || MenuItemsModel == null)
                return;

            try
            {
                _listViewClasses.SelectedItem = MenuItemsModel.SelectedClass;
            }

            catch { }
        }

        private void UpdateShowClasses()
        {
            if (_listViewClasses == null || MenuItemsModel == null)
                return;

            _listViewClasses.Visibility = MenuItemsModel.SelectedItem == NavigationManager.MainMenuSelections.Classes ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateShowAddClassButton()
        {
            if (_listViewAddClassButton == null || MenuItemsModel == null)
                return;

            _listViewAddClassButton.Visibility = MenuItemsModel.SelectedItem == NavigationManager.MainMenuSelections.Classes ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateAll()
        {
            UpdateShowClasses();
            UpdateShowAddClassButton();
            UpdateSelectedClass();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = (ListView)sender;

            //if (e.AddedItems.Count == 0)
            //{
            //    if (e.RemovedItems.Count > 0)
            //    {
            //        // Try to re-select the selected item
            //        listView.SelectedItem = e.RemovedItems[0];
            //    }
            //}
        }

        private void listViewMain_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!(e.ClickedItem is NavigationManager.MainMenuSelections))
                return;

            var selected = (NavigationManager.MainMenuSelections)e.ClickedItem;

            if (selected == NavigationManager.MainMenuSelections.Classes)
            {
                // Clicking toggles visibility
                if (_listViewClasses != null)
                {
                    _listViewClasses.Visibility = _listViewClasses.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                }

                // If there's classes, we won't close since user can pick a class
                if (MenuItemsModel != null && MenuItemsModel.Classes != null && MenuItemsModel.Classes.Count > 0)
                    return;
            }

            if (RequestClose != null)
                RequestClose(this, new EventArgs());
        }

        private void ListViewClasses_ItemClick(object sender, ItemClickEventArgs e)
        {
            var c = e.ClickedItem as ViewItemClass;
            if (c != null)
            {
                if (MenuItemsModel != null && MenuItemsModel.SelectedItem != null && MenuItemsModel.SelectedItem.Value == NavigationManager.MainMenuSelections.Classes)
                {
                    if (MenuItemsModel.SelectedClass == null || MenuItemsModel.SelectedClass.Identifier != c.Identifier)
                    {
                        MenuItemsModel.SelectClassWithinSemester(c);
                    }
                }
            }

            if (RequestClose != null)
                RequestClose(this, new EventArgs());
        }
    }
}
