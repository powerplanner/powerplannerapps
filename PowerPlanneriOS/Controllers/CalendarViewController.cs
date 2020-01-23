using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Controllers;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using InterfacesiOS.Views.Calendar;
using InterfacesiOS.Views;
using System.Threading.Tasks;
using System.Collections;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using CoreGraphics;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using ToolsPortable;
using PowerPlannerAppDataLibrary.ViewLists;
using PowerPlannerAppDataLibrary.ViewItems;
using System.Collections.Specialized;
using PowerPlanneriOS.Views;
using System.ComponentModel;

namespace PowerPlanneriOS.Controllers
{
    public class CalendarViewController : BaseTasksViewController<CalendarViewModel>
    {
        private object _tabBarHeightListener;
        private MyCalendarView _cal;
        private UIPagedDayView _pagedDayView;
        private AdaptiveView _container;

        public CalendarViewController()
        {
            this.AutomaticallyAdjustsScrollViewInsets = false;
        }

        public override void OnViewModelLoadedOverride()
        {
            // Calendar
            _cal = new MyCalendarView(ViewModel.FirstDayOfWeek)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                DisplayMonth = ViewModel.DisplayMonth,
                SelectedDate = ViewModel.SelectedDate
            };
            _cal.SetSemester(ViewModel.SemesterItemsViewGroup.Semester);
            _cal.Provider = new MyDataProvider(ViewModel.SemesterItemsViewGroup, _cal);
            _cal.DateClicked += new WeakEventHandler<DateTime>(Cal_DateClicked).Handler;
            _cal.DisplayMonthChanged += new WeakEventHandler<DateTime>(Cal_DisplayMonthChanged).Handler;

