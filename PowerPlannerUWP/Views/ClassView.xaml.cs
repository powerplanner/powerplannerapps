using PowerPlannerSending;
using PowerPlannerUWP.Views.ClassViews;

using PowerPlannerUWPLibrary;
using PowerPlannerAppDataLibrary.DataLayer;
using PowerPlannerAppDataLibrary.DataLayer.DataItems;
using PowerPlannerUWPLibrary.TileHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using ToolsUniversal;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Class;
using PowerPlannerAppDataLibrary.App;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClassView : MainScreenContentViewHostGeneric
    {
        private AppBarButton _appBarEditClass;
        private AppBarButton AppBarEditClass
        {
            get
            {
                if (_appBarEditClass == null)
                    _appBarEditClass = CreateAppBarButton(Symbol.Edit, LocalizedResources.GetString("String_EditClass"), appBarEditClass_Click);

                return _appBarEditClass;
            }
        }

        private AppBarButton _appBarPinClass;
        private AppBarButton AppBarPinClass
        {
            get
            {
                if (_appBarPinClass == null)
                    _appBarPinClass = CreateAppBarButton(Symbol.Pin, LocalizedResources.GetString("String_PinClass"), appBarPinClass_Click);

                return _appBarPinClass;
            }
        }

        private AppBarButton _appBarCancelEditClass;
        private AppBarButton AppBarCancelEditClass
        {
            get
            {
                if (_appBarCancelEditClass == null)
                    _appBarCancelEditClass = CreateAppBarButton(Symbol.Cancel, LocalizedResources.Common.GetStringCancel(), appBarCancelEditClass_Click);

                return _appBarCancelEditClass;
            }
        }

        private AppBarButton _appBarEditDetails;
        private AppBarButton AppBarEditDetails
        {
            get
            {
                if (_appBarEditDetails == null)
                    _appBarEditDetails = CreateAppBarButton(Symbol.Edit, LocalizedResources.GetString("String_EditDetails"), appBarEditDetails_Click);

                return _appBarEditDetails;
            }
        }

        private AppBarButton _appBarEditClassTimes;
        private AppBarButton AppBarEditClassTimes
        {
            get
            {
                if (_appBarEditClassTimes == null)
                    _appBarEditClassTimes = CreateAppBarButton(Symbol.Edit, LocalizedResources.GetString("String_EditTimes"), appBarEditClassTimes_Click);

                return _appBarEditClassTimes;
            }
        }

        private AppBarButton _appBarAddHomework;
        private AppBarButton AppBarAddHomework
        {
            get
            {
                if (_appBarAddHomework == null)
                    _appBarAddHomework = CreateAppBarButton(Symbol.Add, LocalizedResources.GetString("String_NewHomework"), appBarAddHomework_Click);

                return _appBarAddHomework;
            }
        }

        private AppBarButton _appBarAddExam;
        private AppBarButton AppBarAddExam
        {
            get
            {
                if (_appBarAddExam == null)
                    _appBarAddExam = CreateAppBarButton(Symbol.Add, LocalizedResources.GetString("String_NewExam"), appBarAddExam_Click);

                return _appBarAddExam;
            }
        }

        private AppBarButton _appBarAddGrade;
        private AppBarButton AppBarAddGrade
        {
            get
            {
                if (_appBarAddGrade == null)
                    _appBarAddGrade = CreateAppBarButton(Symbol.Add, LocalizedResources.GetString("String_NewGrade"), appBarAddGrade_Click);

                return _appBarAddGrade;
            }
        }

        private AppBarButton _appBarDeleteClass;
        private AppBarButton AppBarDeleteClass
        {
            get
            {
                if (_appBarDeleteClass == null)
                    _appBarDeleteClass = CreateAppBarButton(Symbol.Delete, LocalizedResources.GetString("String_DeleteClass"), appBarDeleteClass_Click);

                return _appBarDeleteClass;
            }
        }

        public new ClassViewModel ViewModel
        {
            get { return base.ViewModel as ClassViewModel; }
            set { base.ViewModel = value; }
        }

        public ClassView()
        {
            this.InitializeComponent();

            HeaderUnassignedItems.Content = PowerPlannerResources.GetString("ClassGrades_UnassignedItemsHeader");
            ButtonEditGrades.Content = PowerPlannerResources.GetString("AppBarButtonEdit.Label");

            PivotMain.SelectedIndex = _prevSelectedIndex;
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            try
            {
                ViewModel.ViewItemsGroupClass.Class.Schedules.CollectionChanged += Schedules_CollectionChanged;
                UpdateNoSchedulesText();

                UpdateAppBarButtons();
                UpdatePinButton();

                ViewModel.ViewItemsGroupClass.LoadHomeworkAndExams();
                ViewModel.ViewItemsGroupClass.LoadGrades();
            }

            catch (Exception ex)
            {
                base.IsEnabled = false;
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void UpdateNoSchedulesText()
        {
            if (ViewModel.ViewItemsGroupClass.Class.Schedules.Count > 0)
                StackPanelNoSchedules.Visibility = Visibility.Collapsed;
            else
                StackPanelNoSchedules.Visibility = Visibility.Visible;
        }

        private void Schedules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                UpdateNoSchedulesText();
            }

            catch (Exception ex)
            {
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
                    GoToHomeworkVisualState();
                    break;

                case 4:
                    GoToExamsVisualState();
                    break;

                case 5:
                    GoToGradesVisualState();
                    break;
            }

            //if (PivotMain.SelectedIndex != 0)
            //    PivotHeaders.SelectedIndex = PivotMain.SelectedIndex - 1;
        }

        private AppBarButton[] _defaultSecondaryCommands;
        private AppBarButton[] DefaultSecondaryCommands
        {
            get
            {
                if (_defaultSecondaryCommands == null)
                    _defaultSecondaryCommands = new AppBarButton[] { AppBarDeleteClass };

                return _defaultSecondaryCommands;
            }
        }

        private void SetCommandBarCommands(params AppBarButton[] buttons)
        {
            SetCommandBarCommands(buttons, DefaultSecondaryCommands);
        }

        private void UpdateAppBarButtons()
        {
            var state = PivotGroup.CurrentState;

            if (state == Home)
                SetCommandBarCommands(
                    AppBarEditClass,
                    AppBarPinClass);

            else if (state == Details)
                SetCommandBarCommands(
                    AppBarEditDetails);

            else if (state == ClassTimes)
                SetCommandBarCommands(
                    AppBarEditClassTimes);

            else if (state == Homework)
                SetCommandBarCommands(
                    AppBarAddHomework);

            else if (state == Exams)
                SetCommandBarCommands(
                    AppBarAddExam);

            else if (state == Grades)
                SetCommandBarCommands(
                    AppBarAddGrade);
        }

        private void GoToHomeVisualState()
        {
            VisualStateManager.GoToState(this, "Home", true);
            GoToExpandedHeaderVisualState();

            UpdateAppBarButtons();
        }

        private void GoToDetailsVisualState()
        {
            VisualStateManager.GoToState(this, "Details", true);
            GoToCollapsedHeaderVisualState();

            UpdateAppBarButtons();
        }

        private void GoToClassTimesVisualState()
        {
            VisualStateManager.GoToState(this, "ClassTimes", true);
            GoToCollapsedHeaderVisualState();

            UpdateAppBarButtons();
        }

        private void GoToHomeworkVisualState()
        {
            VisualStateManager.GoToState(this, "Homework", true);
            GoToCollapsedHeaderVisualState();

            UpdateAppBarButtons();
        }

        private void GoToExamsVisualState()
        {
            VisualStateManager.GoToState(this, "Exams", true);
            GoToCollapsedHeaderVisualState();

            UpdateAppBarButtons();
        }

        private void GoToGradesVisualState()
        {
            VisualStateManager.GoToState(this, "Grades", true);
            GoToCollapsedHeaderVisualState();

            UpdateAppBarButtons();
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

        private void appBarAddHomework_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.HomeworkViewModel.Add();
        }

        private void appBarAddExam_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ExamsViewModel.Add();
        }

        private void PivotHeaders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If first time, do nothing
            if (e.RemovedItems.Count == 0)
                return;

            //PivotMain.SelectedIndex = PivotHeaders.SelectedIndex + 1;
        }

        private void BorderClassName_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PivotMain.SelectedIndex = 0;
        }

        private void tileClassTimes_Click(object sender, RoutedEventArgs e)
        {
            PivotMain.SelectedIndex = 2;
        }

        private void tileHomework_Click(object sender, RoutedEventArgs e)
        {
            PivotMain.SelectedIndex = 3;
        }

        private void tileExams_Click(object sender, RoutedEventArgs e)
        {
            PivotMain.SelectedIndex = 4;
        }

        private void tileGrades_Click(object sender, RoutedEventArgs e)
        {
            PivotMain.SelectedIndex = 5;
        }

        private void appBarEditClassTimes_Click(object sender, RoutedEventArgs e)
        {
            this.EditSchedules();
        }

        private void appBarEditDetails_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EditDetails();
        }

        private void appBarAddGrade_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GradesViewModel.Add();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void appBarEditClass_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EditClass();
        }

        private void appBarCancelEditClass_Click(object sender, RoutedEventArgs e)
        {
            this.GoToHomeVisualState();
        }

        private void ScrollViewerHome_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            HomeSquaresGrid.DesiredFitToSize = new Size(
                e.NewSize.Width - HomeSquaresGrid.Margin.Left - HomeSquaresGrid.Margin.Right - ScrollViewerHome.Padding.Left - ScrollViewerHome.Padding.Right,
                e.NewSize.Height - HomeSquaresGrid.Margin.Top - HomeSquaresGrid.Margin.Bottom - ScrollViewerHome.Padding.Top - ScrollViewerHome.Padding.Bottom);
        }

        private void ButtonWhatIfMode_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GradesViewModel.OpenWhatIf();
        }

        private async void appBarPinClass_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(await PowerPlannerApp.Current.IsFullVersionAsync()))
                {
                    PowerPlannerApp.Current.PromptPurchase(LocalizedResources.GetString("String_PinningClassPremiumFeatureMessage"));
                    return;
                }

                if (ViewModel == null || ViewModel.ViewItemsGroupClass.Class == null)
                    return;

                if ((AppBarPinClass.Icon as SymbolIcon).Symbol == Symbol.Pin)
                {
                    var acct = await AccountsManager.GetOrLoad(ViewModel.MainScreenViewModel.CurrentLocalAccountId);
                    var data = await AccountDataStore.Get(ViewModel.MainScreenViewModel.CurrentLocalAccountId);

                    await ClassTileHelper.PinTileAsync(acct, data, ViewModel.ViewItemsGroupClass.Class.Identifier, ViewModel.ViewItemsGroupClass.Class.Name, ColorTools.GetColor(ViewModel.ViewItemsGroupClass.Class.Color));
                }

                else
                {
                    await ClassTileHelper.UnpinTile(ViewModel.MainScreenViewModel.CurrentLocalAccountId, ViewModel.ViewItemsGroupClass.Class.Identifier);
                }

                UpdatePinButton();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void UpdatePinButton()
        {
            try
            {
                if (ViewModel == null || ViewModel.ViewItemsGroupClass.Class == null)
                    return;

                if (ClassTileHelper.IsPinned(ViewModel.MainScreenViewModel.CurrentLocalAccountId, ViewModel.ViewItemsGroupClass.Class.Identifier))
                {
                    AppBarPinClass.Icon = new SymbolIcon(Symbol.UnPin);
                    AppBarPinClass.Label = LocalizedResources.GetString("String_UnpinClass");
                }

                else
                {
                    AppBarPinClass.Icon = new SymbolIcon(Symbol.Pin);
                    AppBarPinClass.Label = LocalizedResources.GetString("String_PinClass");
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
        
        private void ButtonToggleShowPastCompletedHomework_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.ViewItemsGroupClass.IsPastCompletedHomeworkDisplayed)
                    ViewModel.ViewItemsGroupClass.HidePastCompletedHomework();

                else
                    ViewModel.ViewItemsGroupClass.ShowPastCompletedHomework();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void ButtonToggleShowPastCompletedExams_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.ViewItemsGroupClass.IsPastCompletedExamsDisplayed)
                    ViewModel.ViewItemsGroupClass.HidePastCompletedExams();

                else
                    ViewModel.ViewItemsGroupClass.ShowPastCompletedExams();
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

        private void HomeworkListViewItem_OnClickItem(object sender, ViewItemTaskOrEvent e)
        {
            ViewModel.GradesViewModel.ShowUnassignedItem(e);
        }

        private void ButtonEditGrades_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GradesViewModel.ConfigureGrades();
        }
    }
}
