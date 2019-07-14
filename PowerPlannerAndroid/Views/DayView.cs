using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Day;
using InterfacesDroid.Views;
using PowerPlannerAndroid.Views.Controls;
using Android.Support.Design.Widget;
using PowerPlannerAppDataLibrary.Extensions;
using System.ComponentModel;
using ToolsPortable;

namespace PowerPlannerAndroid.Views
{
    public class DayView : InterfacesDroid.Views.PopupViewHost<DayViewModel>
    {
        private DayPagerControl _dayPagerControl;

        public DayView(ViewGroup root) : base(Resource.Layout.Day, root)
        {
        }

        public override void OnViewModelLoadedOverride()
        {
            ViewModel.PropertyChanged += new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;

            var addItemControl = FindViewById<FloatingAddItemControl>(Resource.Id.FloatingAddItemControl);
            addItemControl.OnRequestAddExam += AddItemControl_OnRequestAddExam;
            addItemControl.OnRequestAddHomework += AddItemControl_OnRequestAddHomework;

            _dayPagerControl = FindViewById<DayPagerControl>(Resource.Id.DayPagerControl);
            _dayPagerControl.CurrentDateChanged += _dayPagerControl_CurrentDateChanged;
            _dayPagerControl.Initialize(ViewModel.SemesterItemsViewGroup, ViewModel.CurrentDate);
            _dayPagerControl.ItemClick += _dayPagerControl_ItemClick;
            _dayPagerControl.HolidayItemClick += _dayPagerControl_HolidayItemClick;
            _dayPagerControl.ScheduleItemClick += _dayPagerControl_ScheduleItemClick;
            _dayPagerControl.ScheduleClick += _dayPagerControl_ScheduleClick;
        }

        private void _dayPagerControl_HolidayItemClick(object sender, PowerPlannerAppDataLibrary.ViewItems.ViewItemHoliday e)
        {
            ViewModel.ViewHoliday(e);
        }

        private void _dayPagerControl_ScheduleClick(object sender, EventArgs e)
        {
            ViewModel.ViewSchedule();
        }

        private void _dayPagerControl_ScheduleItemClick(object sender, PowerPlannerAppDataLibrary.ViewItems.ViewItemSchedule e)
        {
            ViewModel.ViewClass(e.Class);
        }

        private void AddItemControl_OnRequestAddHomework(object sender, EventArgs e)
        {
            ViewModel.AddHomework();
        }

        private void AddItemControl_OnRequestAddExam(object sender, EventArgs e)
        {
            ViewModel.AddExam();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.CurrentDate):
                    if (_dayPagerControl.CurrentDate != ViewModel.CurrentDate)
                        _dayPagerControl.Initialize(ViewModel.SemesterItemsViewGroup, ViewModel.CurrentDate);
                    break;
            }
        }

        private void _dayPagerControl_CurrentDateChanged(object sender, DateTime date)
        {
            try
            {
                ViewModel.CurrentDate = date;
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private void _dayPagerControl_ItemClick(object sender, PowerPlannerAppDataLibrary.ViewItems.BaseViewItems.BaseViewItemHomeworkExam e)
        {
            ViewModel.ShowItem(e);
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            ViewModel.AddHomework();
        }
    }
}