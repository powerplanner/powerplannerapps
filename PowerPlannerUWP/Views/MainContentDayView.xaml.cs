using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using PowerPlannerUWP.Views.DayViews;
using PowerPlannerUWPLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainContentDayView : MainScreenContentViewHostGeneric
    {
        public new DayViewModel ViewModel
        {
            get { return base.ViewModel as DayViewModel; }
            set { base.ViewModel = value; }
        }

        public MainContentDayView()
        {
            this.InitializeComponent();

            base.Visibility = Visibility.Collapsed;
        }

#if DEBUG
        ~MainContentDayView()
        {
            System.Diagnostics.Debug.WriteLine("MainContentDayView disposed");
        }
#endif

        private AppBarButton _appBarAdd;
        private AppBarButton AppBarAdd
        {
            get
            {
                if (_appBarAdd == null)
                    _appBarAdd = CreateAppBarButton(Symbol.Add, LocalizedResources.Common.GetStringNewItem(), appBarAdd_Click);

                return _appBarAdd;
            }
        }

        private void appBarAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.ShowFlyoutAddHomeworkOrExam(
                    elToCenterFrom: sender as FrameworkElement,
                    addHomeworkAction: ViewModel.AddHomework,
                    addExamAction: ViewModel.AddExam,
                    addHolidayAction: ViewModel.AddHoliday);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private AppBarButton _appBarGoToToday;
        private AppBarButton AppBarGoToToday
        {
            get
            {
                if (_appBarGoToToday == null)
                    _appBarGoToToday = CreateAppBarButton(Symbol.GoToToday, LocalizedResources.Common.GetStringGoToToday(), AppBarGoToToday_Click);

                return _appBarGoToToday;
            }
        }

        private void AppBarGoToToday_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GoToToday();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void GoToToday()
        {
            _dayView.SelectedDate = DateTime.Today;
        }

        private DayView _dayView;

        public override void OnViewModelLoadedOverride()
        {
            try
            {
                base.OnViewModelLoadedOverride();
                
                _dayView = new DayView(ViewModel);
                MainGrid.Children.Add(_dayView);

                this.SetCommandBarPrimaryCommands(AppBarAdd, AppBarGoToToday);

                base.Visibility = Visibility.Visible;

                ViewModel.BackRequested += new WeakEventHandler<System.ComponentModel.CancelEventArgs>(ViewModel_BackRequested).Handler;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void ViewModel_BackRequested(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_dayView.CloseExpandedEvents())
            {
                e.Cancel = true;
            }
        }
    }
}
