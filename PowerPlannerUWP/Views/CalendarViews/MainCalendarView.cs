using InterfacesUWP;
using InterfacesUWP.CalendarFolder;
using PowerPlannerAppDataLibrary;
using PowerPlannerAppDataLibrary.Extensions;
using PowerPlannerAppDataLibrary.PPEventArgs;
using PowerPlannerAppDataLibrary.ViewItems.BaseViewItems;
using PowerPlannerAppDataLibrary.ViewModels.MainWindow.MainScreen.Calendar;
using PowerPlannerUWPLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;
using ToolsUniversal;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PowerPlannerUWP.Views.CalendarViews
{
    public class MainCalendarView : TCalendarView
    {
        public class ItemSelectedEventArgs : EventArgs
        {
            public BaseViewItemHomeworkExam Item { get; private set; }

            public ItemSelectedEventArgs(BaseViewItemHomeworkExam item)
            {
                Item = item;
            }
        }

        public override bool AutoInitialize
        {
            get
            {
                return false;
            }
        }

        public CalendarViewModel ViewModel { get; private set; }

        public MainCalendarView(CalendarViewModel viewModel)
        {
            ViewModel = viewModel;
            this.SetBinding(DisplayMonthProperty, new Binding()
            {
                Path = new PropertyPath(nameof(ViewModel.DisplayMonth)),
                Source = viewModel,
                Mode = BindingMode.TwoWay
            });
        }

#if DEBUG
        ~MainCalendarView()
        {
            System.Diagnostics.Debug.WriteLine("MainCalendarView disposed");
        }
#endif

        protected override void OnApplyTemplate()
        {
            try
            {
                base.Initialize();
            }

            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        //void calItem_OnClicked(object sender, EventArgs e)
        //{
        //    if (OnItemSelected != null)
        //        OnItemSelected(this, new ItemSelectedEventArgs((sender as MainCalendarViewItem).Item));
        //}

        protected override TCalendarGrid GenerateCalendarGrid(DateTime displayMonth)
        {
            var grid = new MainCalendarGrid(this, displayMonth, IsMouseOver, ViewModel.SemesterItemsViewGroup.Items, ViewModel);
            grid.OnRequestChangeItemDate += Grid_OnRequestChangeItemDate;
            return grid;
        }

        private void Grid_OnRequestChangeItemDate(object sender, ChangeItemDateEventArgs e)
        {
            var dontWait = ViewModel.MoveItem(e.Item, e.DesiredDate);
        }

        protected override void GenerateEventsOnGrid(TCalendarGrid grid, DateTime displayMonth)
        {
            // Nothing
        }
    }
}
