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
    public class CalendarViewController : PopupViewController<CalendarViewModel>
    {
        private MyCalendarView _cal;
        private UIPagedDayView _pagedDayView;
        private AdaptiveView _container;

        public CalendarViewController()
        {
            //Title = GetTitle();
            HideBackButton();

            NavItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add)
            {
                Title = "Add item"
            };
            NavItem.RightBarButtonItem.Clicked += new WeakEventHandler<EventArgs>(ButtonAddItem_Clicked).Handler;
        }

        private void ButtonAddItem_Clicked(object sender, EventArgs e)
        {
            // https://developer.xamarin.com/recipes/ios/standard_controls/alertcontroller/#ActionSheet_Alert
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            actionSheetAlert.AddAction(UIAlertAction.Create("Add Task", UIAlertActionStyle.Default, delegate { ViewModel.AddHomework(); }));
            actionSheetAlert.AddAction(UIAlertAction.Create("Add Event", UIAlertActionStyle.Default, delegate { ViewModel.AddExam(); }));
            actionSheetAlert.AddAction(UIAlertAction.Create("Add Holiday", UIAlertActionStyle.Default, delegate { ViewModel.AddHoliday(); }));

            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Required for iPad - You must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.BarButtonItem = NavItem.RightBarButtonItem;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the alert
            this.PresentViewController(actionSheetAlert, true, null);
        }

        public override void OnViewModelLoadedOverride()
        {
            UpdateTitle();

            // Calendar
            _cal = new MyCalendarView(ViewModel.FirstDayOfWeek)
            {
                DisplayMonth = ViewModel.DisplayMonth,
                SelectedDate = ViewModel.SelectedDate
            };
            _cal.SetSemester(ViewModel.SemesterItemsViewGroup.Semester);
            _cal.Provider = new MyDataProvider(ViewModel.SemesterItemsViewGroup, _cal);
            _cal.DateClicked += new WeakEventHandler<DateTime>(Cal_DateClicked).Handler;
            _cal.DisplayMonthChanged += new WeakEventHandler<DateTime>(Cal_DisplayMonthChanged).Handler;

            // Day pager
            _pagedDayView = new UIPagedDayView(ViewModel.SemesterItemsViewGroup, ViewModel)
            {
                Date = ViewModel.SelectedDate
            };
            _pagedDayView.OnRequestViewClass += new WeakEventHandler<ViewItemClass>(PagedDayView_OnRequestViewClass).Handler;
            _pagedDayView.DateChanged += new WeakEventHandler<DateTime>(PagedDayView_DateChanged).Handler;
            _pagedDayView.OnRequestExpand += new WeakEventHandler(PagedDayView_OnRequestExpand).Handler;

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            _container = new AdaptiveView(_cal, _pagedDayView, ViewModel)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            ContentView.Add(_container);
            _container.StretchWidthAndHeight(ContentView);

            base.OnViewModelLoadedOverride();
        }

        private void PagedDayView_OnRequestExpand(object sender, EventArgs e)
        {
            ViewModel.ExpandDay();
        }

        private UIBarButtonItem _backButton;
        private UIButton _backButtonContents;
        private void UpdateTitle()
        {
            if (ViewModel.DisplayState == CalendarViewModel.DisplayStates.Day)
            {
                Title = "";

                if (_backButtonContents == null)
                {
                    _backButtonContents = new UIButton(UIButtonType.Custom);
                    _backButtonContents.SetImage(UIImage.FromBundle("ToolbarBack").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                    _backButtonContents.TouchUpInside += _backButton_Clicked;
                }

                _backButtonContents.SetTitle(UISingleDayView.GetHeaderText(ViewModel.SelectedDate), UIControlState.Normal);
                _backButtonContents.SizeToFit();

                if (_backButton == null)
                {
                    _backButton = new UIBarButtonItem(_backButtonContents);
                }

                NavItem.LeftBarButtonItem = _backButton;
            }

            else
            {
                Title = ViewModel.DisplayMonth.ToString("MMMM yyyy");
                NavItem.LeftBarButtonItem = null;
            }
        }

        private void _backButton_Clicked(object sender, EventArgs e)
        {
            ViewModel.BackToCalendar();
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
                    if (ViewModel.DisplayState == CalendarViewModel.DisplayStates.Day)
                    {
                        UpdateTitle();
                    }
                    break;

                case nameof(ViewModel.DisplayMonth):
                    UpdateTitle();
                    break;

                case nameof(ViewModel.DisplayState):
                    UpdateTitle();
                    break;
            }
        }

        private enum SplitModes
        {
            Calendar,
            Split,
            Day
        }

        private class AdaptiveView : UIView
        {
            private MyCalendarView _calendarView;
            private UIPagedDayView _dayView;
            private CalendarViewModel _viewModel;

            public AdaptiveView(MyCalendarView calendarView, UIPagedDayView dayView, CalendarViewModel viewModel)
            {
                _calendarView = calendarView;
                _dayView = dayView;
                _viewModel = viewModel;

                _viewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

                Add(calendarView);
                Add(dayView);
            }

            private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(ViewModel.DisplayState):
                        UIView.Animate(0.4, () => UpdateLayout(base.Bounds));
                        break;
                }
            }

            public override CGRect Bounds
            {
                get => base.Bounds;
                set
                {
                    base.Bounds = value;

                    if (value.Height > 500)
                    {
                        _viewModel.ViewSizeState = CalendarViewModel.ViewSizeStates.Compact;
                    }
                    else
                    {
                        _viewModel.ViewSizeState = CalendarViewModel.ViewSizeStates.FullyCompact;
                    }

                    UpdateLayout(value);
                }
            }

            private void UpdateLayout(CGRect bounds)
            {
                var width = bounds.Width;
                var height = bounds.Height;

                switch (_viewModel.DisplayState)
                {
                    case CalendarViewModel.DisplayStates.CompactCalendar:
                        {
                            _calendarView.Hidden = false;
                            _calendarView.Frame = new CGRect(0, 0, width, height);
                            _dayView.Hidden = true;
                        }
                        break;

                    case CalendarViewModel.DisplayStates.Day:
                        {
                            _calendarView.Hidden = true;
                            _dayView.Frame = new CGRect(0, 0, width, height);
                            _dayView.Hidden = false;
                        }
                        break;

                    case CalendarViewModel.DisplayStates.Split:
                        {
                            _calendarView.Hidden = false;
                            _dayView.Hidden = false;

                            nfloat splitCalendarHeight = 300;

                            if (height > 950)
                            {
                                splitCalendarHeight = 450;
                            }

                            _calendarView.Frame = new CGRect(0, 0, width, splitCalendarHeight);

                            _dayView.Frame = new CGRect(0, splitCalendarHeight, width, height - splitCalendarHeight);
                        }
                        break;
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
            ViewModel.SelectedDate = e;
        }

        protected string GetTitle()
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