using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using PowerPlanneriOS.Views;
using ToolsPortable;
using InterfacesiOS.Views;
using PowerPlannerAppDataLibrary.ViewItems;
using System.ComponentModel;

namespace PowerPlanneriOS.Controllers
{
    public class DayViewController : BaseTasksViewController<DayViewModel>
    {
        protected override string GetTitle()
        {
            return "Day";
        }

        public DayViewController()
        {
            AutomaticallyAdjustsScrollViewInsets = false;
        }

        private UIPagedDayView _pagedDayView;
        public override void OnViewModelLoadedOverride()
        {
            _pagedDayView = new UIPagedDayView(ViewModel.SemesterItemsViewGroup, ViewModel.MainScreenViewModel)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Date = ViewModel.CurrentDate
            };
            _pagedDayView.OnRequestViewClass += new WeakEventHandler<ViewItemClass>(PagedDayView_OnRequestViewClass).Handler;
            _pagedDayView.DateChanged += new WeakEventHandler<DateTime>(PagedDayView_DateChanged).Handler;
            View.Add(_pagedDayView);
            _pagedDayView.StretchWidthAndHeight(View);

            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            base.OnViewModelLoadedOverride();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.CurrentDate):
                    if (_pagedDayView.Date != ViewModel.CurrentDate.Date)
                    {
                        _pagedDayView.Date = ViewModel.CurrentDate.Date;
                    }
                    break;
            }
        }

        private void PagedDayView_OnRequestViewClass(object sender, ViewItemClass e)
        {
            ViewModel.ViewClass(e);
        }

        private void PagedDayView_DateChanged(object sender, DateTime e)
        {
            ViewModel.CurrentDate = e;
        }
    }
}