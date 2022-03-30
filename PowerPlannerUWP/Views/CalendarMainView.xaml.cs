using InterfacesUWP.CalendarFolder;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using PowerPlannerUWP.Views.CalendarViews;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using Vx.Uwp;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CalendarMainView : MainScreenContentViewHostGeneric
    {
        public new CalendarViewModel ViewModel
        {
            get { return base.ViewModel as CalendarViewModel; }
            set { base.ViewModel = value; }
        }

        public CalendarMainView()
        {
            this.InitializeComponent();
        }

#if DEBUG
        ~CalendarMainView()
        {
            System.Diagnostics.Debug.WriteLine("CalendarMainView disposed");
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
                App.ShowFlyoutAddTaskOrEvent(
                    elToCenterFrom: sender as FrameworkElement,
                    addTaskAction: delegate { ViewModel.AddTask(ShouldUseSelectedDate()); },
                    addEventAction: delegate { ViewModel.AddEvent(ShouldUseSelectedDate()); },
                    addHolidayAction: delegate { ViewModel.AddHoliday(ShouldUseSelectedDate()); });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private bool ShouldUseSelectedDate()
        {
            return _visualState == VisualState.Compact;
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
            var completelyCompact = Root.Child as CompletelyCompactCalendarPageView;

            if (completelyCompact != null)
            {
                completelyCompact.GoToToday();
            }
        }

        public override void OnViewModelLoadedOverride()
        {
            base.OnViewModelLoadedOverride();

            try
            {
                if (base.ActualWidth > 0)
                {
                    UpdateUI();
                }

                base.SizeChanged += CalendarPage_SizeChanged;

                ViewModel.BackRequested += new WeakEventHandler<System.ComponentModel.CancelEventArgs>(ViewModel_BackRequested).Handler;
            }

            catch (Exception ex)
            {
                base.IsEnabled = false;
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void ViewModel_BackRequested(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var compact = Root.Child as CompactCalendarPageView;
            if (compact != null && compact.HandleBackPress())
            {
                e.Cancel = true;
            }
        }

        private void CalendarPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                UpdateUI();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
        
        private void UpdateUI()
        {
            if (base.ActualHeight < 550)
                SetVisualState(VisualState.CompletelyCompact);

            else if (base.ActualWidth < 676)
            {
                if (base.ActualHeight < 700)
                    SetVisualState(VisualState.CompletelyCompact);

                else
                    SetVisualState(VisualState.Compact);
            }

            else
                SetVisualState(VisualState.Full);
        }

        private FullSizeCalendarComponent _fullSizeCalendarComponent;
        private FrameworkElement _fullSizeCalendarComponentElement;
        private VisualState _visualState;
        private void SetVisualState(VisualState state)
        {
            _visualState = state;
            switch (state)
            {
                case VisualState.Full:

                    if (_fullSizeCalendarComponentElement != null && Root.Child == _fullSizeCalendarComponentElement)
                        return;

                    base.HideCommandBar();

                    if (_fullSizeCalendarComponent == null)
                    {
                        _fullSizeCalendarComponent = new FullSizeCalendarComponent(ViewModel);
                        _fullSizeCalendarComponentElement = _fullSizeCalendarComponent.Render();
                        _fullSizeCalendarComponentElement.HorizontalAlignment = HorizontalAlignment.Stretch;
                        _fullSizeCalendarComponentElement.VerticalAlignment = VerticalAlignment.Stretch;
                    }

                    Root.Child = _fullSizeCalendarComponentElement;

                    break;

                case VisualState.Compact:

                    if (Root.Child is CompactCalendarPageView)
                        return;

                    base.HideCommandBar();

                    var compactView = new CompactCalendarPageView();
                    compactView.Initialize(ViewModel);
                    Root.Child = compactView;

                    break;


                case VisualState.CompletelyCompact:

                    if (Root.Child is CompletelyCompactCalendarPageView)
                        return;

                    SetCommandBarPrimaryCommands(AppBarAdd, AppBarGoToToday);

                    var completelyCompact = new CompletelyCompactCalendarPageView();
                    completelyCompact.Initialize(ViewModel);
                    Root.Child = completelyCompact;
                    break;
                    

                default:
                    throw new NotImplementedException();
            }
        }

        private enum VisualState
        {
            Full,
            Compact,
            CompletelyCompact
        }
    }
}
