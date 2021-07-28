using PowerPlannerAppDataLibrary.ViewItems;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PowerPlannerUWP.Views.DayViews
{
    public sealed partial class SingleDayView : UserControl
    {
        private DateTime _today;
        private DayViewModel _viewModel;

        public SingleDayView(DayViewModel calendar, DateTime date)
        {
            this.InitializeComponent();

            Date = date;
            _today = calendar.Today;
            _viewModel = calendar;

            setHeaderText(date);
            listHeader.DateToUseForNewItems = date;
            listHeader.Classes = calendar.SemesterItemsViewGroup.Classes;
            scheduleSnapshot.Initialize(calendar.SemesterItemsViewGroup, date);

            listControl.ItemsSource = TasksOrEventsOnDay.Get(calendar.MainScreenViewModel.CurrentAccount, calendar.SemesterItemsViewGroup.Items, date);

            var semester = calendar.SemesterItemsViewGroup.Semester;
            if (semester != null && !semester.IsDateDuringThisSemester(date))
                RootGrid.Children.Add(new DifferentSemesterOverlayControl());
        }

        public bool CloseExpandedEvents()
        {
            return scheduleSnapshot.HideAnyPopups();
        }
        
        public DateTime Date
        {
            get; private set;
        }

        private void setHeaderText(DateTime date)
        {
            listHeader.Header = getHeaderText(date);
        }

        private string getHeaderText(DateTime date)
        {
            if (date.Date == _today)
                return LocalizedResources.Common.GetRelativeDateToday().ToUpper();

            else if (date.Date == _today.AddDays(1))
                return LocalizedResources.Common.GetRelativeDateTomorrow().ToUpper();

            else if (date.Date == _today.AddDays(-1))
                return LocalizedResources.Common.GetRelativeDateYesterday().ToUpper();

            return PowerPlannerAppDataLibrary.Helpers.DateHelpers.ToMediumDateString(date).ToUpper();
        }

        private void OnHeaderClicked(object sender, RoutedEventArgs e)
        {
            //Popup p = new Popup()
            //{
            //    IsLightDismissEnabled = true
            //};
            //p.Child = new SmallCalendarView()
            //{
            //    Width = 330,
            //    Height = 330,
            //    Items = Store.Data.ActiveSemester.Upcoming
            //};
            //p.IsOpen = true;
        }

        private void scheduleSnapshot_OnRequestViewHoliday(object sender, ViewItemHoliday e)
        {
            _viewModel.ViewHoliday(e);
        }
    }
}
