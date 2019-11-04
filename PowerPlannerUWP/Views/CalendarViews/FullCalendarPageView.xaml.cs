using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.CalendarViews
{
    public sealed partial class FullCalendarPageView : UserControl
    {
        public FullCalendarPageView()
        {
            this.InitializeComponent();
        }

        private CalendarViewModel _viewModel;

        private MainCalendarView _calendar;

        public void Initialize(CalendarViewModel viewModel)
        {
            try
            {
                _viewModel = viewModel;

                _calendar = new MainCalendarView(viewModel); // This object handles loading items when month changes

                _calendar.FirstDayOfWeek = viewModel.FirstDayOfWeek;
                _calendar.DisplayMonth = viewModel.DisplayMonth;

                calendarContainer.Child = _calendar;

                _calendar.SelectionChanged += _calendar_SelectionChanged;
                _calendar.DisplayMonthChanged += _calendar_DisplayMonthChanged;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void _calendar_DisplayMonthChanged(object sender, EventArgs e)
        {
            try
            {
                _viewModel.DisplayMonth = _calendar.DisplayMonth;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void _calendar_SelectionChanged(object sender, InterfacesUWP.CalendarFolder.EventArgsCalendar e)
        {
            try
            {
                if (e.SelectedDate == null)
                    return;

                _viewModel.OpenDay(e.SelectedDate.Value);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            showPopupMenuAdd();
        }

        private void buttonToday_Click(object sender, RoutedEventArgs e)
        {
            _calendar.DisplayMonth = DateTime.Today;
        }

        private void showPopupMenuAdd()
        {
            try
            {
                App.ShowFlyoutAddHomeworkOrExam(
                    elToCenterFrom: buttonAdd,
                    addHomeworkAction: delegate { _viewModel.AddHomework(false); },
                    addExamAction: delegate { _viewModel.AddExam(false); },
                    addHolidayAction: delegate { _viewModel.AddHoliday(false); });
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public DateTime DisplayMonth
        {
            get { return _calendar.DisplayMonth; }
        }
    }
}
