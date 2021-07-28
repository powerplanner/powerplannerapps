using InterfacesUWP.CalendarFolder;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ToolsPortable;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerPlannerUWP.Views.CalendarViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompletelyCompactCalendarPageView : Page
    {
        public CompletelyCompactCalendarPageView()
        {
            this.InitializeComponent();
        }

#if DEBUG
        ~CompletelyCompactCalendarPageView()
        {
            System.Diagnostics.Debug.WriteLine("CompletelyCompactCalendar disposed");
        }
#endif

        private void SmallCalendar_SelectionChanged(object sender, InterfacesUWP.CalendarFolder.EventArgsCalendar e)
        {
            try
            {
                if (e.SelectedDate == null)
                    return;

                DateTime selectedDate = e.SelectedDate.Value;

                // Reset to unselected so date can be selected again
                smallCalendar.SelectedDate = null;

                _viewModel.OpenDay(selectedDate.Date);
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private CalendarViewModel _viewModel;

        public void Initialize(CalendarViewModel viewModel)
        {
            try
            {
                _viewModel = viewModel;

                // Not bothering to bind this property for now
                smallCalendar.FirstDayOfWeek = viewModel.FirstDayOfWeek;

                // Don't set selected date, since this doesn't use a selected date
                smallCalendar.SetBinding(TCalendarView.DisplayMonthProperty, new Binding()
                {
                    Path = new PropertyPath(nameof(viewModel.DisplayMonth)),
                    Source = viewModel,
                    Mode = BindingMode.TwoWay
                });

                smallCalendar.ViewModel = viewModel.SemesterItemsViewGroup;

                smallCalendar.SelectionChanged += SmallCalendar_SelectionChanged;
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public void GoToToday()
        {
            smallCalendar.DisplayMonth = DateTime.Today;
        }
    }
}
