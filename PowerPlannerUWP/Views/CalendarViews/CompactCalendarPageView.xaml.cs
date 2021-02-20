using InterfacesUWP.CalendarFolder;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
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
    public sealed partial class CompactCalendarPageView : UserControl
    {
        public CompactCalendarPageView()
        {
            this.InitializeComponent();
        }

        private CalendarViewModel _viewModel;

        public void Initialize(CalendarViewModel viewModel)
        {
            try
            {
                _viewModel = viewModel;

                smallCalendar.SelectedDate = viewModel.SelectedDate;

                // Not bothering to bind this property for now
                smallCalendar.FirstDayOfWeek = viewModel.FirstDayOfWeek;

                // Set after, since display month can be different than selected date
                smallCalendar.SetBinding(TCalendarView.DisplayMonthProperty, new Binding()
                {
                    Path = new PropertyPath(nameof(viewModel.DisplayMonth)),
                    Source = viewModel,
                    Mode = BindingMode.TwoWay
                });

                smallCalendar.ViewModel = viewModel.SemesterItemsViewGroup;

                smallCalendar.SelectionChanged += SmallCalendar_SelectionChanged;

                listHeader.Classes = viewModel.SemesterItemsViewGroup.Classes;

                UpdateDay();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public bool HandleBackPress()
        {
            if (scheduleSnapshot != null && scheduleSnapshot.HideAnyPopups())
            {
                return true;
            }

            return false;
        }

        private void UpdateDay()
        {
            if (smallCalendar.SelectedDate == null)
                return;

            var date = smallCalendar.SelectedDate.Value;

            setHeaderText(date);
            listHeader.DateToUseForNewItems = date;
            scheduleSnapshot.Initialize(_viewModel.SemesterItemsViewGroup, date);

            listControl.ItemsSource = TasksOrEventsOnDay.Get(_viewModel.MainScreenViewModel.CurrentAccount, _viewModel.SemesterItemsViewGroup.Items, date);
        }

        private void setHeaderText(DateTime date)
        {
            listHeader.Header = getHeaderText(date);
        }

        private string getHeaderText(DateTime date)
        {
            if (date.Date == _viewModel.Today)
                return LocalizedResources.Common.GetRelativeDateToday().ToUpper();

            else if (date.Date == _viewModel.Today.AddDays(1))
                return LocalizedResources.Common.GetRelativeDateTomorrow().ToUpper();

            else if (date.Date == _viewModel.Today.AddDays(-1))
                return LocalizedResources.Common.GetRelativeDateYesterday().ToUpper();

            return date.ToString("dddd, MMM d").ToUpper();
        }

        private void SmallCalendar_SelectionChanged(object sender, InterfacesUWP.CalendarFolder.EventArgsCalendar e)
        {
            try
            {
                if (e.SelectedDate == null)
                    return;

                _viewModel.SelectedDate = e.SelectedDate.Value;

                UpdateDay();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        public DateTime SelectedDate
        {
            get { return smallCalendar.SelectedDate == null ? DateTime.Today : smallCalendar.SelectedDate.Value; }
        }

        public DateTime DisplayMonth
        {
            get { return smallCalendar.DisplayMonth; }
        }

        private void scheduleSnapshot_OnRequestViewHoliday(object sender, ViewItemHoliday e)
        {
            _viewModel.ViewHoliday(e);
        }
    }
}
