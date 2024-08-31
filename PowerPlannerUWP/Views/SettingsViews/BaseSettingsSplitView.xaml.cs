using InterfacesUWP.Views;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.Settings;
using PowerPlannerUWP.ViewModel.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.SettingsViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class BaseSettingsSplitView : ViewHostGeneric
    {
        public new BaseSettingsSplitViewModel ViewModel
        {
            get { return base.ViewModel as BaseSettingsSplitViewModel; }
            set { base.ViewModel = value; }
        }

        /// Take in a list of settings items
        ///  - DisplayName
        ///  - PageType (so can navigate to it)
        ///  
        /// Have a frame to the right of the list where we display the selected page
        /// 
        /// If in compact mode, only show the list or the frame at one time.
        /// Also, handle the back button press such that it displays the list if displaying the page


        /// <summary>
        /// 
        /// </summary>
        public BaseSettingsSplitView()
        {
            this.InitializeComponent();
        }

        public override void OnViewModelLoadedOverride()
        {
            if (ViewModel is TileSettingsViewModel)
            {
                this.Title = LocalizedResources.GetString("Settings_LiveTiles_Title");
                ListBoxItems.ItemsSource = new SettingsListItem[]
                {
                    new SettingsListItem(LocalizedResources.GetString("Settings_LiveTiles_ItemMainTile_DisplayName"), typeof(MainTileViewModel)),
                    new SettingsListItem(LocalizedResources.GetString("Settings_LiveTiles_ItemScheduleTile_DisplayName"), typeof(ScheduleTileViewModel)),
                    new SettingsListItem(LocalizedResources.GetString("Settings_LiveTiles_ItemClassTiles_DisplayName"), typeof(ClassTilesPagedHostViewModel)),
                    new SettingsListItem(LocalizedResources.GetString("Settings_LiveTiles_ItemQuickAddTile_DisplayName"), typeof(QuickAddTileViewModel))
                };
            }

            else if (ViewModel is CalendarIntegrationViewModel)
            {
                this.Title = LocalizedResources.GetString("Settings_CalendarIntegration_Title");
                ListBoxItems.ItemsSource = new SettingsListItem[]
                {
                    new SettingsListItem(LocalizedResources.GetString("Settings_CalendarIntegration_ItemTasks_DisplayName"), typeof(CalendarIntegrationTasksViewModel)),
                    new SettingsListItem(LocalizedResources.GetString("Settings_CalendarIntegration_ItemClasses_DisplayName"), typeof(CalendarIntegrationClassesViewModel))
                };
            }

            else
            {
                throw new NotImplementedException();
            }

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateState();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.State):
                    UpdateState();
                    break;
            }
        }

        private void UpdateState()
        {
            switch (ViewModel.State)
            {
                case BaseSettingsSplitViewModel.ViewState.Both:
                    GoToBothVisibleState();
                    break;

                case BaseSettingsSplitViewModel.ViewState.Content:
                    GoToContentVisibleState();
                    break;

                case BaseSettingsSplitViewModel.ViewState.Items:
                    GoToListVisibleState();
                    break;
            }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(BaseSettingsSplitView), null);

        public string Title
        {
            get { return GetValue(TitleProperty) as string; }
            set { SetValue(TitleProperty, value); }
        }
        
        private void ThisPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsCompactMode())
            {
                ViewModel.SwitchToSingle();
            }

            else
            {
                ViewModel.SwitchToSplit();
            }
        }

        public bool IsCompactMode()
        {
            return base.ActualWidth < 700;
        }

        private void GoToBothVisibleState()
        {
            VisualStateManager.GoToState(this, "BothVisibleState", true);

            // Select first item as default
            if (ListBoxItems.SelectedIndex == -1 && ListBoxItems.Items.Count > 0)
                ListBoxItems.SelectedIndex = 0;
        }

        private void GoToListVisibleState()
        {
            // Clear selected item
            ListBoxItems.SelectedIndex = -1;

            VisualStateManager.GoToState(this, "ListVisibleState", true);
        }

        private void GoToContentVisibleState()
        {
            VisualStateManager.GoToState(this, "ContentVisibleState", true);
        }

        private void ListBoxItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ListBoxItems.SelectedItem as SettingsListItem;
            if (selectedItem != null)
            {
                ViewModel.Content = ViewModel.Items.Where(i => i.GetType() == selectedItem.ViewModelContentType).First();
            }
            else
            {
                ViewModel.Content = null;
            }
        }
    }

    public class SettingsListItem
    {
        public string DisplayName { get; private set; }

        public Type ViewModelContentType { get; private set; }

        public SettingsListItem(string displayName, Type viewModelContentType)
        {
            DisplayName = displayName;
            ViewModelContentType = viewModelContentType;
        }
    }
}