            // Day pager
            _pagedDayView = new UIPagedDayView(ViewModel.SemesterItemsViewGroup, ViewModel.MainScreenViewModel)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Date = ViewModel.SelectedDate
            };
            _pagedDayView.OnRequestViewClass += new WeakEventHandler<ViewItemClass>(PagedDayView_OnRequestViewClass).Handler;
            _pagedDayView.DateChanged += new WeakEventHandler<DateTime>(PagedDayView_DateChanged).Handler;

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            _container = new AdaptiveView(_cal, _pagedDayView, ViewModel)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View.Add(_container);
            _container.StretchWidth(View);
            _container.PinToTop(View);

            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                _container.RemovePinToBottom(View).PinToBottom(View, (int)MainScreenViewController.TAB_BAR_HEIGHT);
            });

            base.OnViewModelLoadedOverride();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedDate):
                    if (_pagedDayView.Date != ViewModel.SelectedDate.Date)
                    {
                        _pagedDayView.Date = ViewModel.SelectedDate.Date;
                    }
                    if (_cal.SelectedDate != ViewModel.SelectedDate.Date)
                    {
                        _cal.SelectedDate = ViewModel.SelectedDate.Date;
                    }
                    break;
            }
        }

        private class AdaptiveView : UIView
        {
            private MyCalendarView _calendarView;
            private UIPagedDayView _dayView;
            private CalendarViewModel _viewModel;

            public bool IsInSplitMode { get; set; }

            public AdaptiveView(MyCalendarView calendarView, UIPagedDayView dayView, CalendarViewModel viewModel)
            {
                _calendarView = calendarView;
                _dayView = dayView;
                _viewModel = viewModel;

                Add(calendarView);
                Add(dayView);
            }

            public override void LayoutSubviews()
            {
                var width = this.Bounds.Width;
                var height = this.Bounds.Height;

                if (height > 500)
                {
                    nfloat splitCalendarHeight = 300;

                    if (height > 950)
                    {
                        splitCalendarHeight = 450;
                    }

                    _calendarView.Frame = new CGRect(0, 0, width, splitCalendarHeight);

                    _dayView.Frame = new CGRect(0, splitCalendarHeight, width, height - splitCalendarHeight);
                    _dayView.Hidden = false;

                    _calendarView.SelectedDate = _viewModel.SelectedDate;

                    IsInSplitMode = true;
                }
                else
                {
                    _calendarView.Frame = new CGRect(0, 0, width, height);
                    _dayView.Hidden = true;

                    _calendarView.SelectedDate = null;

                    IsInSplitMode = false;
                }
            }
        }

        private void PagedDayView_OnRequestViewClass(object sender, ViewItemClass e)
        {
            ViewModel.ViewClass(e);
        }

        private void PagedDayView_DateChanged(object sender, DateTime e)
        {
            ViewModel.SelectedDate = e;
        }

        private void Cal_DisplayMonthChanged(object sender, DateTime e)
        {
            ViewModel.DisplayMonth = e;
        }

        private void Cal_DateClicked(object sender, DateTime e)
        {
            if (_container.IsInSplitMode)
            {
                ViewModel.SelectedDate = e;
            }

            else
            {
                ViewModel.OpenDay(e);
            }
        }

        protected override string GetTitle()
        {
            return "Calendar";
        }

        private class MyCalendarView : BareUICalendarView
        {
            public MyCalendarView(DayOfWeek firstDayOfWeek) : base(firstDayOfWeek) { }

            protected override BareUICalendarMonthView CreateView()
            {
                return new MyMonthView(FirstDayOfWeek);
            }

            public void SetSemester(ViewItemSemester semester)
            {
                foreach (var view in GetViews().OfType<MyMonthView>())
                {
                    view.SetSemester(semester);
                }
            }
        }

        private class MyMonthView : BareUICalendarMonthView
        {
            public MyMonthView(DayOfWeek firstDayOfWeek) : base(firstDayOfWeek) { }

            protected override CGColor GetColorForItem(object item)
            {
                if (item is BaseViewItemHomeworkExam)
                {
                    var c = (item as BaseViewItemHomeworkExam).GetClassOrNull();
                    if (c != null)
                    {
                        return BareUIHelper.ToCGColor(c.Color);
                    }
                }

                return base.GetColorForItem(item);
            }

            public override void LayoutSubviews()
            {
                if (_differentSemesterView != null)
                {
                    _differentSemesterView.Frame = new CGRect(
                        x: 0,
                        y: 0,
                        width: this.Frame.Width,
                        height: this.Frame.Height);
                }

                base.LayoutSubviews();
            }

            private ViewItemSemester _semester;
            public void SetSemester(ViewItemSemester semester)
            {
                _semester = semester;
                UpdateIsDifferentSemester();
            }

            protected override void OnMonthChanged()
            {
                UpdateIsDifferentSemester();

                base.OnMonthChanged();
            }

            private void UpdateIsDifferentSemester()
            {
                bool isDifferentSemester = _semester != null && !_semester.IsMonthDuringThisSemester(Month);

                SetIsDifferentSemester(isDifferentSemester);
            }

            private UIView _differentSemesterView;
            private void SetIsDifferentSemester(bool isDifferent)
            {
                if (isDifferent)
                {
                    if (_differentSemesterView == null)
                    {
                        _differentSemesterView = new DifferentSemesterOverlayControl(int.MaxValue, 16, 16);
                        this.Add(_differentSemesterView);
                    }
                }
                else
                {
                    if (_differentSemesterView != null)
                    {
                        _differentSemesterView.RemoveFromSuperview();
                        _differentSemesterView = null;
                    }
                }
            }
        }

        private class MyDataProvider : BareUICalendarItemsSourceProvider
        {
            private SemesterItemsViewGroup _semesterItems;
            private MyObservableList<BaseViewItemHomeworkExamGrade> _holidays;
            private MyCalendarView _calendarView;
            public MyDataProvider(SemesterItemsViewGroup semesterItems, MyCalendarView calendarView)
            {
                _calendarView = calendarView;
                _semesterItems = semesterItems;
                _holidays = semesterItems.Items.Sublist(i => i is ViewItemHoliday);
                _holidays.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(_holidays_CollectionChanged).Handler;
            }

            private void _holidays_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                _calendarView.UpdateAllBackgroundColors();
            }

            public override IEnumerable GetItemsSource(DateTime date)
            {
                return HomeworksOnDay.Get(_semesterItems.Items, date)
                    .Sublist(i => (i is BaseViewItemHomeworkExam) && !(i as BaseViewItemHomeworkExam).IsComplete());
            }

            public override CGColor GetBackgroundColorForDate(DateTime date)
            {
                if (_holidays.OfType<ViewItemHoliday>().Any(i => date >= i.Date.Date && date.Date <= i.EndTime.Date))
                {
                    return new CGColor(228 / 255f, 0, 137 / 225f, 0.3f);
                }

                return base.GetBackgroundColorForDate(date);
            }
        }
    }
}