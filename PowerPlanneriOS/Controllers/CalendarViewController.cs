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

namespace PowerPlanneriOS.Controllers
{
    public class CalendarViewController : BaseTasksViewController<CalendarViewModel>
    {
        private object _tabBarHeightListener;

        public CalendarViewController()
        {
            this.AutomaticallyAdjustsScrollViewInsets = false;
        }

        public override void OnViewModelLoadedOverride()
        {
            var cal = new MyCalendarView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                DisplayMonth = ViewModel.DisplayMonth
            };
            cal.SetSemester(ViewModel.SemesterItemsViewGroup.Semester);
            cal.Provider = new MyDataProvider(ViewModel.SemesterItemsViewGroup, cal);
            cal.DateClicked += new WeakEventHandler<DateTime>(Cal_DateClicked).Handler;
            cal.DisplayMonthChanged += new WeakEventHandler<DateTime>(Cal_DisplayMonthChanged).Handler;

            View.Add(cal);
            cal.StretchWidth(base.View);
            cal.PinToTop(base.View);

            MainScreenViewController.ListenToTabBarHeightChanged(ref _tabBarHeightListener, delegate
            {
                cal.RemovePinToBottom(base.View).PinToBottom(base.View, (int)MainScreenViewController.TAB_BAR_HEIGHT);
            });

            base.OnViewModelLoadedOverride();
        }

        private void Cal_DisplayMonthChanged(object sender, DateTime e)
        {
            ViewModel.DisplayMonth = e;
        }

        private void Cal_DateClicked(object sender, DateTime e)
        {
            ViewModel.OpenDay(e);
        }

        protected override string GetTitle()
        {
            return "Calendar";
        }

        private class MyCalendarView : BareUICalendarView
        {
            protected override BareUICalendarMonthView CreateView()
            {
                return new MyMonthView();
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
            private MyObservableList<BaseViewItemMegaItem> _holidays;
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