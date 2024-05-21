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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClassView : MainScreenContentViewHostGeneric
    {
        public new ClassViewModel ViewModel
        {
            get { return base.ViewModel as ClassViewModel; }
            set { base.ViewModel = value; }
        }

        public ClassView()
        {
            this.InitializeComponent();

            Root.RowDefinitions[0].Height = new GridLength(ToolbarComponent.ToolbarHeight);
        }

        private ClassToolbarComponent _toolbar = new ClassToolbarComponent();

        public override void OnViewModelSetOverride()
        {
            base.OnViewModelSetOverride();

            _toolbar.ViewModel = ViewModel;
            _toolbar.OnPinClass = PinClass;
            _toolbar.OnUnpinClass = UnpinClass;

            Root.Children.Add(_toolbar.Render());
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

        public static Guid JustAddedClassId { get; internal set; }
        

        private void EditSchedules()
        {
            ViewModel.EditTimes();
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
                    GoToHomeVisualState();
                    break;

                case 1:
                    GoToDetailsVisualState();
                    break;

                case 2:
                    GoToClassTimesVisualState();
                    break;

                case 3:
                    GoToTasksVisualState();
                    break;

                case 4:
                    GoToEventsVisualState();
                    break;

                case 5:
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

        private void appBarCancelEditClassTimes_Click(object sender, RoutedEventArgs e)
        {
            GoToClassTimesVisualState();
        }

        private void appBarAddTask_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TasksViewModel.Add();
        }

        private void appBarAddEvent_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EventsViewModel.Add();
        }

        private void BorderClassName_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PivotMain.SelectedIndex = 0;
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

        private async void appBarDeleteClass_Click(object sender, RoutedEventArgs e)
        {
            if (await App.ConfirmDelete(LocalizedResources.GetString("String_ConfirmDeleteClassMessage"), LocalizedResources.GetString("String_ConfirmDeleteClassHeader")))
            {
                ViewModel.DeleteClass();
            }
        }
        
        private void ButtonToggleShowPastCompletedTasks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.ViewItemsGroupClass.IsPastCompletedTasksDisplayed)
                    ViewModel.ViewItemsGroupClass.HidePastCompletedTasks();

                else
                    ViewModel.ViewItemsGroupClass.ShowPastCompletedTasks();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void ButtonToggleShowPastCompletedEvents_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.ViewItemsGroupClass.IsPastCompletedEventsDisplayed)
                    ViewModel.ViewItemsGroupClass.HidePastCompletedEvents();

                else
                    ViewModel.ViewItemsGroupClass.ShowPastCompletedEvents();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void WeightCategoryListViewItem_OnRequestViewGrade(object sender, BaseViewItemMegaItem e)
        {
            ViewModel.GradesViewModel.ShowItem(e);
        }

        private void TaskOrEventListViewItem_OnClickItem(object sender, ViewItemTaskOrEvent e)
        {
            ViewModel.GradesViewModel.ShowUnassignedItem(e);
        }
    }
}
