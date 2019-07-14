using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewItemsGroups;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen;
using PowerPlannerAppDataLibrary.ViewItems;
using ToolsPortable;
using CoreGraphics;

namespace PowerPlanneriOS.Views
{
    public class UIPagedDayView : BareUISlideView<UISingleDayView>
    {
        public event EventHandler<ViewItemClass> OnRequestViewClass;
        public event EventHandler<DateTime> DateChanged;

        public UIPagedDayView(SemesterItemsViewGroup semesterItems, MainScreenViewModel mainScreenViewModel)
        {
            // Have to set these here, since when we initialize the views in CreateViews, that gets called
            // from the base class' constructor, which occurs before we could possibly cache these parameters
            foreach (var singleDayView in GetViews())
            {
                singleDayView.SemesterItems = semesterItems;
                singleDayView.MainScreenViewModel = mainScreenViewModel;
                singleDayView.OnRequestViewClass += new WeakEventHandler<ViewItemClass>(SingleDayView_OnRequestViewClass).Handler;
            }
        }

        private void SingleDayView_OnRequestViewClass(object sender, ViewItemClass e)
        {
            OnRequestViewClass?.Invoke(this, e);
        }

        private DateTime _date;
        public DateTime Date
        {
            get { return _date; }
            set
            {
                value = value.Date;
                if (_date == value)
                {
                    return;
                }

                _date = value;
                UpdateAllViews();
            }
        }

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            // Need to hide any popups if they were visible
            UIView hitView = base.HitTest(point, uievent);

            foreach (var e in FindAllEventVisuals())
            {
                if (!e.Descendants().Contains(hitView))
                {
                    e.HideFull();
                }
            }

            return hitView;
        }

        private IEnumerable<UIScheduleViewEventItem> FindAllEventVisuals()
        {
            return this.Descendants().OfType<UIScheduleViewEventItem>();
        }

        protected override UISingleDayView CreateView()
        {
            return new UISingleDayView();
        }

        protected override void OnMovedToNext()
        {
            Date = Date.AddDays(1);
            DateChanged?.Invoke(this, Date);
        }

        protected override void OnMovedToPrevious()
        {
            Date = Date.AddDays(-1);
            DateChanged?.Invoke(this, Date);
        }

        protected override void UpdateCurrView(UISingleDayView curr)
        {
            curr.Date = Date;
        }

        protected override void UpdateNextView(UISingleDayView next)
        {
            next.Date = Date.AddDays(1);
        }

        protected override void UpdatePrevView(UISingleDayView prev)
        {
            prev.Date = Date.AddDays(-1);
        }
    }
}