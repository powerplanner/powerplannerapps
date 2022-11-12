using InterfacesUWP;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vx.Uwp;

namespace PowerPlannerUWP.Views.DayViews
{
    public class DayView : SlideView
    {
        private class DayGenerator : SlideViewContentGenerator
        {
            private DateTime _date;
            private DayViewModel _viewModel;

            public DayGenerator(DayViewModel viewModel, DateTime currentDate)
            {
                _viewModel = viewModel;
                _date = currentDate;
            }

            public Windows.UI.Xaml.FrameworkElement GetCurrent()
            {
                return new SingleDayComponent
                {
                    ViewModel = _viewModel,
                    Date = _date,
                    SemesterItemsViewGroup = _viewModel.SemesterItemsViewGroup,
                    LiveProps = new SingleDayComponentLiveProps
                    {
                        IncludeHeader = true
                    }
                }.Render();
            }

            public void MoveNext()
            {
                _date = _date.AddDays(1);
            }

            public void MovePrevious()
            {
                _date = _date.AddDays(-1);
            }

            public DateTime CurrentDate
            {
                get { return _date; }
            }
        }

        public DayViewModel ViewModel { get; private set; }

        public DayView(DayViewModel viewModel)
        {
            base.MinimumColumnWidth = 390;

            // The items have 10px margins so that they'll have 20px gaps between adjacent, and so we need additional 20px on the right side for the outside of the window
            base.ColumnSpacing = 0;

            ViewModel = viewModel;
            SelectedDate = viewModel.CurrentDate;
        }

        public bool CloseExpandedEvents()
        {
            bool closed = false;
            //foreach (var singleDayView in base.Children.OfType<SingleDayView>())
            //{
            //    closed = singleDayView.CloseExpandedEvents() || closed;
            //}
            return closed;
        }

        public DateTime SelectedDate
        {
            get
            {
                if (base.ContentGenerator == null)
                    return DateTime.Today;

                return (base.ContentGenerator as DayGenerator).CurrentDate;
            }

            set
            {
                base.ContentGenerator = new DayGenerator(ViewModel, value);
            }
        }

        protected override void OnChangedCurrent(Windows.UI.Xaml.FrameworkElement oldEl, Windows.UI.Xaml.FrameworkElement newEl)
        {
            try
            {
                ViewModel.CurrentDate = SelectedDate;
                CloseExpandedEvents();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }
    }
}
