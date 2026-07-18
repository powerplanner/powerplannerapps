using PowerPlannerAppDataLibrary.DataLayer;
using System;
using ToolsUniversal;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerUWP.TileHelpers;
using Vx.Uwp;
using Vx.Components.OnlyForNativeLibraries;
using PowerPlannerAppDataLibrary.Components;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClassView : MainScreenContentViewHostGeneric
    {
        private readonly ItemsControlCollectionBridge _schedulesPreviewBridge;
        private readonly ItemsControlCollectionBridge _tasksPreviewBridge;
        private readonly ItemsControlCollectionBridge _eventsPreviewBridge;
        private ClassViewItemsGroup _previewGroup;

        public new ClassViewModel ViewModel
        {
            get { return base.ViewModel as ClassViewModel; }
            set { base.ViewModel = value; }
        }

        public ClassView()
        {
            this.InitializeComponent();

            _schedulesPreviewBridge = new ItemsControlCollectionBridge(SchedulesPreview);
            _tasksPreviewBridge = new ItemsControlCollectionBridge(TasksPreview);
            _eventsPreviewBridge = new ItemsControlCollectionBridge(EventsPreview);

            Loaded += ClassView_Loaded;
            Unloaded += ClassView_Unloaded;

            Root.RowDefinitions[0].Height = new GridLength(ToolbarComponent.ToolbarHeight);
        }

        private ClassToolbarComponent _toolbar = new ClassToolbarComponent();

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            SetPreviewGroup(ViewModel.ViewItemsGroupClass);

            _toolbar.ViewModel = ViewModel;
            _toolbar.OnPinClass = PinClass;
            _toolbar.OnUnpinClass = UnpinClass;

            Root.Children.Add(_toolbar.Render());
        }

        private void ClassView_Loaded(object sender, RoutedEventArgs e)
        {
            SetPreviewGroup(ViewModel?.ViewItemsGroupClass);
        }

        private void ClassView_Unloaded(object sender, RoutedEventArgs e)
        {
            SetPreviewGroup(null);
        }

        private void SetPreviewGroup(ClassViewItemsGroup group)
        {
            if (_previewGroup != null)
            {
                _previewGroup.PropertyChanged -= PreviewGroup_PropertyChanged;
            }

            _previewGroup = group;

            if (_previewGroup != null)
            {
                _previewGroup.PropertyChanged += PreviewGroup_PropertyChanged;
            }

            _schedulesPreviewBridge.SetSource(group?.Class?.Schedules);
            _tasksPreviewBridge.SetSource(group?.Tasks);
            _eventsPreviewBridge.SetSource(group?.Events);
        }

        private void PreviewGroup_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ClassViewItemsGroup.Tasks):
                    _tasksPreviewBridge.SetSource(_previewGroup.Tasks);
                    break;

                case nameof(ClassViewItemsGroup.Events):
                    _eventsPreviewBridge.SetSource(_previewGroup.Events);
                    break;
            }
        }

        private sealed class ItemsControlCollectionBridge
        {
            private readonly ItemsControl _itemsControl;
            private IEnumerable _source;
            private INotifyCollectionChanged _observableSource;

            public ItemsControlCollectionBridge(ItemsControl itemsControl)
            {
                _itemsControl = itemsControl;
            }

            public void SetSource(IEnumerable source)
            {
                if (ReferenceEquals(_source, source))
                {
                    return;
                }

                if (_observableSource != null)
                {
                    _observableSource.CollectionChanged -= Source_CollectionChanged;
                }

                _source = source;
                _observableSource = source as INotifyCollectionChanged;

                if (_observableSource != null)
                {
                    _observableSource.CollectionChanged += Source_CollectionChanged;
                }

                SynchronizeItems();
            }

            private void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                SynchronizeItems();
            }

            private void SynchronizeItems()
            {
                _itemsControl.Items.Clear();
                if (_source == null)
                {
                    return;
                }

                foreach (object item in _source)
                {
                    _itemsControl.Items.Add(item);
                }
            }
        }

        public override async void OnViewModelLoadedOverride()
        {
            PivotMain.SelectedIndex = ViewModel.InitialPage != null ? (int)ViewModel.InitialPage : _prevSelectedIndex;
            _toolbar.SelectedIndex = PivotMain.SelectedIndex;

            base.OnViewModelLoadedOverride();

            try
            {
                UpdatePinButton();

                ViewModel.ViewItemsGroupClass.LoadTasksAndEvents();
                ViewModel.ViewItemsGroupClass.LoadGrades();
                await ViewModel.GradesViewModel.LoadAsync(); // So that the show weight categories boolean gets initialized
            }

            catch (Exception ex)
            {
                base.IsEnabled = false;
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void tileDetails_Click(object sender, RoutedEventArgs e)
        {
            PivotMain.SelectedIndex = 1;
        }

        private static int _prevSelectedIndex;
        private void PivotMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _toolbar.SelectedIndex = PivotMain.SelectedIndex;
            _prevSelectedIndex = PivotMain.SelectedIndex;
            switch (PivotMain.SelectedIndex)
            {
                case 0:
                    ViewModel.CurrentPage = ClassViewModel.ClassPages.Overview;
                    GoToHomeVisualState();
                    break;

                case 1:
                    ViewModel.CurrentPage = ClassViewModel.ClassPages.Details;
                    GoToDetailsVisualState();
                    break;

                case 2:
                    ViewModel.CurrentPage = ClassViewModel.ClassPages.Times;
                    GoToClassTimesVisualState();
                    break;

                case 3:
                    ViewModel.CurrentPage = ClassViewModel.ClassPages.Tasks;
                    GoToTasksVisualState();
                    break;

                case 4:
                    ViewModel.CurrentPage = ClassViewModel.ClassPages.Events;
                    GoToEventsVisualState();
                    break;

                case 5:
                    ViewModel.CurrentPage = ClassViewModel.ClassPages.Grades;
                    GoToGradesVisualState();
                    break;
            }
        }

        private void GoToHomeVisualState()
        {
            VisualStateManager.GoToState(this, "Home", true);
            GoToExpandedHeaderVisualState();
        }

        private void GoToDetailsVisualState()
        {
            VisualStateManager.GoToState(this, "Details", true);
            GoToCollapsedHeaderVisualState();

            if (PivotItemDetails.Content == null)
            {
                PivotItemDetails.Content = ViewModel.DetailsViewModel.Render();
            }
        }

        private void GoToClassTimesVisualState()
        {
            VisualStateManager.GoToState(this, "ClassTimes", true);
            GoToCollapsedHeaderVisualState();

            if (PivotItemTimes.Content == null)
            {
                PivotItemTimes.Content = ViewModel.TimesViewModel.Render();
            }
        }

        private void GoToTasksVisualState()
        {
            VisualStateManager.GoToState(this, "Tasks", true);
            GoToCollapsedHeaderVisualState();

            if (PivotItemTasks.Content == null)
            {
                PivotItemTasks.Content = ViewModel.TasksViewModel.Render();
            }
        }

        private void GoToEventsVisualState()
        {
            VisualStateManager.GoToState(this, "Events", true);
            GoToCollapsedHeaderVisualState();

            if (PivotItemEvents.Content == null)
            {
                PivotItemEvents.Content = ViewModel.EventsViewModel.Render();
            }
        }

        private void GoToGradesVisualState()
        {
            VisualStateManager.GoToState(this, "Grades", true);
            GoToCollapsedHeaderVisualState();

            if (PivotItemGrades.Content == null)
            {
                PivotItemGrades.Content = ViewModel.GradesViewModel.Render();
            }
        }

        private void GoToCollapsedHeaderVisualState()
        {
            VisualStateManager.GoToState(this, "CollapsedHeader", true);
        }

        private void GoToExpandedHeaderVisualState()
        {
            VisualStateManager.GoToState(this, "ExpandedHeader", true);
        }

        private void tileClassTimes_Click(object sender, RoutedEventArgs e)
        {
            PivotMain.SelectedIndex = 2;
        }

        private void tileTasks_Click(object sender, RoutedEventArgs e)
        {
            PivotMain.SelectedIndex = 3;
        }

        private void tileEvents_Click(object sender, RoutedEventArgs e)
        {
            PivotMain.SelectedIndex = 4;
        }

        private void tileGrades_Click(object sender, RoutedEventArgs e)
        {
            PivotMain.SelectedIndex = 5;
        }

        private void ScrollViewerHome_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            HomeSquaresGrid.DesiredFitToSize = new Size(
                e.NewSize.Width - HomeSquaresGrid.Margin.Left - HomeSquaresGrid.Margin.Right - ScrollViewerHome.Padding.Left - ScrollViewerHome.Padding.Right,
                e.NewSize.Height - HomeSquaresGrid.Margin.Top - HomeSquaresGrid.Margin.Bottom - ScrollViewerHome.Padding.Top - ScrollViewerHome.Padding.Bottom);
        }

        private async void PinClass()
        {
            if (!(await PowerPlannerApp.Current.IsFullVersionAsync()))
            {
                PowerPlannerApp.Current.PromptPurchase(LocalizedResources.GetString("String_PinningClassPremiumFeatureMessage"));
                return;
            }

            if (ViewModel == null || ViewModel.ViewItemsGroupClass.Class == null)
                return;

            var acct = await AccountsManager.GetOrLoad(ViewModel.MainScreenViewModel.CurrentLocalAccountId);
            var data = await AccountDataStore.Get(ViewModel.MainScreenViewModel.CurrentLocalAccountId);

            await ClassTileHelper.PinTileAsync(acct, data, ViewModel.ViewItemsGroupClass.Class.Identifier, ViewModel.ViewItemsGroupClass.Class.Name, ColorTools.GetColor(ViewModel.ViewItemsGroupClass.Class.Color));
        }

        private async void UnpinClass()
        {
            if (ViewModel == null || ViewModel.ViewItemsGroupClass.Class == null)
                return;

            await ClassTileHelper.UnpinTile(ViewModel.MainScreenViewModel.CurrentLocalAccountId, ViewModel.ViewItemsGroupClass.Class.Identifier);
        }

        private void UpdatePinButton()
        {
            try
            {
                if (ViewModel == null || ViewModel.ViewItemsGroupClass.Class == null)
                    return;

                if (ClassTileHelper.IsPinned(ViewModel.MainScreenViewModel.CurrentLocalAccountId, ViewModel.ViewItemsGroupClass.Class.Identifier))
                {
                    _toolbar.IsPinned = true;
                }

                else
                {
                    _toolbar.IsPinned = false;
                }
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
